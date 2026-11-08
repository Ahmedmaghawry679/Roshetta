namespace Roshetta.API.Models
{
    public class Discount
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public int NumOfRequests { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsActive { get; set; }
        public virtual List<Booking> Bookings { get; set; } = new();
    }
}
