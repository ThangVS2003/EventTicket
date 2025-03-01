using EventTicket.Data.Models;
using EventTicket.Service.Service;
using Microsoft.AspNetCore.Mvc;

namespace EventTicket.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly EventService _eventService;

        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        // Hiển thị tất cả sự kiện
        public IActionResult Index(int? month, string? location, decimal? minPrice, decimal? maxPrice)
        {
            List<Event> events;

            if (month.HasValue || !string.IsNullOrEmpty(location) || minPrice.HasValue || maxPrice.HasValue)
            {
                events = _eventService.FilterEvents(month, location, minPrice, maxPrice);
            }
            else
            {
                events = _eventService.GetAllEvents();
            }

            return View(events);
        }
    }
}
