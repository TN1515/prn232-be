namespace Domain.Domain.Entities;

public partial class Blog
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Image { get; set; } = null!;

    public Guid Author { get; set; }

    public DateTime PublishDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public bool IsActive { get; set; }

    public virtual User AuthorNavigation { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<LikeBlog> LikeBlogs { get; set; } = new List<LikeBlog>();
}
