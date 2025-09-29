using System;
using System.Collections.Generic;

namespace Domain.Domain.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public byte[] Expired { get; set; } = null!;

    public decimal TotalAmounts { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OrderId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
