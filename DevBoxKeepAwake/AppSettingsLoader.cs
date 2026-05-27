using System.Text.Json;

namespace DevBoxKeepAwake;

internal static class AppSettingsLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static AppSettings Load(string configPath, FileLogger logger)
    {
        try
        {
            if (!File.Exists(configPath))
            {
                var defaultSettings = new AppSettings();
                TryCreateDefaultConfigFile(configPath, defaultSettings, logger);
                return defaultSettings;
            }

            var json = File.ReadAllText(configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();

            settings.MousePollSeconds = Math.Max(1, settings.MousePollSeconds);
            settings.ActivityEvaluationMinutes = Math.Max(1, settings.ActivityEvaluationMinutes);
            settings.LogRetentionDays = Math.Max(0, settings.LogRetentionDays);
            settings.Targets = settings.Targets
                .Where(target => !string.IsNullOrWhiteSpace(target.FileName))
                .ToList();

            if (settings.Targets.Count == 0)
            {
                settings.Targets.Add(AppSettings.CreateDefaultPythonTarget());
            }

            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to load configuration. Falling back to defaults.", ex);
            return new AppSettings();
        }
    }

    private static void TryCreateDefaultConfigFile(string configPath, AppSettings defaultSettings, FileLogger logger)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            File.WriteAllText(configPath, JsonSerializer.Serialize(defaultSettings, JsonOptions));
            logger.LogInfo("Created default appsettings.json file.");
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to create default appsettings.json. Continuing with in-memory defaults.", ex);
        }
    }
}