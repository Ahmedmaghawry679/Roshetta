using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Roshetta.API.Controllers.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(id, out var parsedId) ? parsedId : Guid.Empty;
        }
    }
}
