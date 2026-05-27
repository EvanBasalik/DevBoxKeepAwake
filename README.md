# DevBox Keep Awake

DevBox Keep Awake is a .NET 10 Windows tray application that keeps one or more configured processes alive while mouse activity is detected. This is helpful when the owners of the DevBox infrastructure are watching for those processes to decide whether or not to suspend or shutdown the DevBox instance.

## Behavior

- Registers itself for current-user Windows startup when `AutoStart` is enabled.
- Samples mouse position every `MousePollSeconds`.
- Evaluates activity every `ActivityEvaluationMinutes`.
- If mouse activity was detected during the evaluation window, it ensures each enabled configured target has at least one matching process running.
- If no mouse activity was detected, it terminates only the processes that DevBox Keep Awake started itself.
- Creates `%LocalAppData%\DevBox Keep Awake\appsettings.json` on first run when no config exists, and still runs with built-in defaults if that file cannot be created.
- Writes daily log files under `%LocalAppData%\DevBox Keep Awake\logs`.
- Provides a GUI via the system tray icon to view and manage targets, configure timings, and access logs and configuration.

## Usage

Right-click the tray icon to access:

- **Settings**: Opens a dialog to view/add/edit/delete keepalive targets and adjust polling/evaluation timings.
- **Open config**: Opens the configuration file in the default text editor.
- **Open log**: Opens today's log file.
- **Open logs folder**: Opens the logs directory.
- **Exit**: Exits the application.

## Configuration

Edit `%LocalAppData%\DevBox Keep Awake\appsettings.json`.

Example:

```json
{
  "AutoStart": true,
  "MousePollSeconds": 5,
  "ActivityEvaluationMinutes": 5,
  "LogRetentionDays": 7,
  "Targets": [
    {
      "Name": "Python",
      "FileName": "python.exe",
      "Arguments": "-c \"import time; time.sleep(86400)\"",
      "ProcessName": "python",
      "WorkingDirectory": null,
      "CreateNoWindow": true,
      "Enabled": true
    }
  ]
}
```

`LogRetentionDays` controls how many daily log files are kept in the logs directory. Set it to `0` to disable automatic cleanup.

The example uses Python for simplicity, but `Targets` can point to any executable. `ProcessName` should match the name Windows shows for the running process without `.exe`.

## Build

```powershell
dotnet build .\DevBoxKeepAwake\DevBoxKeepAwake.csproj
```

## Publish

```powershell
dotnet publish .\DevBoxKeepAwake\DevBoxKeepAwake.csproj -c Release -r win-x64 --self-contained false
```

The publish output is configured for single-file deployment.
