using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Services
{
    public interface ISpecializationService
    {
        Task<IEnumerable<SpecializationDTO>> GetAllSpecializationsAsync();
    }
}
