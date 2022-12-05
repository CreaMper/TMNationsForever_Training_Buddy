using LogicStorage;
using LogicStorage.Dtos.Config;
using LogicStorage.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Configurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LogHandler _log;
        private Factory _factory;

        private ConfiguratorConfigDto _config;

        public MainWindow()
        {
            InitializeComponent();
            _factory = new Factory();

            _log = new LogHandler(rtxt_logBox);

            _config = new ConfiguratorConfigDto()
            {
                ListeningIntensivityLevel = 10
            };

            var configFromFile = _factory.Serializer.DeserializeExecutorConfig();
            if (configFromFile != null)
            {
                try
                {
                    var dataStructure = _factory.Client.VerifyClientFileStructure();
                    if (!dataStructure)
                        return;
                    _factory.Network.Device = _factory.Network.DeviceList.FirstOrDefault(x => x.Name.Equals(configFromFile.NetworkInterfaceName));

                    lbl_connectionConfigStatus.Content = "CONFIGURED";
                    lbl_connectionConfigStatus.Foreground = Brushes.Green;

                    dd_internetInterfaces.IsEnabled = false;
                    chk_showAllInterfaces.IsEnabled = false;
                    btn_interfaceAuto.IsEnabled = false;
                    btn_connectionTest.IsEnabled = false;
                    _config.NetworkConfigured = true;

                    lbl_executableConfigStatus.Content = "CLIENT NOT FOUND";

                    btn_exeAutoDetect.IsEnabled = false;
                    btn_startClient.IsEnabled = true;
                }
                catch
                {
                    return;
                }

                _log.AddLog("Found a previous configuration file! If you wish to hard-reset this settings, please remove config.json file!", LogTypeEnum.Info);
                _log.AddLog("Please, start the Buddy client!", LogTypeEnum.Info);
            }
            else
            {
                dd_internetInterfaces.ItemsSource = _factory.Network.GetDeviceList(_config.ShowAllInterfaces);
                _log.AddLog("Hi! Please, use two sections above to configure your Internet Interfaces and Game Executable!", LogTypeEnum.Info);
            }
        }

        private void dd_internetInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _factory.Network.Device = _factory.Network.DeviceList.FirstOrDefault(x => x.Name.Equals(dd_internetInterfaces.SelectedItem));
            if (_factory.Network.Device != null)
                _log.AddLog($"Interface changed to: {_factory.Network.Device.Name}", LogTypeEnum.Info);
        }

        private void chk_showAllInterfaces_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_showAllInterfaces.IsChecked.Value)
            {
                _config.ShowAllInterfaces = true;
                dd_internetInterfaces.SelectedItem = null;
                _factory.Network.Device = null;
                dd_internetInterfaces.ItemsSource = _factory.Network.GetDeviceList(_config.ShowAllInterfaces);
            }
            else
            {
                _config.ShowAllInterfaces = false;
                dd_internetInterfaces.SelectedItem = null;
                _factory.Network.Device = null;
                dd_internetInterfaces.ItemsSource = _factory.Network.GetDeviceList(_config.ShowAllInterfaces);
            }
        }

        private void btn_connectionTest_Click(object sender, RoutedEventArgs e)
        {
            if (_factory.Network.Device == null)
            {
                _log.AddLog("You need to select interface to test it's connection!", LogTypeEnum.Error);
                return;
            }

            _log.AddLog("Started interface challange...", LogTypeEnum.Info);

            if (_factory.Network.ChallangeInterface(_factory.Network.Device))
            {
                lbl_connectionConfigStatus.Content = "CONFIGURED";
                lbl_connectionConfigStatus.Foreground = Brushes.Green;
                _log.AddLog("Interface challange successful!", LogTypeEnum.Success);

                dd_internetInterfaces.IsEnabled = false;
                chk_showAllInterfaces.IsEnabled = false;
                btn_interfaceAuto.IsEnabled = false;
                btn_connectionTest.IsEnabled = false;
                _config.NetworkConfigured = true;
                ProgressChecker();
            }
            else
            {
                lbl_connectionConfigStatus.Content = "ERROR";
                lbl_connectionConfigStatus.Foreground = Brushes.Red;
                _log.AddLog("Interface challange failed - please select different one!", LogTypeEnum.Error);
            }
        }

        private void btn_interfaceAuto_Click(object sender, RoutedEventArgs e)
        {
/*            var temporaryDevice = _factory.Network.GetDeviceList(false).FirstOrDefault();
            if (temporaryDevice == null)
            {
                _log.AddLog("Cannot find interface automatically!", LogTypeEnum.Error);
                return;
            }

            _factory.Network.Device = _factory.Network.DeviceList.FirstOrDefault(x => x.Name.Equals(temporaryDevice));
            dd_internetInterfaces.SelectedItem = temporaryDevice;*/

/*            _factory.Importer.UseSetForegroundWindow(_factory.Client.Buddy.MainWindowHandle);
            _factory.Importer.UseSetWindowText(_factory.Client.Buddy.MainWindowHandle, $"test");*/
           var p = new Process();
            p.StartInfo = new ProcessStartInfo("replay.gbx")
            {
                UseShellExecute = true
            };
            p.Start();

        }

        private void btn_startExe_Click(object sender, RoutedEventArgs e)
        {
            _log.AddLog("Starting client...", LogTypeEnum.Info);

            _factory.Client.Buddy = new Process
            {
                StartInfo = { FileName = "TmForever.exe" }
            };
            _factory.Client.Buddy.Start();

            _log.AddLog("Awaiting for client to fully load...", LogTypeEnum.Info);
            Thread.Sleep(5000);

            if (!_factory.Client.Buddy.HasExited)
            {
                lbl_executableConfigStatus.Content = "CONFIGURED";
                lbl_executableConfigStatus.Foreground = Brushes.Green;

                DisableGameExecutableSettings();
                return;
            }

            _log.AddLog("Cannot find a Trackmania process! Please, re-run and try again!", LogTypeEnum.Error);
            return;
        }

        private void DisableGameExecutableSettings()
        {
            _log.AddLog($"Found an Trackmania process! with PID {_factory.Client.Buddy.Id}", LogTypeEnum.Info);
            _factory.Importer.UseSetWindowText(_factory.Client.Buddy.MainWindowHandle, "TM Training Buddy Client");
            _log.AddLog("Please, make sure that game is in WINDOWED mode!", LogTypeEnum.Info);

            btn_startClient.IsEnabled = false;
            _config.ClientConfigured = true;
            ProgressChecker();
        }

        private void ProgressChecker()
        {
            if (!_config.NetworkConfigured)
                _log.AddLog("You still need to configure an Internet Interface before start!", LogTypeEnum.Info);

            if (!_config.ClientConfigured)
                _log.AddLog("You still need to configure an Game Executable before start!", LogTypeEnum.Info);

            if (_config.NetworkConfigured && _config.ClientConfigured)
            {
                _log.AddLog("Configuration sucessfull! You can start the buddy and run a normal game client!", LogTypeEnum.Success);
                btn_startBuddy.IsEnabled = true;
            }
        }

        private void btn_monitorStart_Click(object sender, RoutedEventArgs e)
        {
            var checkForBuddyClient = _factory.Client.GetGameClientProcess();
            if (checkForBuddyClient == null)
            {
                _log.AddLog("Buddy client was not found! Please, start buddy client once again!", LogTypeEnum.Error);
                btn_startClient.IsEnabled = true;
                btn_startBuddy.IsEnabled = false;
                _config.ClientConfigured = false;
                return;
            }

            var msgResult = MessageBox.Show("Configuration tool will close and Buddy will start to assist you! Do you want me to open another game client for you?",
                "Training Buddy",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                var additionallClient = new Process
                {
                    StartInfo =
                    {
                        FileName = "TmForever.exe"
                    }
                };

                additionallClient.Start();
            }


            _config.ClientPID = _factory.Client.Buddy.Id;
            _config.NetworkInterfaceName = _factory.Network.Device.Name;
            _factory.Serializer.SerializeExecutorConfig(_config);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "Buddy_Executor.exe"
                }
            };

            if (_config.MinimaliseExecutor)
                process.StartInfo.CreateNoWindow = true;

            process.Start();

            System.Environment.Exit(1);
        }

        private void btn_exeAutoDetect_Click(object sender, RoutedEventArgs e)
        {
            var dataStructure = _factory.Client.VerifyClientFileStructure();
            if (!dataStructure)
            {
                _log.AddLog("Cannot find a game client! Please make sure that Training Buddy is in the same directory as game client!", LogTypeEnum.Error);
                return;
            }

            var obsoleteGameClients = _factory.Client.GetGameClientProcess();
            if (obsoleteGameClients != null)
            {
                _log.AddLog("Please, exit all trackmania clients before proceed!", LogTypeEnum.Error);
                return;
            }

            lbl_executableConfigStatus.Content = "CLIENT NOT FOUND";
            _log.AddLog("Client executable found! Please, start the client using button above!", LogTypeEnum.Info);

            btn_exeAutoDetect.IsEnabled = false;
            btn_startClient.IsEnabled = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(lbl_listeningIntensivityLevel != null)
            {
                lbl_listeningIntensivityLevel.Content = sld_intensivity.Value;
                _config.ListeningIntensivityLevel = Int32.Parse(sld_intensivity.Value.ToString());
            }
        }

        private void chk_minimaliseExecutor_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_minimaliseExecutor.IsChecked.Value)
                _config.MinimaliseExecutor = true;
            else
                _config.MinimaliseExecutor = false;
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
