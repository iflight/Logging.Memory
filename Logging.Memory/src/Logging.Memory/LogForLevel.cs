using System;
using System.Collections.Generic;

namespace Logging.Memory
{
    internal class LogForLevel
    {
        internal LogForLevel(int maxLogsCount)
        {
            logList = new List<Tuple<DateTime, string>>(maxLogsCount);
        }

        internal int currentLogIndex = 0;

        internal List<Tuple<DateTime, string>> logList;

    }
}
