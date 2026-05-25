using System.Diagnostics;

namespace DevBoxKeepAwake;

internal sealed class KeepAliveManager
{
    private readonly IReadOnlyList<KeepAliveTargetSettings> _targets;
    private readonly FileLogger _logger;
    private readonly Dictionary<string, Process> _ownedProcesses = new(StringComparer.OrdinalIgnoreCase);

    public KeepAliveManager(IReadOnlyList<KeepAliveTargetSettings> targets, FileLogger logger)
    {
        _targets = targets;
        _logger = logger;
    }

    public void EnsureTargetsRunning()
    {
        foreach (var target in _targets.Where(target => target.Enabled))
        {
            TryClearExitedOwnedProcess(target.Name);

            if (_ownedProcesses.ContainsKey(target.Name))
            {
                continue;
            }

            var processName = target.GetEffectiveProcessName();
            if (Process.GetProcessesByName(processName).Length > 0)
            {
                _logger.LogInfo($"{target.Name}: matching process '{processName}' already running; no new process started.");
                continue;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = target.FileName,
                    Arguments = target.Arguments,
                    WorkingDirectory = GetWorkingDirectory(target),
                    UseShellExecute = false,
                    CreateNoWindow = target.CreateNoWindow,
                };

                var process = Process.Start(startInfo);
                if (process is null)
                {
                    _logger.LogError($"{target.Name}: Process.Start returned null.");
                    continue;
                }

                _ownedProcesses[target.Name] = process;
                _logger.LogInfo($"{target.Name}: started PID {process.Id} using '{target.FileName} {target.Arguments}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{target.Name}: failed to start keepalive process.", ex);
            }
        }
    }

    public void StopOwnedTargets()
    {
        foreach (var target in _targets.Where(target => target.Enabled))
        {
            if (!_ownedProcesses.TryGetValue(target.Name, out var process))
            {
                continue;
            }

            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(5000);
                    _logger.LogInfo($"{target.Name}: stopped owned PID {process.Id}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{target.Name}: failed to stop owned process.", ex);
            }
            finally
            {
                process.Dispose();
                _ownedProcesses.Remove(target.Name);
            }
        }
    }

    public void DisposeOwnedTargets()
    {
        StopOwnedTargets();
    }

    private void TryClearExitedOwnedProcess(string targetName)
    {
        if (!_ownedProcesses.TryGetValue(targetName, out var process))
        {
            return;
        }

        if (!process.HasExited)
        {
            return;
        }

        _logger.LogInfo($"{targetName}: owned PID {process.Id} exited on its own.");
        process.Dispose();
        _ownedProcesses.Remove(targetName);
    }

    private static string GetWorkingDirectory(KeepAliveTargetSettings target)
    {
        if (!string.IsNullOrWhiteSpace(target.WorkingDirectory))
        {
            return target.WorkingDirectory;
        }

        if (Path.IsPathRooted(target.FileName))
        {
            return Path.GetDirectoryName(target.FileName) ?? AppContext.BaseDirectory;
        }

        return AppContext.BaseDirectory;
    }
}