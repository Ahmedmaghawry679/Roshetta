using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Services
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDTO registerDTO);
    }
}
