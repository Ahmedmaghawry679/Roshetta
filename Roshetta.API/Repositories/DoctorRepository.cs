using Microsoft.EntityFrameworkCore;
using Roshetta.API.Data;
using Roshetta.API.Models;
using Roshetta.API.Models.DTOs;

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

        public async Task<Doctor?> GetByIdAsync(Guid id)
        {
            return await context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == id);
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

        public async Task<PagedList<DoctorDTO>> GetPagedAsync(DoctorQueryParameters parameters)
        {
            var query = context.Doctors
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(d => d.User.FirstName.Contains(parameters.SearchTerm) ||
                d.User.LastName.Contains(parameters.SearchTerm) ||
                d.User.Email.Contains(parameters.SearchTerm) ||
                d.Specialization.Name.Contains(parameters.SearchTerm));
            }

            query = parameters.SortBy?.ToLower() switch
            {
                "firstname" => parameters.Descending
                ? query.OrderByDescending(d => d.User.FirstName)
                : query.OrderBy(d => d.User.FirstName),

                "lastname" => parameters.Descending
                ? query.OrderByDescending(d => d.User.LastName)
                : query.OrderBy(d => d.User.LastName),

                "email" => parameters.Descending
                ? query.OrderByDescending(d => d.User.Email)
                : query.OrderBy(d => d.User.Email),

                "specialization" => parameters.Descending
                ? query.OrderByDescending(d => d.Specialization.Name)
                : query.OrderBy(d => d.Specialization.Name),

                "price" => parameters.Descending
                ? query.OrderByDescending(d => d.Price)
                : query.OrderBy(d => d.Price),

                _ => query.OrderBy(d => d.Id)
            };

            var doctors = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).Select(d => new DoctorDTO
            {
                Id = d.Id,
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                Email = d.User.Email,
                PhoneNumber = d.User.PhoneNumber,
                Gender = d.User.Gender.ToString(),
                DateOfBirth = d.User.DateOfBirth,
                Specialization = d.Specialization.Name,
                Price = d.Price
            }).ToListAsync();


            return new PagedList<DoctorDTO>(doctors, await query.CountAsync(), parameters.PageNumber, parameters.PageSize);
        }
        public Task<bool> HasBookingsAsync(Guid doctorId)
            => context.Bookings.AnyAsync(b => b.DoctorId == doctorId);
    }
}
