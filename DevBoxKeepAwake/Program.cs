namespace DevBoxKeepAwake;

internal static class Program
{
    private const string SingleInstanceMutexName = "Local\\DevBoxKeepAwake.SingleInstance";
    private const string OpenSettingsSignalName = "Local\\DevBoxKeepAwake.OpenSettings";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var openSettingsSignal = new EventWaitHandle(false, EventResetMode.AutoReset, OpenSettingsSignalName);

        using var singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            openSettingsSignal.Set();
            return;
        }

        var configPath = AppPaths.ConfigFilePath;

        using var logger = new FileLogger(AppPaths.LogDirectory);
        var settings = AppSettingsLoader.Load(configPath, logger);
        logger.SetRetentionDays(settings.LogRetentionDays);

        logger.LogInfo($"Starting {AppConstants.DisplayName}.");
        logger.LogInfo($"Configuration path: {configPath}");
        logger.LogInfo($"Loaded {settings.Targets.Count} keepalive target(s).");

        using var trayContext = new TrayApplicationContext(settings, configPath, logger);
        var openSettingsRegistration = ThreadPool.RegisterWaitForSingleObject(
            openSettingsSignal,
            (_, _) => trayContext.OpenSettingsFromExternalRequest(),
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);

        Application.Run(trayContext);
        openSettingsRegistration.Unregister(null);
    }
}