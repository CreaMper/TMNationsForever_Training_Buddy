using LogicStorage;
using LogicStorage.Dtos.Config;
using LogicStorage.Utils;
using System;
using System.IO;
using System.Threading;
using System.Windows;
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

        public InitialiseWindow()
        {
            InitializeComponent();

            _factory = new Factory();
            _log = new LogHandler(rbx_log, Dispatcher);
            _exception = new ExceptionHandler(_log, Dispatcher);
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            new Thread(Initialise).Start();
        }

        private void Initialise()
        {
            if (_factory.BuddyConfig == null)
            {
                _log.AddLog("Configuration file was not found! Auto-configuration will be performed!", LogTypeEnum.Info);

                if (!_factory.Client.VerifyClientFileStructure())
                {
                    _log.AddLog("Cannot find a game executable! Please, move the program to TM Directory!", LogTypeEnum.CRITICAL);
                    _exception.CriticalThrow();
                }
                else
                {
                    _log.AddLog("Client executable found!", LogTypeEnum.Success);
                }

                _factory.Network.AutoDeviceSelection();
                if (_factory.Network.Device == null)
                {

                    _log.AddLog("Cannot choose an internet interface! Please check your internet connection, restart Buddy and try again!", LogTypeEnum.CRITICAL);
                    _exception.CriticalThrow();
                }
                else
                {
                    _log.AddLog("Internet interface found!", LogTypeEnum.Success);
                }

                _log.AddLog("Saving config file...", LogTypeEnum.Info);

                try
                {
                    _factory.BuddyConfig = new BuddyConfigDto()
                    {
                        InterfaceName = _factory.Network.Device?.Name,
                        ClientPath = Directory.GetCurrentDirectory()
                    };

                    _factory.Serializer.SerializeBuddyConfig(_factory.BuddyConfig);
                }
                catch
                {
                    _log.AddLog("Cannot save a configuration data! Try to open Buddy with administrator privilages!", LogTypeEnum.CRITICAL);
                    _exception.CriticalThrow();
                }
            }
            else
            {
                _log.AddLog("Configuration file found! Loading settings...", LogTypeEnum.Success);
            }
        }
    }
}
