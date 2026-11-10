using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Roshetta.API.Models
{
    public class ApplicationUser: IdentityUser<Guid>
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        [MaxLength(300)]
        public string? Image { get; set; }
        public Doctor? Doctor { get; set; } // Navigation property to Doctor // One-to-One relationship with Doctor
    }
}
