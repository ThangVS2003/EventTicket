using EventTicket.Data;
using EventTicket.Data.Models;
using EventTicket.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm namespace này
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventTicket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly EventTicketContext _context;

        public DashboardController(EventTicketContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            var currentYear = DateTime.Now.Year;

            // Tổng số vé đã bán
            model.TotalTicketsSold = await _context.UserTickets
                .AsNoTracking()
                .CountAsync(ut => ut.IsDeleted == false || ut.IsDeleted == null);

            // Tổng số sự kiện
            model.TotalEvents = await _context.Events
                .AsNoTracking()
                .CountAsync(e => e.IsDeleted == false || e.IsDeleted == null);

            // Tổng số người dùng
            model.TotalUsers = await _context.Users
                .AsNoTracking()
                .CountAsync();

            // Tổng doanh thu
            model.TotalRevenue = await _context.PaymentHistories
                .AsNoTracking()
                .Where(ph => ph.Status == "Completed" && (ph.IsDeleted == false || ph.IsDeleted == null))
                .SumAsync(ph => ph.Amount);

            // Doanh thu theo tháng
            var monthlyRevenues = await _context.PaymentHistories
                .AsNoTracking()
                .Where(ph => ph.Status == "Completed" && (ph.IsDeleted == false || ph.IsDeleted == null) && ph.PaymentDate.HasValue)
                .GroupBy(ph => new { ph.PaymentDate.Value.Year, ph.PaymentDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(ph => ph.Amount)
                })
                .ToListAsync();

            foreach (var revenue in monthlyRevenues.Where(r => r.Year == currentYear))
            {
                model.MonthlyRevenues[revenue.Month - 1] = revenue.Revenue;
            }

            // Tính tỷ lệ tăng trưởng doanh thu
            var currentMonth = DateTime.Now.Month;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var currentMonthRevenue = model.MonthlyRevenues[currentMonth - 1];
            var previousMonthRevenue = model.MonthlyRevenues[previousMonth - 1];

            model.PercentGrowth = previousMonthRevenue > 0
                ? (currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue * 100
                : (currentMonthRevenue > 0 ? 100 : 0);

            // Top sự kiện nổi bật
            model.TopEvents = await _context.UserTickets
                .AsNoTracking()
                .Include(ut => ut.Ticket)
                    .ThenInclude(t => t.Event)
                .Where(ut => ut.IsDeleted == false || ut.IsDeleted == null)
                .GroupBy(ut => ut.Ticket.Event)
                .Select(g => new Event
                {
                    EventId = g.Key.EventId,
                    Name = g.Key.Name,
                    Price = g.Key.Price,
                    Image = g.Key.Image,
                    TotalTickets = g.Count()
                })
                .OrderByDescending(e => e.TotalTickets)
                .Take(5)
                .ToListAsync();

            ViewBag.MonthLabels = model.AllMonths;
            ViewBag.MonthlyRevenues = model.MonthlyRevenues;

            return View(model);
        }
    }
}