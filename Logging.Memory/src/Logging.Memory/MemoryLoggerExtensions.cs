namespace Microsoft.Extensions.Logging
{
    using System;
    using iflight.Logging;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Extensions for memoryLogger
    /// </summary>
    public static class MemoryLoggerExtensions
    {
        /// <summary>
        /// Adds a memory logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new MemoryLoggerProvider((category, logLevel) => logLevel >= LogLevel.Information));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger that is enabled as defined by the filter function.
        /// </summary>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, Func<string, LogLevel, bool> filter, Func<LogLevel, string, string, string> formatter = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new MemoryLoggerProvider(filter, formatter: formatter));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        /// <param name="formatter">Formatter for output log line</param>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, LogLevel minLevel, Func<LogLevel, string, string, string> formatter = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new MemoryLoggerProvider((category, logLevel) => logLevel >= minLevel, formatter: formatter));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, int maxLogCount)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new MemoryLoggerProvider((category, logLevel) => logLevel >= LogLevel.Information, maxLogCount));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger that is enabled as defined by the filter function.
        /// </summary>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, Func<string, LogLevel, bool> filter, int maxLogCount)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new MemoryLoggerProvider(filter, maxLogCount));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, LogLevel minLevel, int maxLogCount)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new MemoryLoggerProvider((category, logLevel) => logLevel >= minLevel, maxLogCount));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger with settings object.
        /// </summary>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, IMemoryLoggerSettings settings)
        {
            factory.AddProvider(new MemoryLoggerProvider(settings));
            return factory;
        }

        /// <summary>
        /// Adds a memory logger with settings from configuration.
        /// </summary>
        public static ILoggerFactory AddMemory(this ILoggerFactory factory, IConfiguration configuration)
        {
            var settings = new ConfigurationMemoryLoggerSettings(configuration);
            return factory.AddMemory(settings);
        }


    }
}
