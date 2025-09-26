using System;
using System.Collections.Generic;

namespace Domain.Domain.Entities;

public partial class LikeBlog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? BlogId { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual User? User { get; set; }
}
