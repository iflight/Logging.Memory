namespace iflight.Logging
{

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;

    public class MemoryLoggerSettings : IMemoryLoggerSettings
    {
        public IChangeToken ChangeToken { get; set; }


        public bool IncludeScopes { get; set; }

        public int MaxLogCount { get; set; }

        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();


        public IMemoryLoggerSettings Reload()
        {
            return this;
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            return Switches.TryGetValue(name, out level);
        }
    }

}
