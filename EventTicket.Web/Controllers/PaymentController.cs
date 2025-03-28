using EventTicket.Data;
using EventTicket.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventTicket.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly EventTicketContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(EventTicketContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Checkout(int eventId, int quantity)
        {
            // Kiểm tra sự kiện
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null || evt.TotalTickets < quantity)
            {
                TempData["ErrorMessage"] = "Sự kiện không tồn tại hoặc không đủ vé.";
                return RedirectToAction("Details", "Event", new { id = eventId });
            }

            // Tìm ticket liên quan đến sự kiện (giả sử mỗi sự kiện chỉ có một loại vé)
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.EventId == eventId);
            if (ticket == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy vé cho sự kiện này.";
                return RedirectToAction("Details", "Event", new { id = eventId });
            }

            // Tính tổng tiền
            double totalAmount = (double)(evt.Price * quantity);

            // Tạo đơn hàng tạm thời
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var order = new Order
            {
                UserId = userId,
                EventId = eventId,
                TicketId = ticket.TicketId,
                Quantity = quantity,
                TotalAmount = (decimal)totalAmount,
                OrderDate = DateTime.Now,
                PaymentStatus = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsDeleted = false
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Gọi API VnpayController để tạo URL thanh toán
            var client = _httpClientFactory.CreateClient("VnpayClient");
            var description = $"Thanh toan don hang {order.OrderId} cho su kien {evt.Name}";
            var requestUrl = $"/api/Vnpay/CreatePaymentUrl?paymentId={order.OrderId}&money={totalAmount}&description={Uri.EscapeDataString(description)}";
            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var paymentUrl = await response.Content.ReadAsStringAsync();
                return Redirect(paymentUrl);
            }

            TempData["ErrorMessage"] = "Không thể tạo URL thanh toán. Vui lòng thử lại.";
            return RedirectToAction("Details", "Event", new { id = eventId });
        }

        public async Task<IActionResult> Return()
        {
            if (Request.QueryString.HasValue)
            {
                var client = _httpClientFactory.CreateClient("VnpayClient");
                var callbackUrl = $"/api/Vnpay/Callback?{Request.QueryString}";
                var response = await client.GetAsync(callbackUrl);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResultJson = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(paymentResultJson);
                    var paymentResult = document.RootElement;

                    bool isSuccess = paymentResult.GetProperty("isSuccess").GetBoolean();
                    int orderId = (int)paymentResult.GetProperty("paymentId").GetInt64();

                    var order = await _context.Orders.FindAsync(orderId);
                    if (order == null)
                    {
                        TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                        return RedirectToAction("PurchasedTickets", "Profile");
                    }

                    if (isSuccess)
                    {
                        TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng của bạn đã được xác nhận.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi khi xử lý kết quả thanh toán.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán.";
            }

            return RedirectToAction("PurchasedTickets", "Profile");
        }
    }
}