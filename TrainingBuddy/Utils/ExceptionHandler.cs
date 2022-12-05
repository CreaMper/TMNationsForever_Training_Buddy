using System;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Linq;
using TrainingBuddy.Handlers;

namespace TrainingBuddy.Utils
{
    public class ExceptionHandler
    {
        private LogHandler _log;
        private Dispatcher _dispather;

        public ExceptionHandler(LogHandler log, Dispatcher dispather)
        {
            _log = log;
            _dispather = dispather;
        }

        public void CriticalThrow()
        {
            new Thread(Critical).Start();
        }

        private void Critical()
        {
            _dispather.Invoke(() =>
            {
                _log.AddLog("Critical error encountered! Exiting in 5 seconds...", LogicStorage.Utils.LogTypeEnum.CRITICAL);
            });

            Thread.Sleep(5000);
            Environment.Exit(69);
        }
    }
}
