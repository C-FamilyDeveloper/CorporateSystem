using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class BanWordsService : IBanWordsService
{
    private readonly TrieNode _root;
    private readonly IWordsFormatter _wordsFormatter;
    private readonly ILogger<BanWordsService> _logger;

    internal class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public bool IsEndOfWord { get; set; }
    }
    
    public BanWordsService(
        [FromKeyedServices(nameof(HtmlWordFormatter))] IWordsFormatter wordsFormatter,
        ILogger<BanWordsService> logger)
    {
        _wordsFormatter = wordsFormatter;
        _logger = logger;
        _root = new TrieNode();
        var banWords = LoadBanWordsFromFile().ToArray();
        _logger.LogInformation($"{nameof(BanWordsService)}: loaded {banWords.Length} words");
        foreach (var word in banWords)
        {
            InsertWordIntoTrie(word);
        }
    }

    public async Task<string> ProcessTextAsync(string content, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var processedWords = new List<string>();

        _logger.LogInformation($"{nameof(ProcessTextAsync)}: content={content}");
        
        foreach (var word in words)
        {
            if (SearchInTrie(word))
            {
                _logger.LogInformation($"{nameof(ProcessTextAsync)}: word={word} is banned");
                processedWords.Add(await _wordsFormatter.FormatWordAsync(word, cancellationToken));
            }
            else
            {
                processedWords.Add(word);
            }
        }

        return string.Join(" ", processedWords);
    }

    private IEnumerable<string> LoadBanWordsFromFile()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ban_words.txt");

        _logger.LogInformation($"{nameof(LoadBanWordsFromFile)}: filepath={filePath}");
        if (!File.Exists(filePath))
        {
            _logger.LogInformation($"{nameof(LoadBanWordsFromFile)}: file is empty");
            return [];
        }
        
        return File.ReadAllLines(filePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line));
    }

    private void InsertWordIntoTrie(string word)
    {
        var node = _root;
        foreach (var ch in word.ToLower())
        {
            if (!node.Children.ContainsKey(ch))
            {
                node.Children[ch] = new TrieNode();
            }
            
            node = node.Children[ch];
        }
        
        node.IsEndOfWord = true;
    }

    private bool SearchInTrie(string word)
    {
        var node = _root;

        var lowerWord = word.ToLower();

        for (var i = 0; i < lowerWord.Length; i++)
        {
            if (lowerWord[i] == '<' && i == 0)
            {
                while (lowerWord[i] != '>')
                {
                    i++;
                }

                i++;
            }
            
            var ch = lowerWord[i];
            if (ch == '<')
                break;
            
            if (!node.Children.TryGetValue(ch, out var child))
            {
                return false;
            }
            
            node = child;
        }
        
        return node.IsEndOfWord;
    }
}