using Roshetta.API.Models;
using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Repositories
{
    public interface IDoctorRepository
    {
        Task AddAsync(Doctor doctor); 
        Task UpdateAsync(Doctor doctor);
        Task DeleteAsync(Guid id);
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<PagedList<DoctorDTO>> GetPagedAsync(DoctorQueryParameters parameters);
        Task<bool> HasBookingsAsync(Guid doctorId);

    }
}
