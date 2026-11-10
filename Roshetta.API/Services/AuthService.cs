using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roshetta.API.Models;
using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AuthService> logger;

        public AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<Result> RegisterAsync(RegisterDTO dto)
        {
            var email = dto.Email.Trim();

            // 1. Duplicate email (friendly early check)
            if (await userManager.FindByEmailAsync(email) is not null)
            {
                logger.LogWarning("Registration rejected: email already registered");
                return Result.Failure(["Email is already registered."], ErrorType.Conflict);
            }

            // 2. Duplicate phone number (Identity doesn't enforce this)
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
            {
                logger.LogWarning("Registration rejected: phone number already registered");
                return Result.Failure(["Phone number is already registered."], ErrorType.Conflict);
            }

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                UserName = email,
                Email = email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Image = dto.Image
            };

            // 3. Create the user
            var createResult = await userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                // Covers the race where two requests pass step 1 at the same time
                var isDuplicate = createResult.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");

                logger.LogWarning("Registration failed at user creation: {ErrorCodes}",
                    string.Join(", ", createResult.Errors.Select(e => e.Code)));

                return Result.Failure(
                    createResult.Errors.Select(e => e.Description),
                    isDuplicate ? ErrorType.Conflict : ErrorType.Validation);
            }

            // 4. Assign the Patient role; never leave a user without a role
            try
            {
                var roleResult = await userManager.AddToRoleAsync(user, Roles.Patient);
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Failed to assign {Role} role to user {UserId}: {ErrorCodes}. Rolling back.",
                        Roles.Patient, user.Id,
                        string.Join(", ", roleResult.Errors.Select(e => e.Code)));

                    await RollbackAsync(user);
                    return Result.Failure(["Could not complete registration. Please try again."], ErrorType.Unexpected);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex,
                    "Role {Role} does not exist (has the seeder run?). Rolling back user {UserId}.",
                    Roles.Patient, user.Id);

                await RollbackAsync(user);
                return Result.Failure(["Could not complete registration. Please try again."], ErrorType.Unexpected);
            }

            logger.LogInformation("User {UserId} registered successfully as {Role}", user.Id, Roles.Patient);
            return Result.Success();
        }

        private async Task RollbackAsync(ApplicationUser user)
        {
            var deleteResult = await userManager.DeleteAsync(user);

            if (deleteResult.Succeeded)
            {
                logger.LogInformation("Rollback succeeded: user {UserId} deleted", user.Id);
                return;
            }

            // User exists with no role. Needs manual cleanup.
            logger.LogCritical(
                "Rollback FAILED: user {UserId} exists without a role. Manual cleanup required. {ErrorCodes}",
                user.Id,
                string.Join(", ", deleteResult.Errors.Select(e => e.Code)));
        }
    }
}