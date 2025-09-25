namespace Domain.Domain.Entities;

public partial class ProductFeedback
{
    public Guid Id { get; set; }

    public decimal Rating { get; set; }

    public string? Content { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ProductId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public bool IsActive { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
