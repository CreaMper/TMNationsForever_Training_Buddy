using LogicStorage;
using LogicStorage.Dtos.ReplayList;
using LogicStorage.Utils;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly List<TrackDataDetailsDto> _data;
        private ReplayDto? _selectedReplay;
        private ReplayDto? _lastReplay;
        private bool _sessionStop = false;
        
        public BuddyWindow(Factory factory)
        {
            InitializeComponent();

            _factory = factory;
            _log = new LogHandler(rtb_log, Dispatcher);
            _data = new List<TrackDataDetailsDto>();

            _log.AddLog("Welcome to TrackMania Training Buddy! I will carefully watch, which map are you playing and then provide you with the best replay I can find!", LogTypeEnum.Info);
            _log.AddLog("But before that, you need to configure me... Start both clients, check all settings and click Watch!", LogTypeEnum.Info);
            _log.AddLog("If you don't know what to do or you encountered any problem - check README.MD", LogTypeEnum.Info);
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void LastReplays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var replaysData = _data.FirstOrDefault(x => x.Name.Equals(lb_LastReplays.SelectedItem))?.Replays;
            if(replaysData!= null)
            {
                lv_replayData.ItemsSource = replaysData;
            }
        }

        private void BuddyStart_Click(object sender, RoutedEventArgs e)
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

                    if (_factory.Client.User != null)
                    {
                        _log.AddLog("Watch have started automatically! If you want to stop session, press Watch Stop!", LogTypeEnum.Success);
                        StartWatch();
                    }
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

        private void UserStart_Click(object sender, RoutedEventArgs e)
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
                    
                    if(_factory.Client.Buddy != null)
                    {
                        _log.AddLog("Watch have started automatically! If you want to stop session, press Watch Stop!", LogTypeEnum.Success);
                        StartWatch();
                    }
                    return;
                }
            }
            else
            {
                _log.AddLog("User Client already started! Use this button ONLY for re-run the User Client!", LogTypeEnum.Error);
            }
        }

        private void StartWatch_Click(object sender, RoutedEventArgs e)
        {
            StartWatch();
        }

        private void StartWatch()
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
            _factory.Network.Device.Close();

            Dispatcher.Invoke(() => {
                btn_startWatch.IsEnabled = true;
                btn_stopWatch.IsEnabled = false;
                btn_buddyReloadReplay.IsEnabled = false;
                chk_replayAutoLoad.IsEnabled = false;
                btn_replayLoad.IsEnabled = false;

                if (_factory.Client.Buddy.HasExited)
                {
                    lbl_buddyPid.Content = "---";
                    _factory.Client.Buddy = null;
                }
                if(_factory.Client.User.HasExited)
                {
                    lbl_userPid.Content = "---";
                    lbl_buddyLastTrack.Content = "---";
                    lbl_buddyTime.Content = "---";
                    _factory.Client.User = null;
                }
            });
        }

        private void WatchProcess()
        {
            Dispatcher.Invoke(() => {
                btn_startWatch.IsEnabled = false;
                btn_stopWatch.IsEnabled = true;
                chk_replayAutoLoad.IsEnabled = true;
            });
            _log.AddLog("Buddy has started to watch you! Start a map on a User Client and Buddy will download all data for you!", LogTypeEnum.Info);
            _factory.Network.Device.Open(DeviceModes.Promiscuous, 10);

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

                if(_data.Any(x => x.Uid.Equals(trackInfo.UID)))
                {
                    _log.AddLog("Track data already exists in Buddy! Bringing it to the top of the list...", LogTypeEnum.Info);

                    Dispatcher.Invoke(() => {
                        lbl_userMapCount.Content = Int32.Parse(lbl_userMapCount.Content.ToString()!) + 1;
                    });

                    _data.Move(_data.FindIndex(x => x.Uid.Equals(trackInfo.UID)), 0);
                    continue;
                }

                var trackIdAndSource = _factory.Request.GetTrackIdAndSource(trackInfo);
                if (trackIdAndSource == null)
                {
                    _log.AddLog("Cannot find a Track ID! Buddy won't work for this map...", LogTypeEnum.CRITICAL);
                    continue;
                }

                _log.AddLog("Track ID found!", LogTypeEnum.Success);
                _log.AddLog($"Track ID: {trackIdAndSource.TrackId} | Source: {trackIdAndSource.Source}", LogTypeEnum.Info);

                var replayList = _factory.Request.GetReplayList(trackIdAndSource);
                if (replayList == null)
                {
                    _log.AddLog("Cannot find a replay that would be updated to TMX! Buddy won't work for this map...", LogTypeEnum.CRITICAL);
                    continue;
                }

                var trackDto = new TrackDataDetailsDto()
                {
                    Uid = trackInfo.UID,
                    Author = trackInfo.AuthorName,
                    Name = Converters.TrackNameConverter(trackInfo.TrackName),
                    TMXId = Int32.Parse(trackIdAndSource.TrackId),
                    Source = trackIdAndSource.Source,
                    Replays = replayList.Select(x => Converters.TrackStatsResultDtoToReplayDtoConverter(x, trackIdAndSource.Source))
                };

                _log.AddLog("Replay data found!", LogTypeEnum.Success);
                _log.AddLog($"Best time by: {trackDto.Replays.First().Player} | Time: {trackDto.Replays.First().Time} | ReplayId: {trackDto.Replays.First().ReplayId} | Source: {trackDto.Replays.First().Source}", LogTypeEnum.Info);

                _data.Insert(0, trackDto);

                Dispatcher.Invoke(() => 
                {
                    if(chk_replayAutoLoad.IsChecked!.Value)
                        DownloadAndInjectReplay(trackDto.Replays.FirstOrDefault());
                });

                DataUpdate();
            }
        }

        private void DataUpdate()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lb_LastReplays.ItemsSource = _data.Select(x => x.Name);
                btn_replayLoad.IsEnabled = false;
                lb_LastReplays.SelectedIndex = 0;

                if (lbl_userMapCount.Content.Equals("---"))
                    lbl_userMapCount.Content = "1";
                else
                    lbl_userMapCount.Content = Int32.Parse(lbl_userMapCount.Content.ToString()!) + 1;
            }));
        }

        private void StopWatch_Click(object sender, RoutedEventArgs e)
        {
            _log.AddLog("Session stopped!", LogTypeEnum.Info);
            _sessionStop = true;
            btn_startWatch.IsEnabled = true;
            btn_stopWatch.IsEnabled = false;
            lbl_userMapCount.Content = "---";
        }

        private void ReplayLoad_Click(object sender, RoutedEventArgs e)
        {
            DownloadAndInjectReplay(_selectedReplay);
        }

        private void DownloadAndInjectReplay(ReplayDto? replay)
        {
            if(replay == null)
            {
                _log.AddLog("This track does not have any replay data!", LogTypeEnum.CRITICAL);
                return;
            }

            var downloadSuccessful = _factory.Request.DownloadReplay(replay);
            if (!downloadSuccessful)
            {
                _log.AddLog("Error occured when downloading a replay file! Perhaps buddy doesn't have access to file?", LogTypeEnum.CRITICAL);
            }
            else
            {
                _log.AddLog("Replay downloaded successful! Injecting file to buddy client...", LogTypeEnum.Success);
                lbl_buddyLastTrack.Content = replay.Player;
                lbl_buddyTime.Content = replay.Time;
                _factory.Client.InjectReplay(_factory.Client.Buddy);
                _lastReplay = replay;
                btn_buddyReloadReplay.IsEnabled = true;
            }
        }

        private void ReplayData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = lv_replayData.SelectedIndex;
            var replayPool = _data.FirstOrDefault(x => x.Name.Equals(lb_LastReplays.SelectedItem))?.Replays;
            if(replayPool!= null)
            {
                _selectedReplay = replayPool.FirstOrDefault(x => x.Rank.Equals(selectedIndex+1))!;
                if (_selectedReplay == null)
                    _selectedReplay = replayPool.FirstOrDefault()!;
                else
                    _log.AddLog($"Selected replay done by {_selectedReplay.Player} in time of {_selectedReplay.Time}", LogTypeEnum.Info);
            }

            btn_replayLoad.IsEnabled = true;
        }

        private void BuddyReloadReplay_Click(object sender, RoutedEventArgs e)
        {
            _log.AddLog("Re-injecting last replay!", LogTypeEnum.Success);
            _log.AddLog($"Last replay done by {_lastReplay?.Player} in time of {_lastReplay?.Time}", LogTypeEnum.Info);
            _factory.Client.InjectReplay(_factory.Client.Buddy);
        }

        private void SafeExit_Click(object sender, RoutedEventArgs e)
        {
            _log.AddLog($"Thanks for playing with me! I hope you enjoyed! Performing safe exit...", LogTypeEnum.Success);

            var msgResult = MessageBox.Show("Do you want to exit Buddy? All clients will be closed as well...",
                "Exit confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                if (_factory.Client.Buddy != null)
                    _factory.Client.Buddy.Kill();

                if (_factory.Client.User != null)
                    _factory.Client.User.Kill();

                Environment.Exit(1);
            }
        }
    }
}
