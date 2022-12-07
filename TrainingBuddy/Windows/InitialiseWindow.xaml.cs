using LogicStorage;
using LogicStorage.Dtos;
using LogicStorage.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TrainingBuddy.Handlers;
using TrainingBuddy.Utils;
using TrainingBuddy.Windows;

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
            _exception = new ExceptionHandler(_log);
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

                var deviceFromConfig = _factory.Network.DeviceList.FirstOrDefault(x => x.Name.Equals(_factory.BuddyConfig.InterfaceName));
                if (!_factory.Network.ChallangeInterface(deviceFromConfig))
                {
                    _log.AddLog("Cannot load internet interface from config! Corrupted config will be deleted!", LogTypeEnum.CRITICAL);
                    if (!_factory.Serializer.RemoveCorruptedBuddyConfig())
                    {
                        _log.AddLog("Cannot remove config file! Please, do it manualy!", LogTypeEnum.CRITICAL);
                    }
                    _exception.CriticalThrow();
                }
                _factory.Network.Device = deviceFromConfig;

                if (_factory.BuddyConfig.ClientPath.Equals(string.Empty) || !_factory.Client.VerifyClientFileStructure())
                {
                    _log.AddLog("Cannot read a executable path from config file! Corrupted config will be deleted!", LogTypeEnum.CRITICAL);
                    if (!_factory.Serializer.RemoveCorruptedBuddyConfig())
                    {
                        _log.AddLog("Cannot remove config file! Please, do it manualy!", LogTypeEnum.CRITICAL);
                    }
                    _exception.CriticalThrow();
                }

                if (!Directory.Exists(_factory.BuddyConfig.ClientPath))
                {
                    _log.AddLog("Cannot read a executable path from config file! Corrupted config will be deleted!", LogTypeEnum.CRITICAL);
                    if (!_factory.Serializer.RemoveCorruptedBuddyConfig())
                    {
                        _log.AddLog("Cannot remove config file! Please, do it manualy!", LogTypeEnum.CRITICAL);
                    }
                    _exception.CriticalThrow();
                }
            }

            _log.AddLog("Initialisation complete! Say Hi to your new buddy in a few seconds!", LogTypeEnum.Success);
            Thread.Sleep(3000);
            Dispatcher.Invoke(() => {
                new BuddyWindow(_factory).Show();
                Close();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/A0A0GM3N0",
                UseShellExecute = true
            });
        }
    }
}
