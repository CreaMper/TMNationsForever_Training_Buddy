using LogicStorage;
using LogicStorage.Utils;
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
using System.Windows.Shapes;
using TrainingBuddy.Handlers;
using TrainingBuddy.Utils;

namespace TrainingBuddy.Windows
{
    /// <summary>
    /// Interaction logic for Buddy.xaml
    /// </summary>
    public partial class BuddyWindow : Window
    {
        private readonly Factory _factory;
        private readonly LogHandler _log;
        private readonly ExceptionHandler _exception;

        public BuddyWindow()
        {
            InitializeComponent();

            _factory = new Factory();
            _log = new LogHandler(rtb_log, Dispatcher);
            _exception = new ExceptionHandler(_log, Dispatcher);

            _log.AddLog("Welcome to TrackMania Training Buddy! I will carefully watch, which map are you playing and then provide you with the best replay I can find!", LogTypeEnum.Info);
            _log.AddLog("But before that, you need to configure me... Start both clients, check all settings and click Watch!", LogTypeEnum.Info);
            _log.AddLog("If you don't know what to do or you encountered any problem - check README.MD", LogTypeEnum.Info);
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lbl_listeningIntensivityLevel != null)
            {
                lbl_listeningIntensivityLevel.Content = sld_intensivity.Value;
                _factory.BuddyConfig.SensivityLevel = Int32.Parse(sld_intensivity.Value.ToString());
            }
        }
    }
}
