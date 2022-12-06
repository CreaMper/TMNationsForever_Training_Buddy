using LogicStorage;
using LogicStorage.Dtos.ReplayList;
using LogicStorage.Dtos.TrackData;
using LogicStorage.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        private bool _sessionStop = false;

        public BuddyWindow(Factory factory)
        {
            InitializeComponent();

            _factory = factory;
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

        private void btn_buddyStart_Click(object sender, RoutedEventArgs e)
        {
            if (_factory.Client.Buddy == null || _factory.Client.Buddy.HasExited)
            {
                _log.AddLog("Starting Buddy Client...", LogTypeEnum.Info);

                _factory.Client.Buddy = new Process
                {
                    StartInfo = { FileName = "TmForever.exe" }
                };
                _factory.Client.Buddy.Start();

                if (!_factory.Client.Buddy.HasExited)
                {
                    new Thread(UpdateBuddyWindowName).Start();
                    btn_buddyStart.IsEnabled = false;

                    lbl_buddyPid.Content = _factory.Client.Buddy.Id.ToString();
                    _log.AddLog($"Found an Trackmania process! with PID {_factory.Client.Buddy.Id}", LogTypeEnum.Info);
                    _log.AddLog("Please, make sure that game is in WINDOWED mode!", LogTypeEnum.Info);
                    return;
                }
            }
            else
            {
                _log.AddLog("Buddy Client already started! Use this button ONLY for re-run the Buddy Client!", LogTypeEnum.Error);
            }
        }

        private void UpdateBuddyWindowName()
        {
            Thread.Sleep(3000);
            _factory.Importer.UseSetWindowText(_factory.Client.Buddy.MainWindowHandle, $"TM Training Buddy Client ( PID: {_factory.Client.Buddy.Id} )");
            Dispatcher.Invoke(()=>{
                btn_buddyStart.IsEnabled = true;
                _log.AddLog("Buddy Client started!", LogTypeEnum.Success);
            });
        }

        private void UpdateUserWindowName()
        {
            Thread.Sleep(3000);
            _factory.Importer.UseSetWindowText(_factory.Client.User.MainWindowHandle, $"TM Training User Client ( PID: {_factory.Client.User.Id} )");

            Dispatcher.Invoke(() => {
                btn_userStart.IsEnabled = true;
                _log.AddLog("User Client started!", LogTypeEnum.Success);
            });
        }

        private void btn_userStart_Click(object sender, RoutedEventArgs e)
        {
            if (_factory.Client.User == null || _factory.Client.User.HasExited)
            {
                _log.AddLog("Starting User Client...", LogTypeEnum.Info);

                _factory.Client.User = new Process
                {
                    StartInfo = { FileName = "TmForever.exe" }
                };
                _factory.Client.User.Start();

                if (!_factory.Client.User.HasExited)
                {
                    new Thread(UpdateUserWindowName).Start();
                    btn_userStart.IsEnabled = false;

                    lbl_userPid.Content = _factory.Client.User.Id.ToString();
                    _log.AddLog($"Found an Trackmania process! with PID {_factory.Client.User.Id}", LogTypeEnum.Info);
                    _log.AddLog("Please, make sure that game is in WINDOWED mode!", LogTypeEnum.Info);
                    return;
                }
            }
            else
            {
                _log.AddLog("User Client already started! Use this button ONLY for re-run the User Client!", LogTypeEnum.Error);
            }
        }

        private void btn_startWatch_Click(object sender, RoutedEventArgs e)
        {
            if (_factory.Client.Buddy == null || _factory.Client.User == null)
            {
                _log.AddLog("You must start both clients (Buddy and User) before session can be started!", LogTypeEnum.Error);
                return;
            }
            else 
            {
                _sessionStop = false;
                new Thread(WatchProcess).Start();
            }
        }

        private void BreakWatchClientClose()
        {
            _sessionStop = true;
            Dispatcher.Invoke(() => {
                btn_startWatch.IsEnabled = true;
                btn_stopWatch.IsEnabled = false;

                if (_factory.Client.Buddy.HasExited)
                {
                    lbl_buddyPid.Content = "---";
                    _factory.Client.Buddy = null;
                }
                else
                {
                    lbl_userPid.Content = "---";
                    _factory.Client.User = null;
                }
            });
        }

        private void WatchProcess()
        {
            Dispatcher.Invoke(() => {
                btn_startWatch.IsEnabled = false;
                btn_stopWatch.IsEnabled = true;
            });
            _log.AddLog("Buddy has started to watch you! Start a map on a User Client and Buddy will download all data for you!", LogTypeEnum.Info);

            while (true)
            {
                if (_factory.Client.Buddy.HasExited || _factory.Client.User.HasExited)
                {
                    _log.AddLog("User or Buddy client has been closed!", LogTypeEnum.CRITICAL);
                    _log.AddLog("Please, start it once again!", LogTypeEnum.CRITICAL);
                    BreakWatchClientClose();
                    break;
                }
                if (_sessionStop)
                    break;

                var packetData = PacketSniffer.Sniff(_factory.Network);
                if (packetData == null)
                    continue;

                var trackInfo = Converters.PacketStringToTrackDataConverter(packetData);
                if (trackInfo == null)
                    continue;

                _log.AddLog("Found a new map Packet!", LogTypeEnum.Success);
                _log.AddLog($"Author: {trackInfo.AuthorName} | Trackname: {Converters.TrackNameConverter(trackInfo.TrackName)} | UID: {trackInfo.UID}", LogTypeEnum.Info);


/*
                var packetData = PacketSniffer.Sniff(_factory.Network);
                if (packetData == null)
                    continue;

                var trackInfo = Converters.PacketStringToTrackDataConverter(packetData);
                if (trackInfo == null)
                    continue;

                _log.AddLog("Found a new map Packet!", LogTypeEnum.Success);
                _log.AddLog($"Author: {trackInfo.AuthorName} | Trackname: {Converters.TrackNameConverter(trackInfo.TrackName)} | UID: {trackInfo.UID}", LogTypeEnum.Info);

                var trackIdAndSource = _factory.Request.GetTrackIdAndSource(trackInfo);
                if (trackIdAndSource == null)
                {
                    _log.AddLog("Cannot find a Track ID! Buddy won't work for this map...", LogTypeEnum.CRITICAL);
                    continue;
                }

                _log.AddLog("Track ID found!", LogTypeEnum.Success);
                _log.AddLog($"Track ID: {trackIdAndSource.TrackId} | Source: {trackIdAndSource.Source}", LogTypeEnum.Info);

                var replayDataAndSource = _factory.Request.GetReplayId(trackIdAndSource);
                if (replayDataAndSource == null)
                {
                    _log.AddLog("Cannot find a replay that would be updated to TMX! Buddy won't work for this map...", LogTypeEnum.CRITICAL);
                    continue;
                }

                _log.AddLog("Replay ID found!", LogTypeEnum.Success);
                _log.AddLog($"Replay by: {replayDataAndSource.Author} | Time: {replayDataAndSource.Time} | ReplayId: {replayDataAndSource.ReplayId} | Source: {replayDataAndSource.Source}", LogTypeEnum.Info);

                var downloadSuccessful = _factory.Request.DownloadReplay(replayDataAndSource);
                if (!downloadSuccessful)
                {
                    _log.AddLog("Error occured when downloading a replay file! Perhaps buddy doesn't have access to file?", LogTypeEnum.CRITICAL);
                    continue;
                }
                else
                {
                    _log.AddLog("Replay downloaded successful! Injecting file to buddy client...", LogTypeEnum.Success);
                    _factory.Client.InjectReplay(_factory.Client.Buddy, replayDataAndSource);
                    continue;
                }*/
            }
        }

        private void btn_stopWatch_Click(object sender, RoutedEventArgs e)
        {
            _log.AddLog("Session stopped!", LogTypeEnum.Info);
            _sessionStop = true;
            btn_startWatch.IsEnabled = true;
            btn_stopWatch.IsEnabled = false;
        }
    }
}
