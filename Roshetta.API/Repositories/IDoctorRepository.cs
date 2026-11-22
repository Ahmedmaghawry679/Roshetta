using Roshetta.API.Models;

namespace Roshetta.API.Repositories
{
    public interface IDoctorRepository
    {
        Task AddAsync(Doctor doctor); 
        Task UpdateAsync(Doctor doctor);
        Task DeleteAsync(Guid id);
        Task<Doctor> GetByIdAsync(Guid id);
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<IEnumerable<Doctor>> GetBySpecializationAsync(Guid specializationId);
        Task<Doctor> GetByEmailAsync(string email);
        Task<Doctor> GetByPhoneAsync(string phone);

    }
}
