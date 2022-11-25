using System.Collections.Generic;

namespace LogicStorage.Dtos
{
    public class TrackStatsDto
    {
        public bool More { get; set; }
        public List<TrackStatsResultDto> Results { get; set; }
    }
}