using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Roshetta.API.Models
{
    [Index(nameof(DoctorId), nameof(Day), IsUnique = true)]
    public class WorkingDay
    {
        public Guid Id { get; set; }
        [ForeignKey("DoctorId")]
        public Guid DoctorId { get; set; } // Foreign key to Doctor
        public Doctor Doctor { get; set; } // Navigation property to Doctor
        public Days Day { get; set; }
        public virtual List<DaySlot> DaySlots { get; set; } = new(); // Navigation property to DaySlot
    }
}
