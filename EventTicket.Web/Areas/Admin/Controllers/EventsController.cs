using EventTicket.Data;
using EventTicket.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventTicket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly EventTicketContext _context;

        public EventsController(EventTicketContext context)
        {
            _context = context;
        }

        // GET: Admin/Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Category)
                .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                .ToListAsync();
            return View(events);
        }

        // GET: Admin/Events/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.Where(c => c.IsDeleted == false || c.IsDeleted == null), "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Image,EventDate,EndDate,Location,TotalTickets,Price,CategoryId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                @event.CreatedDate = DateTime.Now;
                @event.CreatedBy = User.Identity.Name;
                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sự kiện thành công!"; // Thêm thông báo
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories.Where(c => c.IsDeleted == false || c.IsDeleted == null), "CategoryId", "Name", @event.CategoryId);
            return View(@event);
        }

        // GET: Admin/Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null || @event.IsDeleted == true) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories.Where(c => c.IsDeleted == false || c.IsDeleted == null), "CategoryId", "Name", @event.CategoryId);
            return View(@event);
        }

        // POST: Admin/Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Name,Description,Image,EventDate,EndDate,Location,TotalTickets,Price,CategoryId,CreatedDate,CreatedBy")] Event @event)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    @event.UpdatedDate = DateTime.Now;
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sửa sự kiện thành công!"; // Thêm thông báo
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories.Where(c => c.IsDeleted == false || c.IsDeleted == null), "CategoryId", "Name", @event.CategoryId);
            return View(@event);
        }

        // GET: Admin/Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.EventId == id && (m.IsDeleted == false || m.IsDeleted == null));
            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                @event.IsDeleted = true;
                @event.UpdatedDate = DateTime.Now;
                _context.Update(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sự kiện thành công!"; // Thêm thông báo
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id && (e.IsDeleted == false || e.IsDeleted == null));
        }
        // Trong EventsController
        [HttpGet]
        public async Task<IActionResult> GetEventPrice(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null) return NotFound();
            return Json(new { price = eventEntity.Price.ToString("N0") });
        }
    }
}