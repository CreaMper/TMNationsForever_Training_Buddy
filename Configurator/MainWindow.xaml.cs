using LogicStorage.Dtos;
using LogicStorage.Handlers;
using LogicStorage.Utils;
using SharpPcap;
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
        private ILiveDevice _device;
        private ConfigurationDto _config;
        private Process _clientProcess;

        private static NetworkHandler _network = new NetworkHandler();
        private static ClientHandler _client = new ClientHandler();
        private static LogHandler _log;
        private static DLLImporter _importer = new DLLImporter();
        private static Serializer _serializer = new Serializer();

        public MainWindow()
        {
            InitializeComponent();
            _log = new LogHandler(tb_logBox, sv_log);
            _log.AddLog("Initialising...");

            _config = new ConfigurationDto();

            dd_internetInterfaces.ItemsSource = _network.GetDeviceList(_config.ShowAllInterfaces);
            _log.AddLog("Initialising done! Please, configure network interface and executable settings!");
        }

        private void dd_internetInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _device = _network.DeviceList.FirstOrDefault(x => x.Name.Equals(dd_internetInterfaces.SelectedItem));
            if (_device != null)
                _log.AddLog($"Interface changed to: {_device.Name}");
        }

        private void chk_showAllInterfaces_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_showAllInterfaces.IsChecked.Value)
            {
                _config.ShowAllInterfaces = true;
                dd_internetInterfaces.SelectedItem = null;
                _device = null;
                dd_internetInterfaces.ItemsSource = _network.GetDeviceList(_config.ShowAllInterfaces);
            }
            else
            {
                _config.ShowAllInterfaces = false;
                dd_internetInterfaces.SelectedItem = null;
                _device = null;
                dd_internetInterfaces.ItemsSource = _network.GetDeviceList(_config.ShowAllInterfaces);
            }
        }

        private void btn_connectionTest_Click(object sender, RoutedEventArgs e)
        {
            if (_device == null)
            {
                _log.AddLog("You need to select interface to test it's connection!");
                return;
            }

            _log.AddLog("Started interface challange...");

            if (_network.ChallangeInterface(_device))
            {
                lbl_connectionTestResult.Content = "OK";
                lbl_connectionTestResult.Foreground = Brushes.Green;
                _log.AddLog("Interface challange successful!");

                dd_internetInterfaces.IsEnabled = false;
                chk_showAllInterfaces.IsEnabled = false;
                btn_interfaceAuto.IsEnabled = false;
                btn_connectionTest.IsEnabled = false;
                _config.NetworkConfigured = true;
                ProgressChecker();

            }
            else
            {
                lbl_connectionTestResult.Content = "ERROR";
                lbl_connectionTestResult.Foreground = Brushes.Red;
                _log.AddLog("Interface challange failed - please select different one!");
            }
        }

        private void btn_interfaceAuto_Click(object sender, RoutedEventArgs e)
        {
            var temporaryDevice = _network.GetDeviceList(false).FirstOrDefault();
            if (temporaryDevice == null)
            {
                _log.AddLog("Cannot find interface automatically!");
                return;
            }

            _device = _network.DeviceList.FirstOrDefault(x => x.Name.Equals(temporaryDevice));
            dd_internetInterfaces.SelectedItem = temporaryDevice;
            _log.AddLog($"Auto select: {temporaryDevice}");

        }

        private void btn_startExe_Click(object sender, RoutedEventArgs e)
        {
            _log.AddLog("Starting client...");

            _clientProcess = new Process
            {
                StartInfo =
                    {
                        FileName = "TmForever.exe"
                    }
            };
            _clientProcess.Start();

            _log.AddLog("Awaiting for client to fully load...");
            Thread.Sleep(5000);

            if (!_clientProcess.HasExited)
            {
                DisableGameExecutableSettings();
                return;
            }

            _log.AddLog("Cannot find a Trackmania process! Please, re-run and try again!");
            return;
        }

        private void DisableGameExecutableSettings()
        {
            _log.AddLog($"Found an Trackmania process! with PID {_clientProcess.Id}");
            _importer.UseSetWindowText(_clientProcess.MainWindowHandle, "TM Training Buddy Client");
            _log.AddLog("Renamed a window name to make it easy for you!");

            btn_startExe.IsEnabled = false;
            _config.ClientConfigured = true;
            ProgressChecker();
        }

        private void ProgressChecker()
        {
            if (!_config.NetworkConfigured)
                _log.AddLog("You still need to configure an Internet Interface before start!");

            if (!_config.ClientConfigured)
                _log.AddLog("You still need to configure an Game Executable before start!");

            if (_config.NetworkConfigured && _config.ClientConfigured)
            {
                _log.AddLog("Configuration sucessfull! You can start the buddy and run a normal game client!");
                btn_monitorStart.IsEnabled = true;
            }
        }

        private void btn_monitorStart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Buddy will start to assist you! You can start your own client and start the training!",
                "Training Buddy",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _config.ClientPID = _clientProcess.Id;
            _config.NetworkInterfaceName = _device.Name;
            _serializer.SerializeExecutorConfig(_config);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "Buddy_Executor.exe"
                }
            };
            process.Start();

            System.Environment.Exit(1);
        }

        private void btn_exeAutoDetect_Click(object sender, RoutedEventArgs e)
        {
            var dataStructure = _client.VerifyClientFileStructure();
            if (!dataStructure)
            {
                _log.AddLog("Cannot find a game client! Please make sure that Training Buddy is in the same directory as game client!");
                return;
            }

            var obsoleteGameClients = _client.GetGameClientProcess();
            if (obsoleteGameClients != null)
            {
                _log.AddLog("Please, exit all trackmania clients before proceed!");
                return;
            }

            lbl_filePath.Foreground = Brushes.Green;
            lbl_filePath.Content = "OK";
            _log.AddLog("Client executable found! Please, start the client using button above!");

            btn_exeAutoDetect.IsEnabled = false;
            btn_startExe.IsEnabled = true;
        }
    }
}
