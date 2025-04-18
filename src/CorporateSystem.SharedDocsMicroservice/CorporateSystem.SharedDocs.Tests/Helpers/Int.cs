namespace CorporateSystem.SharedDocs.Tests.Helpers;

internal static class Int
{
    private static int _id = 1;
    
    public static int GetUniqueNumber() => _id++;
}