using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;

namespace Domain.Domain.Entities;

public partial class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal CostPrice { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Material { get; set; } = null!;
    public string CommonImage { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public string Category { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();
    public virtual ICollection<ProductImages>? ProductImages { get; set; }
    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();


    [Column("ShopID")]
    public Guid? ShopID { get; set; }

    public virtual Shop? Shop { get; set; }
}
