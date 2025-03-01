using EventTicket.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventTicket.Service.Service
{
    public class EventService
    {
        private readonly EventTicketContext _context;

        public EventService(EventTicketContext context)
        {
            _context = context;
        }

        public List<Event> GetAllEvents()
        {
            return _context.Events.Where(e => e.IsDeleted == false).ToList();
        }

        public List<Event> FilterEvents(int? month, string? location, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Events.Where(e => e.IsDeleted == false);

            if (month.HasValue)
                query = query.Where(e => e.EventDate.Month == month.Value);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(e => e.Location.Contains(location));

            if (minPrice.HasValue)
                query = query.Where(e => e.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(e => e.Price <= maxPrice.Value);

            return query.ToList();
        }
    }
}
