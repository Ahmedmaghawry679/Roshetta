using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Roshetta.API.Models
{
    [Index(nameof(Name), IsUnique =true)]
    public class Specialization
    {
        public Guid Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public virtual List<Doctor> Doctors { get; set; } = new();
    }
}
