using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Logging.Memory.Settings
{
    /// <summary>
    /// Interface for logger settings
    /// </summary>
    public interface IMemoryLoggerSettings
    {
        /// <summary>
        /// Max count of stored logs entries
        /// </summary>
        int MaxLogCount { get; }

        /// <summary>
        /// Log settings ChangeToken
        /// </summary>
        IChangeToken ChangeToken { get; }

        /// <summary>
        /// try to chaneg log level
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        bool TryGetSwitch(string name, out LogLevel level);

        /// <summary>
        /// Reload settings
        /// </summary>
        /// <returns>Reloaded setting object</returns>
        IMemoryLoggerSettings Reload();
    }

}

