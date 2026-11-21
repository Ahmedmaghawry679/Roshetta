using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Roshetta.API.Models;
using Roshetta.API.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Roshetta.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AuthService> logger;
        private readonly IConfiguration configuration;

        public AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.configuration = configuration;
        }

        // ---------------------------------------------------------------
        // Register
        // ---------------------------------------------------------------
        public async Task<Result> RegisterAsync(RegisterDTO dto)
        {
            var email = dto.Email.Trim();

            if (await userManager.FindByEmailAsync(email) is not null)
            {
                logger.LogWarning("Registration rejected: email already registered");
                return Result.Failure(["Email is already registered."], ErrorType.Conflict);
            }

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
                Gender = dto.Gender
            };

            var createResult = await userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var isDuplicate = createResult.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");

                logger.LogWarning("Registration failed at user creation: {ErrorCodes}",
                    string.Join(", ", createResult.Errors.Select(e => e.Code)));

                return Result.Failure(
                    createResult.Errors.Select(e => e.Description),
                    isDuplicate ? ErrorType.Conflict : ErrorType.Validation);
            }

            // AddToRoleAsync THROWS if the role doesn't exist, so keep the try/catch
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
                logger.LogError(ex, "Role {Role} does not exist (has the seeder run?). Rolling back user {UserId}.",
                    Roles.Patient, user.Id);

                await RollbackAsync(user);
                return Result.Failure(["Could not complete registration. Please try again."], ErrorType.Unexpected);
            }

            logger.LogInformation("User {UserId} registered successfully as {Role}", user.Id, Roles.Patient);
            return Result.Success();
        }

        // ---------------------------------------------------------------
        // Login
        // ---------------------------------------------------------------
        public async Task<Result<LoginResponse>> LoginAsync(LoginDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email.Trim());
            if (user is null)
                return InvalidCredentials();

            if (await userManager.IsLockedOutAsync(user))
            {
                logger.LogWarning("Login blocked: user {UserId} is locked out", user.Id);
                return InvalidCredentials();
            }

            if (!await userManager.CheckPasswordAsync(user, dto.Password))
            {
                await userManager.AccessFailedAsync(user);
                logger.LogWarning("Login failed: invalid credentials");
                return InvalidCredentials();
            }

            await userManager.ResetAccessFailedCountAsync(user);

            var response = await IssueTokensAsync(user);
            if (response is null)
                return Result<LoginResponse>.Failure(["Could not complete login. Please try again."], ErrorType.Unexpected);

            logger.LogInformation("User {UserId} logged in successfully", user.Id);
            return Result<LoginResponse>.Success(response);
        }

        private Result<LoginResponse> InvalidCredentials()
            => Result<LoginResponse>.Failure(["Invalid email or password."], ErrorType.Unauthorized);
        

        // ---------------------------------------------------------------
        // Refresh
        // ---------------------------------------------------------------
        public async Task<Result<LoginResponse>> RefreshAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);

            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hash);
            if (user is null)
            {
                logger.LogWarning("Refresh failed: unknown or already-used token");
                return InvalidRefreshToken();
            }

            if (user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                logger.LogWarning("Refresh failed: token expired for user {UserId}", user.Id);
                return InvalidRefreshToken();
            }

            // Rotation: this overwrites the old hash, so the old token stops working
            var response = await IssueTokensAsync(user);
            if (response is null)
                return InvalidRefreshToken();   // e.g. a concurrent refresh won the race

            logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);
            return Result<LoginResponse>.Success(response);
        }

        // ---------------------------------------------------------------
        // Logout
        // ---------------------------------------------------------------
        public async Task<Result> LogoutAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hash);

            if (user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await userManager.UpdateAsync(user);
                logger.LogInformation("User {UserId} logged out", user.Id);
            }

            // Always succeed, so the endpoint doesn't reveal which tokens exist
            return Result.Success();
        }

        // ---------------------------------------------------------------
        // Token helpers
        // ---------------------------------------------------------------

        // Used by login and refresh: builds both tokens and stores the refresh HASH
        private async Task<LoginResponse?> IssueTokensAsync(ApplicationUser user)
        {
            var accessToken = await GenerateJwtAccessTokenAsync(user);

            var rawRefreshToken = GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenDays", 7));

            user.RefreshToken = HashToken(rawRefreshToken);   // only the hash is stored
            user.RefreshTokenExpiryTime = refreshExpiry;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                logger.LogError("Failed to save refresh token for user {UserId}: {ErrorCodes}",
                    user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Code)));
                return null;
            }

            return new LoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                AccessTokenExpiresAt = accessToken.ValidTo,
                RefreshToken = rawRefreshToken,          // raw goes to the client
                RefreshTokenExpiresAt = refreshExpiry    // same value that was saved
            };
        }

        private async Task<JwtSecurityToken> GenerateJwtAccessTokenAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Email, user.Email!)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:AccessTokenMinutes", 15)),
                signingCredentials: creds);
        }

        // Only generates the random string. No database work here.
        private static string GenerateRefreshToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        // Deterministic: same input always gives the same hash (that's how lookup works)
        private static string HashToken(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private static Result<LoginResponse> InvalidRefreshToken()
            => Result<LoginResponse>.Failure(["Invalid refresh token."], ErrorType.Unauthorized);

        private async Task RollbackAsync(ApplicationUser user)
        {
            var deleteResult = await userManager.DeleteAsync(user);

            if (deleteResult.Succeeded)
            {
                logger.LogInformation("Rollback succeeded: user {UserId} deleted", user.Id);
                return;
            }

            logger.LogCritical(
                "Rollback FAILED: user {UserId} exists without a role. Manual cleanup required. {ErrorCodes}",
                user.Id,
                string.Join(", ", deleteResult.Errors.Select(e => e.Code)));
        }
    }
}