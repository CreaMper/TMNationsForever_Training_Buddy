using System;
using System.Threading;
using System.Windows;
using TrainingBuddy.Handlers;

namespace TrainingBuddy.Utils
{
    public class ExceptionHandler
    {
        private readonly LogHandler _log;

        public ExceptionHandler(LogHandler log)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                MessageBox.Show($"Critical error occured! Buddy is going to die...! \n\n EXCEPTION: {eventArgs.ExceptionObject.ToString()?[..500]}", "Exception thrown!", MessageBoxButton.OK, MessageBoxImage.Error);
            };

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
