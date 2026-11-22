namespace Roshetta.API.Models.DTOs
{
    public class DoctorCreateResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TempPassword { get; set; } = string.Empty;

    }
}
