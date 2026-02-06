using Microsoft.Extensions.Logging;

namespace MySandbox.Tests;

public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}
