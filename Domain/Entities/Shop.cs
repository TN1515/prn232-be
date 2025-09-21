using Domain.Entities;

namespace Domain.Domain.Entities;

public partial class Shop
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? Province { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public decimal? RatingAverage { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public bool IsActive { get; set; }

    public string? Qrbanking { get; set; }

    public Guid? OwnerId { get; set; }


    public virtual User? Owner { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<BankSettings> BankSettings { get; set; } = new List<BankSettings>();
}
