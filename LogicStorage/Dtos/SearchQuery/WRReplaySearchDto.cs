namespace LogicStorage.Dtos.SearchQuery{ 

    public class WRReplaySearchDto
    {
        public UserSearchDto User { get; set; }
        public int ReplayTime { get; set; }
        public int ReplayScore { get; set; }
    }

}