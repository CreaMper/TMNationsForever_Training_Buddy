using LogicStorage.Utils;

namespace LogicStorage.Dtos.Track
{
    public class TrackAndSourceDto
    {
        public string TrackId { get; set; }
        public ApiTypeEnum Source { get; set; }
    }
}
