using LogicStorage.Utils;
using Microsoft.Win32;
using SharpPcap;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LogicStorage.Handlers;
using LogicStorage.Dtos;

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

        private void btn_fileDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Exe Files (.exe)|*.exe",
                FilterIndex = 1
            };

            var selected = false;

            if (dialog.ShowDialog() == true)
            {
                _config.ExePath = dialog.FileName;
                lbl_filePath.Content = dialog.FileName;
                selected = true;
            }

            if (!_config.ExePath.Equals("none") && selected)
            {
                _log.AddLog("Executable file choosed successfuly! Please, run the game form executable!");
                btn_startExe.IsEnabled = true;
            }
        }

        private void btn_startExe_Click(object sender, RoutedEventArgs e)
        {
            _clientProcess = _client.GetProcessByName();
            if (_clientProcess != null)
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

            btn_fileDialog.IsEnabled = false;
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
                _log.AddLog("Everything seems to be configured. Please, start a normal game and click start!");
                btn_monitorStart.IsEnabled = true;
                chk_minimaliseExecutor.IsEnabled = true;
                chk_preserveSettings.IsEnabled = true;
                chk_promptMsg.IsEnabled = true;
            }
        }

        private void btn_monitorStart_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Buddy will now start his job in background. Stick to the README and you will be fine! Ready?",
                    "Training Buddy",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _log.Clean();
                _log.AddLog("Buddy started!");

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = "Executor.exe"
                    }
                };
                process.Start();

                _config.ClientPID = _clientProcess.Id;
                _config.NetworkInterfaceName = _device.Name;

                _serializer.SerializeExecutorConfig(_config);

                System.Environment.Exit(1);
            }
        }

        private void chk_promptMsg_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_promptMsg.IsChecked.Value)
                _config.AllwaysPromptTextBox = true;
            else
                _config.AllwaysPromptTextBox = false;
        }

        private void chk_minimaliseExecutor_Checked(object sender, RoutedEventArgs e)
        {
            if(chk_minimaliseExecutor.IsChecked.Value)
                _config.MinimaliseExecutor = true;
            else
                _config.MinimaliseExecutor = false;
        }

        private void chk_preserveSettings_Checked(object sender, RoutedEventArgs e)
        {
            if(chk_preserveSettings.IsChecked.Value)
                _config.PreserveSettings = true;
            else
                _config.PreserveSettings = false;
        }
    }
}
