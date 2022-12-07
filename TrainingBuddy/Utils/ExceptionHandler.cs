using System;
using System.Threading;
using TrainingBuddy.Handlers;

namespace TrainingBuddy.Utils
{
    public class ExceptionHandler
    {
        private LogHandler _log;

        public ExceptionHandler(LogHandler log)
        {
            _log = log;
        }

        public void CriticalThrow()
        {
            _log.AddLog("Critical error encountered! Exiting in 5 seconds...", LogicStorage.Utils.LogTypeEnum.CRITICAL);

            Thread.Sleep(5000);
            Environment.Exit(69);
        }
    }
}
