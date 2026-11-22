namespace Roshetta.API.Models.DTOs
{
    public class DoctorDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Guid SpecializationId { get; set; }
        public decimal Price { get; set; }
    }
}
