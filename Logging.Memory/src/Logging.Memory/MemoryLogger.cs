namespace Logging.Memory
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
        private static object lockObj = new object();

        private Func<string, LogLevel, bool> filter;

        private Func<LogLevel, string, string, Exception, string> logLineFormatter = null;

        private static readonly Dictionary<LogLevel, LogForLevel> logsDictionary = new Dictionary<LogLevel, LogForLevel>();

        /// <summary>
        /// Max count of stored log lines
        /// </summary>
        public static int MaxLogCount = 200;

        /// <summary>
        /// Logger name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Logger filter
        /// </summary>
        public Func<string, LogLevel, bool> Filter
        {
            get { return this.filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.filter = value;
            }
        }

        /// <summary>
        /// Last <see cref="MaxLogCount"/> log lines
        /// </summary>
        public static List<string> LogList
        {
            get
            {
                return logsDictionary
                            .SelectMany(x => x.Value.logList)
                            .OrderByDescending(x => x.Item1)
                            .Take(MaxLogCount)
                            .Select(x => x.Item2)
                            .Reverse() // keep asc sort like in 1st version
                            .ToList();
            }
        }

        /// <summary>
        /// Return last <see cref="MaxLogCount"/> log lines with specified logLevel
        /// </summary>
        /// <param name="logLevel">log level for return</param>
        /// <returns></returns>
        public static List<string> GetLog(LogLevel logLevel)
        {
            if (logsDictionary.TryGetValue(logLevel, out var log))
            {
                return log.logList.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
            }
            else
            {
                return Enumerable.Empty<string>().ToList();
            }
        }

        /// <summary>
        /// Return log lines with logLevel more or equal than <paramref name="minLogLevel"/>
        /// </summary>
        /// <param name="minLogLevel">Min log level</param>
        /// <returns>List of log lines</returns>
        public static List<string> GetLogGte(LogLevel minLogLevel)
        {
            return logsDictionary.Where(x => x.Key >= minLogLevel)
                        .SelectMany(x => x.Value.logList)
                            .OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
        }

        /// <summary>
        /// Return log lines with logLevel less or equal than <paramref name="maxLogLevel"/>
        /// </summary>
        /// <param name="maxLogLevel">Max log level</param>
        /// <returns>List of log lines</returns>
        public static List<string> GetLogLte(LogLevel maxLogLevel)
        {
            return logsDictionary.Where(x => x.Key <= maxLogLevel)
                        .SelectMany(x => x.Value.logList)
                            .OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
        }

        static MemoryLogger()
        {
            foreach (var level in ((LogLevel[])Enum.GetValues(typeof(LogLevel))).Where(x => x != LogLevel.None))
            {
                logsDictionary.Add(level, new LogForLevel(MaxLogCount));
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="name">logger name</param>
        /// <param name="filter">filter log entities</param>
        /// <param name="maxLogCount">max count of stored lines of log (for each level)</param>
        /// <param name="logLineFormatter">string formatter for log line</param>
        public MemoryLogger(string name, Func<string, LogLevel, bool> filter, 
                                int maxLogCount, 
                                Func<LogLevel, string, string, Exception, string> logLineFormatter)
        {
            Name = name;
            this.filter = filter ?? ((category, logLevel) => true);
            MaxLogCount = maxLogCount;
            this.logLineFormatter = logLineFormatter ?? DefaultLogLineFormatter.Formatter;
        }

        #region ILog implementation
        /// <inheritdoc />
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
                    var preparedMessage = this.logLineFormatter(logLevel, Name, message, exception);
                    lock (lockObj)
                    {
                        if (currentLog.logList.Count < MaxLogCount)
                        {
                            currentLog.logList.Add(new Tuple<DateTime, string>(DateTime.Now, preparedMessage));
                        }
                        else
                        {
                            currentLog.logList[currentLog.currentLogIndex] = new Tuple<DateTime, string>(DateTime.Now, preparedMessage);
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

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return this.filter(Name, logLevel);
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return MemoryLogScope.Push(Name, state);
        }
        #endregion
    }
}
