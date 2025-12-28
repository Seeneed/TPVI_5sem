using System.IO;

namespace ASPA0011_1.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            lock (_lock)
            {
                var message = $"[{eventId.Id,2}: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel.ToString()}] {formatter(state, exception)}{Environment.NewLine}";
                File.AppendAllText(_filePath, message);
            }
        }
    }
}