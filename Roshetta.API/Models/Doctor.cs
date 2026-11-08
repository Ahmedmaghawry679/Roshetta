using System.ComponentModel.DataAnnotations.Schema;

namespace Roshetta.API.Models
{
    public class Doctor
    {
        public Guid Id { get; set; }
        [ForeignKey("UserId")]
        public Guid UserId { get; set; } // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } // Navigation property to ApplicationUser

        [ForeignKey("SpecializationId")]
        public Guid SpecializationId { get; set; } // Foreign key to Specialization
        public Specialization Specialization { get; set; } // Navigation property to Specialization
        public decimal Price { get; set; }
        public virtual List<WorkingDay> WorkingDays { get; set; } = new(); // Navigation property to WorkingDay
        public virtual List<Booking> Bookings { get; set; } = new();
    }
}
