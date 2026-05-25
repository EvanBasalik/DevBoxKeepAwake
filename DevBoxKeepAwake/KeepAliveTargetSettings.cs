namespace DevBoxKeepAwake;

internal sealed class KeepAliveTargetSettings
{
    public string Name { get; set; } = "KeepAlive Target";

    public string FileName { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    public string? WorkingDirectory { get; set; }

    public string? ProcessName { get; set; }

    public bool CreateNoWindow { get; set; } = true;

    public bool Enabled { get; set; } = true;

    public string GetEffectiveProcessName()
    {
        if (!string.IsNullOrWhiteSpace(ProcessName))
        {
            return ProcessName.Trim();
        }

        return Path.GetFileNameWithoutExtension(FileName);
    }
}