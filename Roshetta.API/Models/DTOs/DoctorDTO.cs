namespace Roshetta.API.Models.DTOs
{
    public class DoctorDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
