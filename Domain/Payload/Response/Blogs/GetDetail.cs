namespace Domain.Payload.Response.Blogs
{
    public class GetDetail
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string Image { get; set; } = null!;

        public string Author { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime? ModifyDate { get; set; }
        public int TotalLike { get; set; }
        public int TotalComment { get; set; }
        public bool IsActive { get; set; }
    }
}
