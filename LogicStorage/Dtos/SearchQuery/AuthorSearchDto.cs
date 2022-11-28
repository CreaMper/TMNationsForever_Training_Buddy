namespace LogicStorage.Dtos.SearchQuery{ 

    public class AuthorSearchDto
    {
        public UserSearchDto User { get; set; }
        public string Role { get; set; }
    }

}