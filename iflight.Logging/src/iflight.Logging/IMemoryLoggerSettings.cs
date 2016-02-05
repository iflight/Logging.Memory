namespace iflight.Logging
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    public interface IMemoryLoggerSettings
    {
        int MaxLogCount { get; }

        IChangeToken ChangeToken { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        IMemoryLoggerSettings Reload();
    }

}

