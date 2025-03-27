using EventTicket.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EventTicket.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventTicketContext _context;

        public HomeController(EventTicketContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, int? categoryId, int pageNumber = 1, int pageSize = 6)
        {
            // Lấy danh sách categories để hiển thị trong menu
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsDeleted == false)
                .ToList();

            // Lấy danh sách sự kiện
            var events = _context.Events.AsQueryable();

            // Lọc theo searchString nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Name.Contains(searchString) || e.Description.Contains(searchString));
            }

            // Lọc theo categoryId nếu có
            if (categoryId.HasValue)
            {
                events = events.Where(e => e.CategoryId == categoryId.Value);
            }

            // Tính toán phân trang
            int totalEvents = events.Count(); // Tổng số sự kiện sau khi lọc
            int totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize); // Tổng số trang

            // Áp dụng phân trang
            var pagedEvents = events
                .Skip((pageNumber - 1) * pageSize) // Bỏ qua các bản ghi của các trang trước
                .Take(pageSize) // Lấy số bản ghi cho trang hiện tại
                .ToList();

            // Truyền thông tin phân trang vào ViewBag để sử dụng trong view
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;

            return View(pagedEvents);
        }
    }
}