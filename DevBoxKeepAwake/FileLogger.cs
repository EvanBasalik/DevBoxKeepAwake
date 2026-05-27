using System.Text;
using System.Globalization;

namespace DevBoxKeepAwake;

internal sealed class FileLogger : IDisposable
{
    private readonly string _logDirectory;
    private readonly Lock _lock = new();
    private int _retentionDays = 7;
    private DateOnly _lastCleanupDate = DateOnly.MinValue;

    public FileLogger(string logDirectory, int retentionDays = 7)
    {
        _logDirectory = logDirectory;
        _retentionDays = Math.Max(0, retentionDays);
        Directory.CreateDirectory(_logDirectory);

        CleanupOldLogsIfNeeded(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public void LogInfo(string message)
    {
        WriteLine("INFO", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        var builder = new StringBuilder(message);
        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        WriteLine("ERROR", builder.ToString());
    }

    public string GetCurrentLogFilePath()
    {
        return Path.Combine(_logDirectory, $"{AppConstants.InternalName}_{DateTime.Now:yyyyMMdd}.log");
    }

    public void SetRetentionDays(int retentionDays)
    {
        lock (_lock)
        {
            _retentionDays = Math.Max(0, retentionDays);
            CleanupOldLogsIfNeeded(DateOnly.FromDateTime(DateTime.UtcNow), force: true);
        }
    }

    public void Dispose()
    {
    }

    private void WriteLine(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

        lock (_lock)
        {
            CleanupOldLogsIfNeeded(DateOnly.FromDateTime(DateTime.UtcNow));
            File.AppendAllText(GetCurrentLogFilePath(), line, Encoding.UTF8);
        }
    }

    private void CleanupOldLogsIfNeeded(DateOnly utcToday, bool force = false)
    {
        if (!force && _lastCleanupDate == utcToday)
        {
            return;
        }

        if (_retentionDays == 0)
        {
            _lastCleanupDate = utcToday;
            return;
        }

        var firstDateToKeep = utcToday.AddDays(-(_retentionDays - 1));
        foreach (var filePath in Directory.EnumerateFiles(_logDirectory, "*.log", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!TryGetLogDate(fileName, out var fileDate))
            {
                continue;
            }

            if (fileDate < firstDateToKeep)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore cleanup failures; logging should never fail because old files cannot be deleted.
                }
            }
        }

        _lastCleanupDate = utcToday;
    }

    private static bool TryGetLogDate(string fileNameWithoutExtension, out DateOnly date)
    {
        const string dateFormat = "yyyyMMdd";

        if (DateOnly.TryParseExact(fileNameWithoutExtension, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        var prefix = AppConstants.InternalName + "_";
        if (!fileNameWithoutExtension.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            date = default;
            return false;
        }

        var datePart = fileNameWithoutExtension[prefix.Length..];
        return DateOnly.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
}