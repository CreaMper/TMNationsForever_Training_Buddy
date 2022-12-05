using LogicStorage;
using LogicStorage.Dtos.ReplayList;
using LogicStorage.Dtos.TrackData;
using LogicStorage.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<TrackDto> _data;

        public BuddyWindow()
        {
            InitializeComponent();

            _factory = new Factory();
            _log = new LogHandler(rtb_log, Dispatcher);
            _exception = new ExceptionHandler(_log, Dispatcher);
            _data = new List<TrackDto>();

            _log.AddLog("Welcome to TrackMania Training Buddy! I will carefully watch, which map are you playing and then provide you with the best replay I can find!", LogTypeEnum.Info);
            _log.AddLog("But before that, you need to configure me... Start both clients, check all settings and click Watch!", LogTypeEnum.Info);
            _log.AddLog("If you don't know what to do or you encountered any problem - check README.MD", LogTypeEnum.Info);

            //Fake track info
            #region fakeTrackData
            var fakeTrackData = new List<TrackDto>()
            {
                new TrackDto()
                {
                    Author = "Test author!2@",
                    Guid = Guid.NewGuid(),
                    Name = "Test map name",
                    Source = ApiTypeEnum.TMNF,
                    Time = "231112",
                    TMXId = 74234443,
                    Replays = new List<ReplayDto>()
                    {
                       new ReplayDto()
                       {
                           Player = "Test one replay",
                           ReplayId = 1122663,
                           Source = ApiTypeEnum.TMNF,
                           Time = "22412",
                           Rank = 1
                       },
                       new ReplayDto()
                       {
                           Player = "MoreReplayyys player",
                           ReplayId = 86523,
                           Source = ApiTypeEnum.TMNF,
                           Time = "986111",
                           Rank = 2
                       }
                    }
                },
                new TrackDto()
                {
                    Author = "Another test author",
                    Guid = Guid.NewGuid(),
                    Name = "Mapa dla gurbych grubasów",
                    Source = ApiTypeEnum.NATIONS,
                    Time = "98371",
                    TMXId = 234661,
                    Replays = new List<ReplayDto>()
                    {
                       new ReplayDto()
                       {
                           Player = "Test oneee replay",
                           ReplayId = 1112362623,
                           Source = ApiTypeEnum.NATIONS,
                           Time = "224112",
                           Rank = 1
                       },
                       new ReplayDto()
                       {
                           Player = "Ecven morrre",
                           ReplayId = 8652321,
                           Source = ApiTypeEnum.NATIONS,
                           Time = "123333",
                           Rank = 2
                       }
                    }
                }
            };

            _data = fakeTrackData;
            #endregion

            #region DataInit

            //lb_LastReplays.ItemsSource = fakeTrackData.Select(x => x.Name);

            #endregion


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

        private void lb_LastReplays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var replaysData = _data.FirstOrDefault(x => x.Name.Equals(lb_LastReplays.SelectedItem))?.Replays;
            if(replaysData!= null)
            {
                lv_replayData.ItemsSource = replaysData;
            }

        }
    }
}
