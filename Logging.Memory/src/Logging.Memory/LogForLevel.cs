using System;
using System.Collections.Generic;
using System.Threading;

namespace Logging.Memory
{
    internal class LogForLevel
    {
        internal LogForLevel(int maxLogsCount)
        {
            logList = new List<(DateTime time, string line)>(maxLogsCount);
        }

        readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        int currentLogIndex = 0;
        readonly List<(DateTime time, string line)> logList;

        /// <remarks>This method is thread-safe</remarks>
        public void Append(DateTime time, string line, int maxLogCount)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                if (logList.Count < maxLogCount)
                {
                    logList.Add((time, line));
                }
                else
                {
                    logList[currentLogIndex] = (time, line);
                }

                if (currentLogIndex < maxLogCount - 1)
                {
                    currentLogIndex++;
                }
                else
                {
                    currentLogIndex = 0;
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }
        
        /// <remarks>This method is thread-safe</remarks>
        public (DateTime time, string line)[] GetEntries()
        {
            this.rwLock.EnterReadLock();
            try
            {
                return logList.ToArray();
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }
    }
}