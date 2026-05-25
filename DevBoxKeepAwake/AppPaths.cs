namespace DevBoxKeepAwake;

internal static class AppPaths
{
    private static readonly string AppDataRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppConstants.InternalName);

    public static string AppDataDirectory => AppDataRoot;

    public static string ConfigFilePath => Path.Combine(AppDataRoot, "appsettings.json");

    public static string LogDirectory => Path.Combine(AppDataRoot, "logs");
}