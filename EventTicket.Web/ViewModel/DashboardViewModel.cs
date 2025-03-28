using EventTicket.Data.Models;

namespace EventTicket.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTicketsSold { get; set; } // Tổng số vé đã bán
        public int TotalEvents { get; set; } // Tổng số sự kiện
        public int TotalUsers { get; set; } // Tổng số người dùng
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public List<Event> TopEvents { get; set; } // Các sự kiện nổi bật
        public List<string> AllMonths { get; set; } // Danh sách các tháng
        public List<decimal> MonthlyRevenues { get; set; } // Doanh thu theo tháng
        public decimal PercentGrowth { get; set; } // Tỷ lệ tăng trưởng

        public DashboardViewModel()
        {
            AllMonths = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            MonthlyRevenues = new List<decimal>(new decimal[12]);
            TopEvents = new List<Event>(); // Khởi tạo để tránh null
        }
    }
}