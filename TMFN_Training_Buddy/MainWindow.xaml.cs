using Microsoft.Win32;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
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
using TMFN_Training_Buddy.Handlers;

namespace TMFN_Training_Buddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILiveDevice _device;
        private bool _showAllInterfaces = false;
        private string _exePath = "none"; 

        private NetworkHandler _network = new NetworkHandler();
        private DataHandler _data = new DataHandler();
        private LogHandler _log;

        public MainWindow()
        {
            InitializeComponent();
            _log = new LogHandler(tb_logBox, sv_log);
            _log.AddLog("Initialising...");

            //Data init
            dd_internetInterfaces.ItemsSource = _data.GetDeviceList(_network.DeviceList, _showAllInterfaces);

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
                _showAllInterfaces = true;
                dd_internetInterfaces.SelectedItem = null;
                _device = null;
                dd_internetInterfaces.ItemsSource = _data.GetDeviceList(_network.DeviceList, _showAllInterfaces);
            } 
            else
            {
                _showAllInterfaces = false;
                dd_internetInterfaces.SelectedItem = null;
                _device = null;
                dd_internetInterfaces.ItemsSource = _data.GetDeviceList(_network.DeviceList, _showAllInterfaces);
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
                _log.AddLog("Interface challange successful! Entering a listening mode...");

                dd_internetInterfaces.IsEnabled = false;
                chk_showAllInterfaces.IsEnabled = false;
                btn_interfaceAuto.IsEnabled = false;
                btn_connectionTest.IsEnabled = false;

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
            var temporaryDevice = _data.GetDeviceList(_network.DeviceList, false).FirstOrDefault();
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

            if (dialog.ShowDialog() == true)
            {
                _exePath = dialog.FileName;
                lbl_filePath.Content = dialog.FileName;
            }
                
            if (!_exePath.Equals("none"))
            {
                _log.AddLog("Executable file choosed successfuly! You can run the game safely.");
                btn_startExe.IsEnabled = true;
            }
        }
    }
}
