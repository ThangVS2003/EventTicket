using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class PaymentHistory
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
