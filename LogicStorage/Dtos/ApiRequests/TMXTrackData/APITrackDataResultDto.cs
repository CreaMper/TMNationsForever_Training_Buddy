using System;

namespace LogicStorage.Dtos.ApiRequests.TMXTrackData
{
    public class APITrackDataResultDto
    {
        public int ReplayId { get; set; }
        public APITrackDataUserDto User { get; set; }
        public int ReplayTime { get; set; }
        public int ReplayScore { get; set; }
        public int ReplayRespawns { get; set; }
        public DateTime TrackAt { get; set; }
        public int Score { get; set; }
        public APITrackDataTrackTypeDto Track { get; set; }
        public int Position { get; set; }
        public int IsBest { get; set; }
        public int IsLeaderboard { get; set; }
        public DateTime ReplayAt { get; set; }
    }
}