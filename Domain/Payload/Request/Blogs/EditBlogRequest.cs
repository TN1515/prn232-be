namespace Domain.Payload.Request.Blogs
{
    public class EditBlogRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
    }
}
