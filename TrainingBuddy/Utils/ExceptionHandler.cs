using System;
using System.Threading;
using System.Windows.Threading;
using TrainingBuddy.Handlers;

namespace TrainingBuddy.Utils
{
    public class ExceptionHandler
    {
        private LogHandler _log;
        private Dispatcher _dispather;
        private bool _exceptionOccured;

        public ExceptionHandler(LogHandler log, Dispatcher dispather)
        {
            _log = log;
            _dispather = dispather;
        }

        public bool ExceptionOccured
        {
            get { return _exceptionOccured; }
        }

        public void CriticalThrow()
        {
            _exceptionOccured = true;
            Critical();
        }

        private void Critical()
        {
            _log.AddLog("Critical error encountered! Exiting in 5 seconds...", LogicStorage.Utils.LogTypeEnum.CRITICAL);

            Thread.Sleep(5000);
            Environment.Exit(69);
        }
    }
}
