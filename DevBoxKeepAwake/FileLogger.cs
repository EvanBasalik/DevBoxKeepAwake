using System.Text;

namespace DevBoxKeepAwake;

internal sealed class FileLogger : IDisposable
{
    private readonly string _logDirectory;
    private readonly Lock _lock = new();

    public FileLogger(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
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
        return Path.Combine(_logDirectory, $"{DateTime.Now:yyyyMMdd}.log");
    }

    public void Dispose()
    {
    }

    private void WriteLine(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

        lock (_lock)
        {
            File.AppendAllText(GetCurrentLogFilePath(), line, Encoding.UTF8);
        }
    }
}