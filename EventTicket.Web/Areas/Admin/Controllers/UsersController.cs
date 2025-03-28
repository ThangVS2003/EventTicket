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
    public class UsersController : Controller
    {
        private readonly EventTicketContext _context;

        public UsersController(EventTicketContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsDeleted == false || u.IsDeleted == null)
                .ToListAsync();
            return View(users);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "Name");
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Phone,Username,Password,RoleId,Money")] User user)
        {
            if (ModelState.IsValid)
            {
                user.CreatedDate = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted == true) return NotFound();

            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
            return View(user);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,Phone,Username,Password,RoleId,Money,CreatedDate")] User user)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    user.UpdatedDate = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sửa người dùng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "Name", user.RoleId);
            return View(user);
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id && (m.IsDeleted == false || m.IsDeleted == null));
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.UpdatedDate = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(u => u.UserId == id && (u.IsDeleted == false || u.IsDeleted == null));
        }
    }
}