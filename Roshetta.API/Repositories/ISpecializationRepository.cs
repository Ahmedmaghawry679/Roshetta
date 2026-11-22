using Roshetta.API.Models;

namespace Roshetta.API.Repositories
{
    public interface ISpecializationRepository
    {
        Task AddAsync(Specialization specialization);
        Task UpdateAsync(Specialization specialization);
        Task DeleteAsync(Guid id);
        Task<Specialization> GetByIdAsync(Guid id);
        Task<IEnumerable<Specialization>> GetAllAsync();
        Task<bool> ExistsAsync(Guid id);

    }
}
