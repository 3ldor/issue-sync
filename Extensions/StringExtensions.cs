namespace IssueSync.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        return value.Substring(0, Math.Min(value.Length, maxLength));
    }
}