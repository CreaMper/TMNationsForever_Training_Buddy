using SharpPcap;
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
using TMFN_Training_Buddy.Handlers;

namespace TMFN_Training_Buddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILiveDevice _device;
        private bool showAllInterfaces = false;
        private NetworkHandler _network = new NetworkHandler();
        private DataHandler _data = new DataHandler();


        public MainWindow()
        {
            InitializeComponent();

            //Data init
            dd_internetInterfaces.ItemsSource = _data.GetDeviceList(_network.DeviceList, showAllInterfaces);

        }

        private void dd_internetInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _device = _network.DeviceList.FirstOrDefault(x => x.Equals(dd_internetInterfaces.SelectedItem));
        }

        private void chk_showAllInterfaces_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_showAllInterfaces.IsChecked.Value)
            {
                showAllInterfaces = true;
                dd_internetInterfaces.SelectedItem = null;
                _device = null;
                dd_internetInterfaces.ItemsSource = _data.GetDeviceList(_network.DeviceList, showAllInterfaces);
            } 
            else
            {
                showAllInterfaces = false;
                dd_internetInterfaces.SelectedItem = null;
                _device = null;
                dd_internetInterfaces.ItemsSource = _data.GetDeviceList(_network.DeviceList, showAllInterfaces);
            }

        }
    }
}
