namespace PRNFE.MVC.Helper;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JwtTokenHelper
{
    public static string? GetClaim(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt?.Claims?.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    public static IDictionary<string, string> GetAllClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt?.Claims.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>();
    }

    // Check role is "Quản lý trọ"
    public static bool IsLandlord(string token)
    {
        var role = GetClaim(token, "role");
        return role == "Quản lý trọ";
    }
    // Check role is "Thuê trọ"
    public static bool IsTenant(string token)
    {
        var role = GetClaim(token, "role");
        return role == "Thuê trọ";
    }
}
