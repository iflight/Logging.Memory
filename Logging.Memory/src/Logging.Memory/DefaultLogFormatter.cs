using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logging.Memory
{
    /// <summary>
    /// Default log line formatter
    /// </summary>
    public static class DefaultLogLineFormatter
    {
        /// <summary>
        /// Format string
        /// </summary>
        /// <param name="logLevel">Current line level</param>
        /// <param name="logName">Curent logger name</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception (may be null)</param>
        /// <returns>Formatted string</returns>
        public static string Formatter(LogLevel logLevel, string logName, string message, Exception exception)
        {
            var logLevelString = GetRightPaddedLogLevelString(logLevel);

            return $"{DateTime.Now.ToString("HH:mm:ss,fff")} - {logLevelString}: [{logName}] {message}{(exception != null ? $"{Environment.NewLine}{exception}" : "")}";
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
    }
}
