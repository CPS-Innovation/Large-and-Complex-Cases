namespace CPS.ComplexCases.Common.Extensions;

public static class StringExtensions
{
    public static string? Unquote(this string value)
    {
        return value?.Trim().Trim('"');
    }
}