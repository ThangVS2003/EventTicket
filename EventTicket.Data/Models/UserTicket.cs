using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class UserTicket
{
    public int UserTicketId { get; set; }

    public int UserId { get; set; }

    public int TicketId { get; set; }

    public int OrderId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
