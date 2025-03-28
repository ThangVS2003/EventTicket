using EventTicket.Data;
using EventTicket.Data.Models;
using EventTicket.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventTicket.Web.Controllers
{
    [Authorize] // Yêu cầu người dùng phải đăng nhập để truy cập
    public class ProfileController : Controller
    {
        private readonly EventTicketContext _context;

        public ProfileController(EventTicketContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy UserId từ claims
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted != true);

            if (user == null)
            {
                return NotFound();
            }

            // Map thông tin người dùng sang ViewModel
            var model = new ProfileViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Username = user.Username,
                Money = user.Money,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
                RoleName = user.Role?.Name ?? "User"
            };

            return View(model);
        }
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Trang Đổi mật khẩu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted != true);

            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra mật khẩu hiện tại
            if (user.Password != model.CurrentPassword) // Lưu ý: Nên băm mật khẩu trong thực tế
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            // Cập nhật mật khẩu mới
            user.Password = model.NewPassword; // Lưu ý: Nên băm mật khẩu trong thực tế
            user.UpdatedDate = DateTime.Now;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        // Trang Lịch sử giao dịch
        public async Task<IActionResult> PurchasedTickets()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var userTickets = await _context.UserTickets
                .Include(ut => ut.Ticket)
                    .ThenInclude(t => t.Event)
                .Include(ut => ut.Order)
                .Where(ut => ut.UserId == userId && (ut.IsDeleted == false || ut.IsDeleted == null))
                .OrderByDescending(ut => ut.OrderId)
                .ToListAsync();

            return View(userTickets);
        }

        public async Task<IActionResult> TransactionHistory()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var paymentHistories = await _context.PaymentHistories
                .Include(ph => ph.Order)
                    .ThenInclude(o => o.Event)
                .Where(ph => ph.UserId == userId && (ph.IsDeleted == false || ph.IsDeleted == null))
                .OrderByDescending(ph => ph.OrderId)
                .ToListAsync();

            return View(paymentHistories);
        }
    }
}