using System.Security.Cryptography;

namespace CorporateSystem.SharedDocs.Tests.Helpers;

internal static class StringHelper
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string GetUniqueString(int length = 10)
    {
        return new string(
            Enumerable.Repeat(Chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)])
                .ToArray());
    }
}