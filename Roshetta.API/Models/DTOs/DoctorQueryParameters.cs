namespace Roshetta.API.Models.DTOs
{
    public class DoctorQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = false;
    }
}
