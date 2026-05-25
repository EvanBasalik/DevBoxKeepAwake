using System.Drawing;
using System.Reflection;

namespace DevBoxKeepAwake;

internal static class AppIconProvider
{
    public static Icon Load(FileLogger? logger = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(".app.ico", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(resourceName))
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is not null)
                {
                    return new Icon(stream);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError($"Failed to load embedded icon resource '{resourceName}'.", ex);
            }
        }
        else
        {
            logger?.LogError("Embedded icon resource was not found.");
        }

        var iconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        try
        {
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError($"Failed to load icon from '{iconPath}' as ICO. Attempting bitmap fallback.", ex);
            try
            {
                using var bitmap = new Bitmap(iconPath);
                var iconHandle = bitmap.GetHicon();
                try
                {
                    using var unsafeIcon = Icon.FromHandle(iconHandle);
                    return (Icon)unsafeIcon.Clone();
                }
                finally
                {
                    NativeMethods.DestroyIcon(iconHandle);
                }
            }
            catch (Exception innerEx)
            {
                logger?.LogError("Failed to load app icon from bitmap fallback. Using system fallback icon.", innerEx);
            }
        }

        if (!File.Exists(iconPath))
        {
            logger?.LogInfo($"App icon not found at '{iconPath}'. Using system fallback icon.");
        }

        return SystemIcons.Application;
    }
}