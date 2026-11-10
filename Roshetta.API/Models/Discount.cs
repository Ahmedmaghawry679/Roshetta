using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Roshetta.API.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class Discount
    {
        public Guid Id { get; set; }
        [Required, MaxLength(30)]
        public string Code { get; set; }
        public int NumOfRequests { get; set; }
        public DiscountType DiscountType { get; set; }
        [Precision(18, 2)]
        public decimal DiscountValue { get; set; }
        public bool IsActive { get; set; }
    }
}
