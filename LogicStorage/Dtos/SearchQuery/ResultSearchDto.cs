using System.Collections.Generic; 
namespace LogicStorage.Dtos.SearchQuery{ 

    public class ResultSearchDto
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; }
        public List<AuthorSearchDto> Authors { get; set; }
        public List<int> Tags { get; set; }
        public int AuthorTime { get; set; }
        public int Routes { get; set; }
        public int Difficulty { get; set; }
        public int Environment { get; set; }
        public int Car { get; set; }
        public int PrimaryType { get; set; }
        public int Mood { get; set; }
        public int Awards { get; set; }
        public bool HasThumbnail { get; set; }
        public List<object> Images { get; set; }
        public bool IsPublic { get; set; }
        public WRReplaySearchDto WRReplay { get; set; }
        public int ReplayType { get; set; }
        public UploaderSearchDto Uploader { get; set; }
    }

}