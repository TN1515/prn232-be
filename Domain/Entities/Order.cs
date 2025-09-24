using Domain.Entities.Enum;

namespace Domain.Domain.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public decimal? TotalAmounts { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? DeliveryAddress { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverPhone { get; set; }
    public bool? IsActive { get; set; }
    public string OrderCode { get; set; }
    public PaymentMethodEnum PaymentMethod { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}
