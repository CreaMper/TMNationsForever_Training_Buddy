using LogicStorage.Utils;
using System;
using System.Collections.Generic;

namespace LogicStorage.Dtos.ReplayList
{
    public class TrackDto
    {
        public string Name { get; set; }
        public int TMXId { get; set; }
        public Guid Guid { get; set; }
        public ApiTypeEnum Source { get; set; }
        public string Author { get; set; }
        public string Time { get; set; }
        public List<ReplayDto> Replays { get; set; }
    }
}
