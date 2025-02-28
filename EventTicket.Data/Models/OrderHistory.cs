using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class OrderHistory
{
    public int OrderHistoryId { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? DateTime { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;
}
