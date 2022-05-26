using System.Globalization;

namespace BicepFlex;

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