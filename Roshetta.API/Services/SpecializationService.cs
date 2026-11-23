using Roshetta.API.Models.DTOs;
using Roshetta.API.Repositories;

namespace Roshetta.API.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly ILogger logger;
        private readonly ISpecializationRepository specializationRepository;

        public SpecializationService(ILogger<SpecializationService> logger, 
            ISpecializationRepository specializationRepository)
        {
            this.logger = logger;
            this.specializationRepository = specializationRepository;
        }
        public async Task<IEnumerable<SpecializationDTO>> GetAllSpecializationsAsync()
        {
            var specializations = (await specializationRepository.GetAllAsync()).Select(s=> new SpecializationDTO
            {
                Id = s.Id,
                Name = s.Name   
            });
            return specializations;
        }
    }
}
