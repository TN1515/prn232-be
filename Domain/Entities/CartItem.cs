namespace Domain.Domain.Entities;

public partial class CartItem
{
    public Guid Id { get; set; }

    public Guid? CartId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
    public string Note { get; set; }

    public decimal TotalAmounts { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Product? Product { get; set; }
}
