using System;
using System.Collections.Generic;

namespace Logging.Memory
{
    internal class LogForLevel
    {
        internal LogForLevel(int maxLogsCount)
        {
            logList = new List<(DateTime time, string line)>(maxLogsCount);
        }

        internal int currentLogIndex = 0;

        internal List<(DateTime time, string line)> logList;

    }
}
