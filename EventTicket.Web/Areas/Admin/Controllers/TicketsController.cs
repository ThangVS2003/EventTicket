using EventTicket.Data;
using EventTicket.Data.Models;
using EventTicket.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventTicket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
    {
        private readonly EventTicketContext _context;

        public TicketsController(EventTicketContext context)
        {
            _context = context;
        }

        // GET: Admin/Tickets (với phân trang và tìm kiếm)
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var tickets = _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.IsDeleted == false || t.IsDeleted == null);

            if (!string.IsNullOrEmpty(searchString))
            {
                tickets = tickets.Where(t => t.SeatNumber.Contains(searchString) || t.Event.Name.Contains(searchString));
            }

            var paginatedTickets = await PaginatedList<Ticket>.CreateAsync(tickets.OrderBy(t => t.TicketId), page, pageSize);
            ViewBag.SearchString = searchString;
            return View(paginatedTickets);
        }

        // GET: Admin/Tickets/Create
        public IActionResult Create()
        {
            ViewBag.EventId = new SelectList(_context.Events
                .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                "EventId", "Display");
            return View(new TicketViewModel { Status = "Available" });
        }

        // POST: Admin/Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketViewModel ticketViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(", ", errors);
                ViewBag.EventId = new SelectList(_context.Events
                    .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                    .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                    "EventId", "Display", ticketViewModel.EventId);
                return View(ticketViewModel);
            }

            var eventEntity = await _context.Events.FindAsync(ticketViewModel.EventId);
            if (eventEntity == null)
            {
                TempData["ErrorMessage"] = "Sự kiện không tồn tại.";
                ViewBag.EventId = new SelectList(_context.Events
                    .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                    .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                    "EventId", "Display", ticketViewModel.EventId);
                return View(ticketViewModel);
            }

            var ticket = new Ticket
            {
                EventId = ticketViewModel.EventId,
                Price = eventEntity.Price,
                SeatNumber = ticketViewModel.SeatNumber,
                Status = "Available", // Mặc định
                CreatedDate = DateTime.Now
            };

            _context.Add(ticket);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm vé thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null || ticket.IsDeleted == true) return NotFound();

            var ticketViewModel = new TicketViewModel
            {
                TicketId = ticket.TicketId,
                EventId = ticket.EventId,
                Price = ticket.Price,
                SeatNumber = ticket.SeatNumber,
                Status = ticket.Status,
                CreatedDate = ticket.CreatedDate,
                UpdatedDate = ticket.UpdatedDate,
                IsDeleted = ticket.IsDeleted
            };

            ViewBag.EventId = new SelectList(_context.Events
                .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                "EventId", "Display", ticketViewModel.EventId);
            return View(ticketViewModel);
        }

        // POST: Admin/Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TicketViewModel ticketViewModel)
        {
            if (id != ticketViewModel.TicketId) return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(", ", errors);
                ViewBag.EventId = new SelectList(_context.Events
                    .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                    .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                    "EventId", "Display", ticketViewModel.EventId);
                return View(ticketViewModel);
            }

            try
            {
                var eventEntity = await _context.Events.FindAsync(ticketViewModel.EventId);
                if (eventEntity == null)
                {
                    TempData["ErrorMessage"] = "Sự kiện không tồn tại.";
                    ViewBag.EventId = new SelectList(_context.Events
                        .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                        .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                        "EventId", "Display", ticketViewModel.EventId);
                    return View(ticketViewModel);
                }

                var existingTicket = await _context.Tickets.FindAsync(ticketViewModel.TicketId);
                if (existingTicket == null) return NotFound();

                existingTicket.EventId = ticketViewModel.EventId;
                existingTicket.SeatNumber = ticketViewModel.SeatNumber;
                existingTicket.Status = ticketViewModel.Status;
                existingTicket.Price = eventEntity.Price;
                existingTicket.UpdatedDate = DateTime.Now;

                _context.Update(existingTicket);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sửa vé thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticketViewModel.TicketId)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu: " + ex.Message;
                ViewBag.EventId = new SelectList(_context.Events
                    .Where(e => e.IsDeleted == false || e.IsDeleted == null)
                    .Select(e => new { e.EventId, Display = $"{e.Name} - {e.Price.ToString("N0")} VND" }),
                    "EventId", "Display", ticketViewModel.EventId);
                return View(ticketViewModel);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketId == id && (m.IsDeleted == false || m.IsDeleted == null));
            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // POST: Admin/Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.IsDeleted = true;
                ticket.UpdatedDate = DateTime.Now;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa vé thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(t => t.TicketId == id && (t.IsDeleted == false || t.IsDeleted == null));
        }
    }
}