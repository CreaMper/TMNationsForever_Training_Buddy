using LogicStorage.Utils;
using System.Collections.Generic;

namespace LogicStorage.Dtos.ReplayList
{
    public class TrackDto
    {
        public string Name { get; set; }
        public int TMXId { get; set; }
        public string Uid { get; set; }
        public ApiTypeEnum Source { get; set; }
        public string Author { get; set; }
        public string Time { get; set; }
        public IEnumerable<ReplayDto> Replays { get; set; }
    }
}
