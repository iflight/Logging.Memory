using System;
using System.Collections.Generic;

namespace iflight.Logging
{
    internal class LogLevelLog
    {
        public LogLevelLog(int maxLogsCount)
        {
            logList = new List<Tuple<DateTime, string>>(maxLogsCount);
        }

        public int currentLogIndex = 0;

        public List<Tuple<DateTime, string>> logList;

    }
}
