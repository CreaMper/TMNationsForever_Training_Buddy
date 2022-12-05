using log4net;
using LogicStorage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TrainingBuddy.Handlers;
using TrainingBuddy.Utils;

namespace TrainingBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InitialiseWindow : Window
    {
        private readonly Factory _factory;
        private readonly LogHandler _log;
        private readonly ExceptionHandler _exception;
        public delegate void LoggerDelegate();

        public InitialiseWindow()
        {
            InitializeComponent();

            _factory = new Factory();
            _log = new LogHandler(rbx_log);
            _exception = new ExceptionHandler(_log, Dispatcher);

            if (_factory.BuddyConfig == null)
            {
                _log.AddLog("Configuration file was not found! Auto-configuration will be performed!", LogicStorage.Utils.LogTypeEnum.Info);

                if (!_factory.Client.VerifyClientFileStructure())
                {
                    _log.AddLog("Cannot find a game executable! Please, move the program to TM Directory!", LogicStorage.Utils.LogTypeEnum.CRITICAL);
                    _exception.CriticalThrow();
                }
            }
            else
            {
                _log.AddLog("Configuration file found! Loading settings...", LogicStorage.Utils.LogTypeEnum.Success);
            }
        }
    }
}
