using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Pyther.Parser.CSV;

public static class TransformMethods
{
    private static readonly Regex rgxAuto = new("[^a-zA-Z0-9]");
    public static string Auto(string s)
    {
        s = rgxAuto.Replace(s, "-");
        s = Regex.Replace(s, @"[-]+", "-");
        s = KebabCaseToTitleCase(s);
        return s;
    }

    public static string ToLower(string s) => s.ToLower();
    public static string ToUpper(string s) => s.ToUpper();

    public static string KebabCaseToTitleCase(string s) => string.Concat(s.Split('-').Select(CultureInfo.InvariantCulture.TextInfo.ToTitleCase));

    public static string SnakeCaseToTitleCase(string s) => string.Concat(s.Split('_').Select(CultureInfo.InvariantCulture.TextInfo.ToTitleCase));

    public static string KebabCaseToCamelCase(string s)
    {
        var result = string.Concat(
            s.Split('-').Select(CultureInfo.InvariantCulture.TextInfo.ToTitleCase)
        );
        return result[..1].ToLower() + result[1..];
    }

    public static string SnakeCaseToCamelCase(string s)
    {
        var result = string.Concat(
            s.Split('_').Select(CultureInfo.InvariantCulture.TextInfo.ToTitleCase)
        );
        return result[..1].ToLower() + result[1..];
    }
}
