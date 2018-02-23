using Microsoft.Build.Utilities;
using AsmSpy.Core;

namespace AsmSpy.MSTask
{
    public class MsBuildLogger : ILogger
    {
        private readonly TaskLoggingHelper log;

        public MsBuildLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void LogMessage(string message)
        {
        }

        public void LogError(string message)
        {
            log.LogError(message);
        }

        public void LogWarning(string message)
        {
            log.LogWarning(message);
        }
    }
}