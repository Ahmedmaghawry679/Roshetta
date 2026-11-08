using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roshetta.API.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        [ForeignKey("DoctorId")]
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        [ForeignKey("PatientId")]
        public Guid PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        [ForeignKey("DaySlotId")]
        public Guid DaySlotId { get; set; }
        public DaySlot DaySlot { get; set; }
        public BookingStatus Status { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        [ForeignKey("DiscountId")]
        public Guid? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        [Precision(18, 2)]
        public decimal FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
