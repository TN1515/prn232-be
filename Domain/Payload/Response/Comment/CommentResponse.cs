namespace Domain.Payload.Response.Blog
{
    public class CommentResponse
    {
        public Guid ID { get; set; }
        public string Content { get; set; }
        public Guid UserID { get; set; }
        public string? Avatar { get; set; }
        public string DisplayName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool HasReplies { get; set; }
        public int TotalReplies { get; set; }
        public List<RepliesResponse> Replies { get; set; } = new();
    }

    public class RepliesResponse
    {
        public Guid ID { get; set; }
        public string Content { get; set; }
        public string? Avatar { get; set; }
        public Guid UserID {  get; set; }
        public string DisplayName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool HasReplies { get; set; }
    }
}
