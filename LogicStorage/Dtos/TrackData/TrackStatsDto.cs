using System.Collections.Generic;

namespace LogicStorage.Dtos.TrackData
{
    public class TrackStatsDto
    {
        public bool More { get; set; }
        public List<TrackStatsResultDto> Results { get; set; }
    }
}