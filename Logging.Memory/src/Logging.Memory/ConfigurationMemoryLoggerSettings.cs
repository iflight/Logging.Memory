namespace iflight.Logging
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using System;

    public class ConfigurationMemoryLoggerSettings : IMemoryLoggerSettings
    {
        private readonly IConfiguration _configuration;

        public ConfigurationMemoryLoggerSettings(IConfiguration configuration)
        {
            _configuration = configuration;
            ChangeToken = configuration.GetReloadToken();
        }

        public IChangeToken ChangeToken { get; private set; }

        public int MaxLogCount
        {
            get
            {
                int maxLogCount;
                var value = _configuration["MaxLogCount"];
                if (string.IsNullOrEmpty(value))
                {
                    return 200;
                }
                else if (int.TryParse(value, out maxLogCount))
                {
                    return maxLogCount;
                }
                else
                {
                    var message = $"Configuration value '{value}' for setting '{nameof(MaxLogCount)}' is not supported.";
                    throw new InvalidOperationException(message);
                }
            }
        }

        public IMemoryLoggerSettings Reload()
        {
            ChangeToken = null;
            return new ConfigurationMemoryLoggerSettings(_configuration);
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            var switches = _configuration.GetSection("LogLevel");
            if (switches == null)
            {
                level = LogLevel.None;
                return false;
            }

            var value = switches[name];
            if (string.IsNullOrEmpty(value))
            {
                level = LogLevel.None;
                return false;
            }
            else if (Enum.TryParse<LogLevel>(value, out level))
            {
                return true;
            }
            else
            {
                var message = $"Configuration value '{value}' for category '{name}' is not supported.";
                throw new InvalidOperationException(message);
            }
        }
    }


}
