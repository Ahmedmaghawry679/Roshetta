using Microsoft.AspNetCore.Identity;

namespace Roshetta.API.Models
{
    public class ApplicationUser: IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? Image { get; set; }
        public Doctor? Doctor { get; set; } // Navigation property to Doctor // One-to-One relationship with Doctor
        public virtual List<Booking> Bookings { get; set; } = new();
    }
}
