using Microsoft.Identity.Client;

namespace Roshetta.API.Models.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }

    }
}
