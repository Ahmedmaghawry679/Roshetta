using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Services
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDTO registerDTO);
        Task<Result<LoginResponse>> LoginAsync(LoginDTO loginDTO);
        Task<Result<LoginResponse>> RefreshAsync(string refreshToken);
        Task<Result> LogoutAsync(string refreshToken);
    }
}
