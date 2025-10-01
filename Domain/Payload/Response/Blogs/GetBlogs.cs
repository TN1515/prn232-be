namespace Domain.Payload.Response.Blogs
{
    public class GetBlogs
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public DateTime PublishDate { get; set; }
        public int TotalLike { get; set; }
        public int TotalComment { get; set; }

    }
}
