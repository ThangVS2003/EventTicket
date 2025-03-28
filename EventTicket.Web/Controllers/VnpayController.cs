using Microsoft.AspNetCore.Mvc;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;
using EventTicket.Data;
using EventTicket.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EventTicket.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly EventTicketContext _context;

        public VnpayController(IVnpay vnPayservice, IConfiguration configuration, EventTicketContext context)
        {
            _vnpay = vnPayservice;
            _configuration = configuration;
            _context = context;

            var tmnCode = _configuration["Vnpay:TmnCode"];
            var hashSecret = _configuration["Vnpay:HashSecret"];
            var baseUrl = _configuration["Vnpay:BaseUrl"];
            var callbackUrl = _configuration["Vnpay:CallbackUrl"];

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(callbackUrl))
            {
                throw new ArgumentException($"Thiếu cấu hình VNPay: TmnCode={tmnCode}, HashSecret={(string.IsNullOrEmpty(hashSecret) ? "null" : "****")}, BaseUrl={baseUrl}, CallbackUrl={callbackUrl}");
            }

            _vnpay.Initialize(tmnCode, hashSecret, baseUrl, callbackUrl);
        }

        [HttpGet("CreatePaymentUrl")]
        public ActionResult<string> CreatePaymentUrl(long paymentId, double money, string description)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                var request = new PaymentRequest
                {
                    PaymentId = paymentId,
                    Money = money,
                    Description = description,
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);
                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<(bool Success, string Message)> ProcessPaymentResult(PaymentResult paymentResult)
        {
            int orderId = (int)paymentResult.PaymentId;
            var order = await _context.Orders
                .Include(o => o.Event)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return (false, "Đơn hàng không tồn tại.");
            }

            // Kiểm tra xem đơn hàng đã được xử lý chưa
            if (order.PaymentStatus == "Completed" || order.PaymentStatus == "Failed")
            {
                return (order.PaymentStatus == "Completed", "Đơn hàng đã được xử lý trước đó.");
            }

            if (paymentResult.IsSuccess)
            {
                // Thanh toán thành công
                order.PaymentStatus = "Completed";
                order.UpdatedDate = DateTime.Now;
                _context.Update(order);

                // Cập nhật số vé còn lại của sự kiện
                var evt = order.Event;
                evt.TotalTickets -= order.Quantity;
                _context.Update(evt);

                // Tìm các vé có sẵn (Status = "Available") cho sự kiện
                var availableTickets = await _context.Tickets
                    .Where(t => t.EventId == order.EventId && t.Status == "Available" && (t.IsDeleted == false || t.IsDeleted == null))
                    .Take(order.Quantity)
                    .ToListAsync();

                if (availableTickets.Count < order.Quantity)
                {
                    // Không đủ vé để gán
                    order.PaymentStatus = "Failed";
                    order.UpdatedDate = DateTime.Now;
                    _context.Update(order);

                    var failedOrderHistory = new OrderHistory
                    {
                        OrderId = order.OrderId,
                        Status = "Failed",
                        DateTime = DateTime.Now,
                        Description = "Không đủ vé để gán sau khi thanh toán",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsDeleted = false
                    };
                    _context.OrderHistories.Add(failedOrderHistory);

                    await _context.SaveChangesAsync();
                    return (false, "Không đủ vé để gán");
                }

                // Gán vé cho người dùng
                foreach (var ticket in availableTickets)
                {
                    // Cập nhật trạng thái vé thành "Sold"
                    ticket.Status = "Sold";
                    ticket.UpdatedDate = DateTime.Now;
                    _context.Update(ticket);

                    // Gán vé cho người dùng
                    var userTicket = new UserTicket
                    {
                        UserId = order.UserId,
                        TicketId = ticket.TicketId,
                        OrderId = order.OrderId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsDeleted = false
                    };
                    _context.UserTickets.Add(userTicket);
                }

                // Lưu lịch sử giao dịch
                var paymentHistory = new PaymentHistory
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Amount = order.TotalAmount,
                    PaymentMethod = paymentResult.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Completed",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.PaymentHistories.Add(paymentHistory);

                // Lưu lịch sử đơn hàng
                var orderHistory = new OrderHistory
                {
                    OrderId = order.OrderId,
                    Status = "Completed",
                    DateTime = DateTime.Now,
                    Description = "Đơn hàng đã được thanh toán thành công qua VNPay",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.OrderHistories.Add(orderHistory);

                await _context.SaveChangesAsync();
                return (true, "Thanh toán thành công!");
            }
            else
            {
                // Thanh toán thất bại
                order.PaymentStatus = "Failed";
                order.UpdatedDate = DateTime.Now;
                _context.Update(order);

                // Lưu lịch sử giao dịch
                var paymentHistory = new PaymentHistory
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Amount = order.TotalAmount,
                    PaymentMethod = paymentResult.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Failed",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.PaymentHistories.Add(paymentHistory);

                // Lưu lịch sử đơn hàng
                var failedOrderHistory = new OrderHistory
                {
                    OrderId = order.OrderId,
                    Status = "Failed",
                    DateTime = DateTime.Now,
                    Description = "Thanh toán thất bại qua VNPay",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.OrderHistories.Add(failedOrderHistory);

                await _context.SaveChangesAsync();
                return (false, "Thanh toán thất bại");
            }
        }

        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var (success, message) = await ProcessPaymentResult(paymentResult);
                    if (success)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(message);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        [HttpGet("Callback")]
        public async Task<IActionResult> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var (success, message) = await ProcessPaymentResult(paymentResult);

                    if (success)
                    {
                        // Thanh toán thành công, chuyển hướng đến trang "Vé của tôi"
                        return RedirectToAction("PurchasedTickets", "Profile", new { successMessage = message });
                    }
                    else
                    {
                        // Thanh toán thất bại, chuyển hướng đến trang chính với thông báo lỗi
                        return RedirectToAction("Index", "Home", new { errorMessage = message });
                    }
                }
                catch (Exception ex)
                {
                    // Lỗi xử lý, chuyển hướng đến trang chính với thông báo lỗi
                    return RedirectToAction("Index", "Home", new { errorMessage = $"Lỗi: {ex.Message}" });
                }
            }

            // Không tìm thấy thông tin thanh toán
            return RedirectToAction("Index", "Home", new { errorMessage = "Không tìm thấy thông tin thanh toán." });
        }
    }
}