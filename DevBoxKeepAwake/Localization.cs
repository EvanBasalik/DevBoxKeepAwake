using System.Globalization;
using System.Resources;

namespace DevBoxKeepAwake;

internal static class Localization
{
    private static readonly ResourceManager ResourceManager = new("DevBoxKeepAwake.Resources.Strings", typeof(Localization).Assembly);

    public static string Text(string key)
    {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
            ?? ResourceManager.GetString(key, CultureInfo.InvariantCulture)
            ?? key;
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, Text(key), args);
    }
}