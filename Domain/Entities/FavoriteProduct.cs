using System;
using System.Collections.Generic;

namespace Domain.Domain.Entities;

public partial class FavoriteProduct
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual User User { get; set; } = null!;

}
