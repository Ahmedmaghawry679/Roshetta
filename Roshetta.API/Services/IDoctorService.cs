using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Services
{
    public interface IDoctorService
    {
        Task<Result<CreateDoctorResponse>> AddDoctorAsync(RegisterDoctorDTO dto, Guid adminId);
        Task<PagedList<DoctorDTO>> GetAllDoctorsAsync(DoctorQueryParameters parameters);
        Task<Result<DoctorDTO?>> GetDoctorByIdAsync(Guid doctorId);
        Task<Result<string>> DeleteDoctorAsync(Guid doctorId);
        Task<Result<DoctorDTO?>> UpdateDoctorAsync(Guid doctorId, UpdateDoctorDTO dto);
    }
}
