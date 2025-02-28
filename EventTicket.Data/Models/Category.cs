using System;
using System.Collections.Generic;

namespace EventTicket.Data.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
