using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Image { get; set; }

    public DateTime EventDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Location { get; set; }

    public int TotalTickets { get; set; }

    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
