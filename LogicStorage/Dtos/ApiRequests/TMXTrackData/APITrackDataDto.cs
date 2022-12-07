using System.Collections.Generic;

namespace LogicStorage.Dtos.ApiRequests.TMXTrackData
{
    public class APITrackDataDto
    {
        public bool More { get; set; }
        public List<APITrackDataResultDto> Results { get; set; }
    }
}