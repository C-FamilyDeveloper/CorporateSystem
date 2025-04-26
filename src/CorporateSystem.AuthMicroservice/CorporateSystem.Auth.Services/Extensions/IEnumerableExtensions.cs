namespace CorporateSystem.Auth.Services.Extensions;

internal static class EnumerableExtensions
{
    public static bool IsNotNullAndNotEmpty<T>(this IEnumerable<T>? source)
    {
        return source != null && source.Count() != 0;
    }
}