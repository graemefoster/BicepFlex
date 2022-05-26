using System.Globalization;

namespace BicepParser;

public static class StringEx
{
    public static string ToPascalCase(this string fileName)
    {
        var info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(fileName)
            .Replace("-", "")
            .Replace(" ", "");
    }
}