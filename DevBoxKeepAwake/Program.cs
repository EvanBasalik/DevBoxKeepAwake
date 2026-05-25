namespace DevBoxKeepAwake;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configPath = AppPaths.ConfigFilePath;

        using var logger = new FileLogger(AppPaths.LogDirectory);
        var settings = AppSettingsLoader.Load(configPath, logger);

        logger.LogInfo($"Starting {AppConstants.DisplayName}.");
        logger.LogInfo($"Configuration path: {configPath}");
        logger.LogInfo($"Loaded {settings.Targets.Count} keepalive target(s).");

        Application.Run(new TrayApplicationContext(settings, configPath, logger));
    }
}