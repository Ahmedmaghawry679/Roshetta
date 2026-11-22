using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Services
{
    public interface IDoctorService
    {
        Task<Result<DoctorCreateResponse>> AddDoctorAsync(DoctorDTO dto, Guid adminId);
    }
}
