using System;
using System.Collections.Generic;

namespace Domain.Domain.Entities;

public partial class Cart
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public decimal TotalAmounts { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User? User { get; set; }
}
