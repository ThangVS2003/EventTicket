using EventTicket.Data.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Event> GetEventByIdAsync(int eventId)
        {
            return await _context.Events
                .Include(e => e.Category) // Bao gồm thông tin Category nếu cần
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.IsDeleted != true);
        }
    }
}
