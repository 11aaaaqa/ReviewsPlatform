using System.Security.Claims;
using AccountMicroservice.Api.Models.Business;

namespace AccountMicroservice.Api.Services.TokenServices
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromToken(string token);
        List<Claim> GetClaims(User user);
    }
}
