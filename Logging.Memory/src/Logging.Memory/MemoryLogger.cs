namespace iflight.Logging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;

    public class MemoryLogger : ILogger
    {
        private const int _indentation = 2;
        private readonly string _name;
        private static object _lock = new object();
        private Func<string, LogLevel, bool> _filter;
        private static int logListStartIndex = 0;

        /// <summary>
        /// Максимальное кол-во записей в логе
        /// </summary>
        /// <remarks>
        /// После обновления - изменения будут учтены при следующей записи в список (при добавлении следующей записи).
        /// </remarks>
        public static int MaxLogCount = 200;

        private static List<string> logList = new List<string>(MaxLogCount);

        public static List<string> LogList
        {
            get
            {
                List<string> list = new List<string>();
                if (logList.Count < MaxLogCount)
                {
                    list = logList.GetRange(0, logList.Count);
                }
                else
                {
                    list = new List<string>(logList.GetRange(logListStartIndex, MaxLogCount - logListStartIndex));
                    var range = logList.GetRange(0, logListStartIndex);
                    list.AddRange(range);
                }
                return list;
            }
        }

        public MemoryLogger(string name, Func<string, LogLevel, bool> filter, int maxLogCount)
        {
            _name = name;
            _filter = filter ?? ((category, logLevel) => true);
            MaxLogCount = maxLogCount;
        }

        public string Name { get { return _name; } }

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
            // var values = state as ILogValues;

            if (!string.IsNullOrEmpty(message))
            {
                lock (_lock)
                {
                    if (logList.Count < MaxLogCount)
                    {
                        logList.Add(FormatMessage(logLevel, _name, message));
                    }
                    else
                    {
                        logList[logListStartIndex] = FormatMessage(logLevel, _name, message);
                    }
                    if (logListStartIndex < MaxLogCount - 1)
                    {
                        logListStartIndex++;
                    }
                    else
                    {
                        logListStartIndex = 0;
                    }
                }

            }

            if (exception != null)
            {
              
                lock (_lock)
                {
                    if (logList.Count < MaxLogCount)
                    {
                        logList.Add(FormatMessage(logLevel, _name, exception.Message));
                    }
                    else
                    {
                        logList[logListStartIndex] = FormatMessage(logLevel, _name, exception.Message);
                    }
                    if (logListStartIndex < MaxLogCount - 1)
                    {
                        logListStartIndex++;
                    }
                    else
                    {
                        logListStartIndex = 0;
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
            return _filter(_name, logLevel);
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
