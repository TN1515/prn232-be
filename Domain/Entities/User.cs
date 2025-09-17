using Domain.Entities.Enum;

namespace Domain.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? Dbo { get; set; }

    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public RoleEnum Role { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<LikeBlog> LikeBlogs { get; set; } = new List<LikeBlog>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();
}
