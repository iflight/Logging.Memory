namespace iflight.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// In-memory logger
    /// </summary>
    public class MemoryLogger : ILogger
    {
        private static object _lock = new object();
        private Func<string, LogLevel, bool> _filter;

        /// <summary>
        /// Max count of stored log lines
        /// </summary>
        public static int MaxLogCount = 200;

        private static readonly Dictionary<LogLevel, LogLevelLog> logsDictionary = new Dictionary<LogLevel, LogLevelLog>();

        /// <summary>
        /// Last <see cref="MaxLogCount"/> log lines
        /// </summary>
        public static List<string> LogList
        {
            get
            {
                return logsDictionary
                            .SelectMany(x => x.Value.logList)
                            .OrderByDescending(x => x.Item2)
                            .Take(MaxLogCount)
                            .Select(x => x.Item2)
                            .Reverse() // keep asc sort like in 1st version
                            .ToList();
            }
        }

        /// <summary>
        /// Return last <see cref="MaxLogCount"/> log lines with specified logLevel
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public static List<string> GetLog(LogLevel logLevel)
        {
            if (logsDictionary.TryGetValue(logLevel, out var log))
            {
                return log.logList.Select(x => x.Item2).ToList();
            }
            else
            {
                return Enumerable.Empty<string>().ToList();
            }
        }

        static MemoryLogger()
        {
            foreach (var l in (LogLevel[])Enum.GetValues(typeof(LogLevel)))
            {
                if (l != LogLevel.None)
                    logsDictionary.Add(l, new LogLevelLog(MaxLogCount));
            }
        }

        public MemoryLogger(string name, Func<string, LogLevel, bool> filter, int maxLogCount)
        {
            Name = name;
            _filter = filter ?? ((category, logLevel) => true);
            MaxLogCount = maxLogCount;
        }

        public string Name { get; private set; }

        public Func<string, LogLevel, bool> Filter
        {
            get { return _filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _filter = value;
            }
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (logsDictionary.TryGetValue(logLevel, out var currentLog))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    lock (_lock)
                    {
                        if (currentLog.logList.Count < MaxLogCount)
                        {
                            currentLog.logList.Add(new Tuple<DateTime, string>(DateTime.Now, FormatMessage(logLevel, Name, message)));
                        }
                        else
                        {
                            currentLog.logList[currentLog.currentLogIndex] = new Tuple<DateTime, string>(DateTime.Now, FormatMessage(logLevel, Name, message));
                        }

                        if (currentLog.currentLogIndex < MaxLogCount - 1)
                        {
                            currentLog.currentLogIndex++;
                        }
                        else
                        {
                            currentLog.currentLogIndex = 0;
                        }
                    }

                }

                if (exception != null)
                {

                    lock (_lock)
                    {
                        if (currentLog.logList.Count < MaxLogCount)
                        {
                            currentLog.logList.Add(new Tuple<DateTime, string>(DateTime.Now, FormatMessage(logLevel, Name, exception.Message)));
                        }
                        else
                        {
                            currentLog.logList[currentLog.currentLogIndex] = new Tuple<DateTime, string>(DateTime.Now, FormatMessage(logLevel, Name, exception.Message));
                        }
                        if (currentLog.currentLogIndex < MaxLogCount - 1)
                        {
                            currentLog.currentLogIndex++;
                        }
                        else
                        {
                            currentLog.currentLogIndex = 0;
                        }
                    }
                }
            }
        }

        public virtual string FormatMessage(LogLevel logLevel, string logName, string message)
        {
            var logLevelString = GetRightPaddedLogLevelString(logLevel);
            return $"{DateTime.Now.ToString("HH:mm:ss,fff")} - {logLevelString}: [{logName}] {message}";
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(Name, logLevel);
        }

        private static string GetRightPaddedLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "TRACE   ";
                case LogLevel.Debug:
                    return "DEBUG   ";
                case LogLevel.Information:
                    return "INFO    ";
                case LogLevel.Warning:
                    return "WARNING ";
                case LogLevel.Error:
                    return "ERROR   ";
                case LogLevel.Critical:
                    return "CRITICAL";
                default:
                    return "UNKNOWN ";
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return MemoryLogScope.Push(Name, state);
        }
    }
}
