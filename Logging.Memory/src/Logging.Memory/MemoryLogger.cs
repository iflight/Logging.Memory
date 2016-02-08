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

        private static List<string> logList = new List<string>();

        public static List<string> LogList
        {
            get { return logList; }
        }

        /// <summary>
        /// Максимальное кол-во записей в логе
        /// </summary>
        /// <remarks>
        /// После обновления - изменения будут учтены при следующей записи в список (при добавлении следующей записи).
        /// </remarks>

        public MemoryLogger(string name, Func<string, LogLevel, bool> filter, int maxLogCount)
        {
            _name = name;
            _filter = filter ?? ((category, logLevel) => true);
            MaxLogCount = maxLogCount;
        }

        public string Name { get { return _name; } }

        public bool IncludeScopes { get; set; }

        public int MaxLogCount { get; set; }

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


        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var message = string.Empty;
            var values = state as ILogValues;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else if (values != null)
            {
                var builder = new StringBuilder();
                FormatLogValues(
                    builder,
                    values,
                    level: 1,
                    bullet: false);
                message = builder.ToString();
                if (exception != null)
                {
                    message += Environment.NewLine + exception;
                }
            }
            else
            {
                message = LogFormatter.Formatter(state, exception);
            }
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            lock (_lock)
            {
                var extraItemCount = logList.Count - MaxLogCount;
                if (extraItemCount > 0)
                {
                    logList.RemoveRange(0, extraItemCount);
                }
                logList.Add(FormatMessage(logLevel, _name, message));
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

        public IDisposable BeginScopeImpl(object state)
        {
            return new NoopDisposable();
        }

        private void FormatLogValues(StringBuilder builder, ILogValues logValues, int level, bool bullet)
        {
            var values = logValues.GetValues();
            if (values == null)
            {
                return;
            }
            var isFirst = true;
            foreach (var kvp in values)
            {
                builder.AppendLine();
                if (bullet && isFirst)
                {
                    builder.Append(' ', level * _indentation - 1)
                           .Append('-');
                }
                else
                {
                    builder.Append(' ', level * _indentation);
                }
                builder.Append(kvp.Key)
                       .Append(": ");
                if (kvp.Value is IEnumerable && !(kvp.Value is string))
                {
                    foreach (var value in (IEnumerable)kvp.Value)
                    {
                        if (value is ILogValues)
                        {
                            FormatLogValues(
                                builder,
                                (ILogValues)value,
                                level + 1,
                                bullet: true);
                        }
                        else
                        {
                            builder.AppendLine()
                                   .Append(' ', (level + 1) * _indentation)
                                   .Append(value);
                        }
                    }
                }
                else if (kvp.Value is ILogValues)
                {
                    FormatLogValues(
                        builder,
                        (ILogValues)kvp.Value,
                        level + 1,
                        bullet: false);
                }
                else
                {
                    builder.Append(kvp.Value);
                }
                isFirst = false;
            }
        }

        private static string GetRightPaddedLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return "DEBUG   ";
                case LogLevel.Verbose:
                    return "VERBOSE ";
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

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
