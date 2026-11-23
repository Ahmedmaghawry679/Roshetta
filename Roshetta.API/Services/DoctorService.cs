using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roshetta.API.Data;
using Roshetta.API.Models;
using Roshetta.API.Models.DTOs;
using Roshetta.API.Repositories;
using System.Security.Cryptography;

namespace Roshetta.API.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ILogger<DoctorService> logger;
        private readonly IDoctorRepository doctorRepository;
        private readonly ISpecializationRepository specializationRepository;
        private readonly AppDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public DoctorService(ILogger<DoctorService> logger, 
            IDoctorRepository doctorRepository, 
            ISpecializationRepository specializationRepository,
            AppDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            this.logger = logger;
            this.doctorRepository = doctorRepository;
            this.specializationRepository = specializationRepository;
            this.context = context;
            this.userManager = userManager;
        }
        public async Task<Result<DoctorCreateResponse>> AddDoctorAsync(RegisterDoctorDTO dto, Guid adminId)
        {
            var email = dto.Email.Trim();

            var specializationExists = await specializationRepository.ExistsAsync(dto.SpecializationId);
            if(!specializationExists)
            {
                logger.LogWarning("Specialization with ID {SpecializationId} does not exist.", dto.SpecializationId);
                return Result<DoctorCreateResponse>.Failure(["Specialization does not exist." ], ErrorType.Validation);
            }

            var emailExists = await userManager.FindByEmailAsync(email);
            if(emailExists is not null)
            {
                logger.LogWarning("Doctor Registration rejected: User with this email already exists.");
                return Result<DoctorCreateResponse>.Failure(["User with this email already exists." ], ErrorType.Conflict);
            }

            var phoneExists = await userManager.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber);
            if (phoneExists)
            {
                logger.LogWarning("Doctor Registration rejected: User with this phone number already exists.");
                return Result<DoctorCreateResponse>.Failure(["User with this phone number already exists."], ErrorType.Conflict);
            }

            var tempPasswaord = GenerateTemporaryPassword();

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,
                UserName = email,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth
            };

            await using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var createResult = await userManager.CreateAsync(user, tempPasswaord);
                if (!createResult.Succeeded)
                {
                    var isDuplicate = createResult.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");

                    logger.LogWarning("Registration failed at user creation: {ErrorCodes}",
                        string.Join(", ", createResult.Errors.Select(e => e.Code)));

                    return Result<DoctorCreateResponse>.Failure(
                        createResult.Errors.Select(e => e.Description),
                        isDuplicate ? ErrorType.Conflict : ErrorType.Validation);
                }

                var roleResult = await userManager.AddToRoleAsync(user, Roles.Doctor);
                if(!roleResult.Succeeded)
                {
                    logger.LogError("Failed to add doctor to role.");
                    return Result<DoctorCreateResponse>.Failure(["Failed to create doctor."], ErrorType.Unexpected);
                }

                var doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecializationId = dto.SpecializationId,
                    Price = dto.Price
                };

                await doctorRepository.AddAsync(doctor);
                await tx.CommitAsync();

                logger.LogInformation("User {UserId} created successfully as {Role} by admin {AdminId}.", user.Id, Roles.Doctor, adminId);
                return Result<DoctorCreateResponse>.Success(new DoctorCreateResponse
                {
                    Id = doctor.Id,
                    TempPassword = tempPasswaord,
                    Email = user.Email
                });

            }
            catch(Exception ex)
            {
                await tx.RollbackAsync();
                logger.LogError(ex, "An error occurred while adding a new doctor.");
                return Result<DoctorCreateResponse>.Failure(["An unexpected error occurred."], ErrorType.Unexpected);
            }

        }
        public async Task<PagedList<DoctorDTO>> GetAllDoctorsAsync(DoctorQueryParameters parameters)
        {
            var pageNumber = Math.Max(parameters.PageNumber, 1);
            var pageSize = Math.Clamp(parameters.PageSize, 1, 50);

            parameters.PageNumber = pageNumber;
            parameters.PageSize = pageSize;

            return await doctorRepository.GetPagedAsync(parameters);

        }
        public async Task<Result<DoctorDTO?>> GetDoctorByIdAsync(Guid doctorId)
        {
            var doctor = await doctorRepository.GetByIdAsync(doctorId);
            if (doctor is null)
            {
                logger.LogWarning("Doctor with ID {DoctorId} not found.", doctorId);
                return Result<DoctorDTO?>.Failure(["Doctor not found."], ErrorType.NotFound);
            }

            return Result<DoctorDTO?>.Success(new DoctorDTO
            {
                Id = doctor.Id,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Email = doctor.User.Email??string.Empty,
                PhoneNumber = doctor.User.PhoneNumber??string.Empty,
                Gender = doctor.User.Gender.ToString(),
                DateOfBirth = doctor.User.DateOfBirth,
                Specialization = doctor.Specialization.Name,
                Price = doctor.Price
            };
        }

        private static string GenerateTemporaryPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%*?";
            var all = upper + lower + digits + symbols;

            var chars = new List<char> { Pick(upper), Pick(lower), Pick(digits), Pick(symbols) };
            while (chars.Count < 14) chars.Add(Pick(all));

            return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }
        private static char Pick(string s) => s[RandomNumberGenerator.GetInt32(s.Length)];

        public async Task<Result<string>> DeleteDoctorAsync(Guid doctorId)
        {
            var doctor = await doctorRepository.GetByIdAsync(doctorId);

            if (doctor is null)
            {
                logger.LogWarning("Doctor with ID {DoctorId} not found for deletion.", doctorId);
                return Result<string>.Failure(["Doctor not found."], ErrorType.NotFound);
            }

            if (await doctorRepository.HasBookingsAsync(doctorId))
            {
                logger.LogWarning("Doctor with ID {DoctorId} has existing bookings and cannot be deleted.", doctorId);
                return Result<string>.Failure(["Doctor has existing pending bookings."], ErrorType.Conflict);
            }

            await doctorRepository.DeleteAsync(doctorId);
            

            try
            {
                var result = await userManager.DeleteAsync(doctor.User);
                if (!result.Succeeded) 
                {
                    logger.LogWarning("Doctor with ID {DoctorId} cannot be deleted.", doctorId);
                    return Result<string>.Failure(["Doctor with ID {DoctorId}cannot be deleted."], ErrorType.Unexpected);
                }
                logger.LogInformation("Doctor with ID {DoctorId} deleted successfully.", doctorId);
                return Result<string>.Success("Doctor deleted successfully.");
            }
            catch (DbUpdateException)
            {
                // a booking was created between the check and the delete
                return Result<string>.Failure(["Doctor has existing bookings."], ErrorType.Conflict);
            }
        }
    }
}
