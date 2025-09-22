using System;
using System.Collections.Generic;

namespace Domain.Domain.Entities;

public partial class Comment
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Content { get; set; } = null!;

    public Guid? ParentId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public Guid? BlogId { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual User? User { get; set; }
}
