namespace EventTicket.Web.ViewModels
{
    public class TicketViewModel
    {
        public int TicketId { get; set; }

        public int EventId { get; set; }

        public decimal Price { get; set; }

        public string? SeatNumber { get; set; }

        public string Status { get; set; } = "Available";

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool? IsDeleted { get; set; }
    }
}