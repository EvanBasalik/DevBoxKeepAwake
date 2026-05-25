namespace DevBoxKeepAwake;

internal sealed class AppSettings
{
    public bool AutoStart { get; set; } = true;

    public int MousePollSeconds { get; set; } = 5;

    public int ActivityEvaluationMinutes { get; set; } = 5;

    public List<KeepAliveTargetSettings> Targets { get; set; } = [CreateDefaultPythonTarget()];

    internal static KeepAliveTargetSettings CreateDefaultPythonTarget()
    {
        return new KeepAliveTargetSettings
        {
            Name = "Python",
            FileName = "python.exe",
            Arguments = "-c \"import time; time.sleep(86400)\"",
            ProcessName = "python",
            CreateNoWindow = true,
            Enabled = true,
        };
    }
}