using EventTicket.Data.Models;
using EventTicket.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventTicket.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventTicketContext _context;

        public HomeController(EventTicketContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var events = _context.Events.ToList(); // Lấy danh sách sự kiện từ database
            return View(events);
        }
    }
}
