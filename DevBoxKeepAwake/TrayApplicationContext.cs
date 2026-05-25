using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

namespace DevBoxKeepAwake;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly AppSettings _settings;
    private readonly string _configPath;
    private readonly FileLogger _logger;
    private readonly KeepAliveManager _keepAliveManager;
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;

    private NativePoint _lastCursorPosition;
    private DateTimeOffset _lastEvaluation = DateTimeOffset.UtcNow;
    private bool _mouseMovedSinceLastEvaluation;
    private bool _evaluationInProgress;
    private bool _pythonInstallPrompted;

    public TrayApplicationContext(AppSettings settings, string configPath, FileLogger logger)
    {
        _settings = settings;
        _configPath = configPath;
        _logger = logger;
        _keepAliveManager = new KeepAliveManager(_settings.Targets, _logger);

        AutoStartManager.Apply(_settings.AutoStart, _logger);
        _lastCursorPosition = GetCursorPosition();

        _notifyIcon = new NotifyIcon
        {
            Text = AppConstants.DisplayName,
            Icon = LoadAppIcon(_logger),
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };

        _timer = new System.Windows.Forms.Timer
        {
            Interval = _settings.MousePollSeconds * 1000,
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        ValidateConfiguredTargetsOnStartup();

        _logger.LogInfo("Starting configured keepalive targets during application startup.");
        _keepAliveManager.EnsureTargetsRunning();

        _logger.LogInfo("Tray application initialized.");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer.Dispose();

            _keepAliveManager.DisposeOwnedTargets();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var currentPosition = GetCursorPosition();
        if (currentPosition != _lastCursorPosition)
        {
            _mouseMovedSinceLastEvaluation = true;
            _lastCursorPosition = currentPosition;
        }

        var elapsed = DateTimeOffset.UtcNow - _lastEvaluation;
        if (elapsed < TimeSpan.FromMinutes(_settings.ActivityEvaluationMinutes) || _evaluationInProgress)
        {
            return;
        }

        _evaluationInProgress = true;
        try
        {
            if (_mouseMovedSinceLastEvaluation)
            {
                _logger.LogInfo("Mouse activity detected during evaluation window. Ensuring keepalive targets are running.");
                _keepAliveManager.EnsureTargetsRunning();
                _notifyIcon.Text = Localization.Format("NotifyIconStatusFormat", AppConstants.DisplayName, Localization.Text("StatusActive"));
            }
            else
            {
                _logger.LogInfo("No mouse activity detected during evaluation window. Stopping owned keepalive targets.");
                _keepAliveManager.StopOwnedTargets();
                _notifyIcon.Text = Localization.Format("NotifyIconStatusFormat", AppConstants.DisplayName, Localization.Text("StatusIdle"));
            }

            _lastEvaluation = DateTimeOffset.UtcNow;
            _mouseMovedSinceLastEvaluation = false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error during activity evaluation.", ex);
        }
        finally
        {
            _evaluationInProgress = false;
        }
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add(Localization.Text("MenuSettings"), null, (_, _) => ShowSettingsForm());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Localization.Text("MenuOpenConfig"), null, (_, _) => OpenWithShell(_configPath));
        menu.Items.Add(Localization.Text("MenuOpenLog"), null, (_, _) => OpenWithShell(_logger.GetCurrentLogFilePath()));
        menu.Items.Add(Localization.Text("MenuOpenLogsFolder"), null, (_, _) => OpenWithShell(AppPaths.LogDirectory));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Localization.Text("MenuExit"), null, (_, _) => ExitApplication());

        return menu;
    }

    private void ShowSettingsForm()
    {
        try
        {
            using var form = new SettingsForm(_settings, _configPath, _logger, SaveSettingsAsync);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ApplySettings();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error opening settings form.", ex);
        }
    }

    private async Task SaveSettingsAsync(AppSettings settings, string configPath)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };

            var json = JsonSerializer.Serialize(settings, jsonOptions);
            await File.WriteAllTextAsync(configPath, json);
            _logger.LogInfo("Settings persisted to configuration file.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save settings to configuration file.", ex);
            throw;
        }
    }

    private void ApplySettings()
    {
        try
        {
            var oldPollInterval = _timer.Interval;
            var newPollInterval = _settings.MousePollSeconds * 1000;

            if (oldPollInterval != newPollInterval)
            {
                _timer.Interval = newPollInterval;
                _logger.LogInfo($"Updated mouse poll interval from {oldPollInterval}ms to {newPollInterval}ms.");
            }

            _keepAliveManager.DisposeOwnedTargets();
            ValidateConfiguredTargetsOnStartup();
            _logger.LogInfo("Settings applied. Keepalive manager updated with new targets.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error applying settings.", ex);
        }
    }

    private void ValidateConfiguredTargetsOnStartup()
    {
        var missing = TargetAvailabilityService.GetMissingTargets(_settings.Targets);
        var missingPython = missing.Any(target => target.IsPython);

        if (missingPython && !_pythonInstallPrompted)
        {
            _pythonInstallPrompted = true;
            if (PythonInstaller.EnsurePythonInstalledWithPrompt(null, _logger))
            {
                missing = TargetAvailabilityService.GetMissingTargets(_settings.Targets);
            }
        }

        if (missing.Count == 0)
        {
            return;
        }

        var summary = string.Join(", ", missing.Select(target => $"{target.Name} ({target.FileName})"));
        _logger.LogError($"Unavailable targets on startup: {summary}");
        MessageBox.Show(
            Localization.Format("TargetsUnavailableDialogFormat", summary),
            Localization.Text("TargetAvailabilityCaption"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private static Icon LoadAppIcon(FileLogger logger)
    {
        return AppIconProvider.Load(logger);
    }

    private void ExitApplication()
    {
        _logger.LogInfo("Exit requested from tray icon.");
        ExitThread();
    }

    private static NativePoint GetCursorPosition()
    {
        return NativeMethods.GetCursorPos(out var point) ? point : new NativePoint(0, 0);
    }

    private void OpenWithShell(string path)
    {
        try
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                _logger.LogInfo($"Requested path does not exist yet: {path}");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to open '{path}'.", ex);
        }
    }
}