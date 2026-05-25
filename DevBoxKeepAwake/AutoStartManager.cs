using Microsoft.Win32;

namespace DevBoxKeepAwake;

internal static class AutoStartManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string LegacyInternalName = "DevBoxKeepAlive";

    public static void Apply(bool enabled, FileLogger logger)
    {
        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

            if (enabled)
            {
                var processPath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(processPath))
                {
                    logger.LogError("Unable to determine current executable path for autostart registration.");
                    return;
                }

                runKey.SetValue(AppConstants.InternalName, $"\"{processPath}\"");
                if (runKey.GetValue(LegacyInternalName) is not null)
                {
                    runKey.DeleteValue(LegacyInternalName, throwOnMissingValue: false);
                    logger.LogInfo("Removed legacy autostart registration for DevBoxKeepAlive.");
                }

                logger.LogInfo("Autostart registration ensured.");
                return;
            }

            if (runKey.GetValue(AppConstants.InternalName) is not null)
            {
                runKey.DeleteValue(AppConstants.InternalName, throwOnMissingValue: false);
                logger.LogInfo("Autostart registration removed.");
            }

            if (runKey.GetValue(LegacyInternalName) is not null)
            {
                runKey.DeleteValue(LegacyInternalName, throwOnMissingValue: false);
                logger.LogInfo("Removed legacy autostart registration for DevBoxKeepAlive.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to update autostart registration.", ex);
        }
    }
}