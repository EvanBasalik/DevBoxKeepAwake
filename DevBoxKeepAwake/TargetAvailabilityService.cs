namespace DevBoxKeepAwake;

internal static class TargetAvailabilityService
{
    internal readonly record struct MissingTarget(string Name, string FileName, bool IsPython);

    public static bool IsPythonAvailable()
    {
        return IsExecutableAvailable("python.exe", out _);
    }

    public static bool IsPythonTarget(KeepAliveTargetSettings target)
    {
        var fileName = Path.GetFileName(target.FileName).Trim().ToLowerInvariant();
        return fileName is "python" or "python.exe";
    }

    public static bool IsTargetAvailable(KeepAliveTargetSettings target)
    {
        return IsExecutableAvailable(target.FileName, out _);
    }

    public static List<MissingTarget> GetMissingTargets(IEnumerable<KeepAliveTargetSettings> targets)
    {
        var missing = new List<MissingTarget>();
        foreach (var target in targets)
        {
            if (IsExecutableAvailable(target.FileName, out _))
            {
                continue;
            }

            missing.Add(new MissingTarget(target.Name, target.FileName, IsPythonTarget(target)));
        }

        return missing;
    }

    public static bool IsExecutableAvailable(string fileName, out string? resolvedPath)
    {
        resolvedPath = null;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var candidate = fileName.Trim();
        if (candidate.Contains(Path.DirectorySeparatorChar) || candidate.Contains(Path.AltDirectorySeparatorChar) || Path.IsPathRooted(candidate))
        {
            if (!File.Exists(candidate))
            {
                return false;
            }

            resolvedPath = Path.GetFullPath(candidate);
            return true;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var directories = pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var pathext = Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT;.COM";
        var extensions = pathext.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var hasExtension = Path.HasExtension(candidate);
        foreach (var dir in directories)
        {
            if (!hasExtension)
            {
                foreach (var ext in extensions)
                {
                    var path = Path.Combine(dir, candidate + ext.ToLowerInvariant());
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    resolvedPath = path;
                    return true;
                }
            }

            var directPath = Path.Combine(dir, candidate);
            if (!File.Exists(directPath))
            {
                continue;
            }

            resolvedPath = directPath;
            return true;
        }

        return false;
    }
}