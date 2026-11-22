using Microsoft.EntityFrameworkCore;
using Roshetta.API.Data;
using Roshetta.API.Models;

namespace Roshetta.API.Repositories
{
    public class SpecializationRepository(AppDbContext context) : ISpecializationRepository
    {   
        public async Task AddAsync(Specialization specialization)
        {
            await context.Specializations.AddAsync(specialization);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var specialization = await context.Specializations.FindAsync(id);
            if (specialization is not null)
            {
                context.Specializations.Remove(specialization);
                await context.SaveChangesAsync();
            }
        }

        

        public async Task<IEnumerable<Specialization>> GetAllAsync()
        {
            return await context.Specializations.ToListAsync();
        }

        public async Task<Specialization> GetByIdAsync(Guid id)
        {
            return await context.Specializations.FindAsync(id);
        }

        public async Task UpdateAsync(Specialization specialization)
        {
            context.Specializations.Update(specialization);
            await context.SaveChangesAsync();
        }  
        
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await context.Specializations.AnyAsync(s => s.Id == id);
        }

    }
}
