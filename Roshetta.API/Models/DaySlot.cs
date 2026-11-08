using System.ComponentModel.DataAnnotations.Schema;

namespace Roshetta.API.Models
{
    public class DaySlot
    {
        public Guid Id { get; set; }
        [ForeignKey("WorkingDayId")]
        public Guid WorkingDayId { get; set; } // Foreign key to WorkingDay
        public WorkingDay WorkingDay { get; set; } // Navigation property to WorkingDay
        public TimeOnly StartTime { get; set; }
        public Booking? Booking { get; set; }
    }
}
