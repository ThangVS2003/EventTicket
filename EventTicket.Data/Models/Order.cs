using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public int TicketId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? OrderDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();

    public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
