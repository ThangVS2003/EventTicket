using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int EventId { get; set; }

    public decimal Price { get; set; }

    public string? SeatNumber { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
