using Microsoft.EntityFrameworkCore;
using Roshetta.API.Data;
using Roshetta.API.Models;

namespace Roshetta.API.Repositories
{
    public class DoctorRepository(AppDbContext context) : IDoctorRepository
    {   
        public async Task AddAsync(Doctor doctor)
        {
            await context.Doctors.AddAsync(doctor);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var doctor = await context.Doctors.FindAsync(id);
            if (doctor is not null)
            {
                context.Doctors.Remove(doctor);
                await context.SaveChangesAsync();
            }
        }

        

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await context.Doctors.ToListAsync();
        }

        public async Task<Doctor> GetByIdAsync(Guid id)
        {
            return await context.Doctors.FindAsync(id);
        }

        public async Task<Doctor> GetByEmailAsync(string email)
        {
            return await context.Doctors.FirstOrDefaultAsync(d => d.User.Email == email);
        }

        public async Task<Doctor> GetByPhoneAsync(string phone)
        {
            return await context.Doctors.FirstOrDefaultAsync(d => d.User.PhoneNumber == phone);
        }

        public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(Guid specializationId)
        {
            return await context.Doctors.Where(d => d.SpecializationId == specializationId).ToListAsync();
        }

        public async Task UpdateAsync(Doctor doctor)
        {
            context.Doctors.Update(doctor);
            await context.SaveChangesAsync();
        }   
        
    }
}
