namespace CorporateSystem.SharedDocs.Kafka.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<IReadOnlyList<T>> Buffer<T>(
        this IAsyncEnumerable<T> src,
        int count,
        TimeSpan delay = default)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var buffer = new List<T>();
        await foreach (var item in src)
        {
            buffer.Add(item);

            if (buffer.Count >= count)
            {
                yield return buffer.AsReadOnly();
                buffer.Clear();
            }

            if (delay != TimeSpan.Zero && buffer.Count < count)
            {
                await Task.Delay(delay);
            }
        }

        yield return buffer.AsReadOnly();
    }
}