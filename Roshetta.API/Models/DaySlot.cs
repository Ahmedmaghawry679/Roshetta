using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace Roshetta.API.Models
{
    [Index(nameof(WorkingDayId), nameof(StartTime), IsUnique = true)]
    public class DaySlot
    {
        public Guid Id { get; set; }
        [ForeignKey("WorkingDayId")]
        public Guid WorkingDayId { get; set; } // Foreign key to WorkingDay
        public WorkingDay WorkingDay { get; set; } // Navigation property to WorkingDay
        public TimeOnly StartTime { get; set; }
        public List<Booking>? Bookings { get; set; } = new();
    }
}
