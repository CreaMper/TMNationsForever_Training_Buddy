using log4net;
using LogicStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TrainingBuddy.Handlers;

namespace TrainingBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InitialiseWindow : Window
    {
        private Factory _factory;
        private static LogHandler _log;

        public InitialiseWindow()
        {
            InitializeComponent();

            _factory = new Factory();
            _log = new LogHandler(rbx_log);

            if (_factory.BuddyConfig == null)
            {
                _log.AddLog("Configuration file was not found! Auto-configuration will be performed!", LogicStorage.Utils.LogTypeEnum.Info);
            }
        }
    }
}
