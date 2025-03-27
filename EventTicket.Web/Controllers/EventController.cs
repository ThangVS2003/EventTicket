using EventTicket.Data.Models;
using EventTicket.Service.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventTicket.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly EventService _eventService;

        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> EventDetail(int id)
        {
            var eventDetail = await _eventService.GetEventByIdAsync(id);

            if (eventDetail == null)
            {
                return NotFound();
            }

            return View(eventDetail); // Chỉ định view cụ thể
        }
    }
}