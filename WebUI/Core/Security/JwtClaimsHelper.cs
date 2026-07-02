using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebUI.Models.ViewModels;

namespace WebUI.Core.Security;

public static class JwtClaimsHelper
{
    private const string MsClaimsBase = "http://schemas.microsoft.com/ws/2008/06/identity/claims/";
    private const string SoapNameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    public static string? GetAccessToken(HttpContext context)
    {
        return context.Session.GetString("AccessToken")
               ?? context.User.FindFirst("AccessToken")?.Value;
    }

    public static JwtSecurityToken? ReadToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }

    public static string? GetClaimValue(JwtSecurityToken token, params string[] types)
    {
        foreach (var type in types)
        {
            var claim = token.Claims.FirstOrDefault(c => c.Type == type);
            if (claim != null)
                return claim.Value;
        }

        return null;
    }

    public static List<string> GetRoles(JwtSecurityToken token)
    {
        return token.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == MsClaimsBase + "role" || c.Type == "role")
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static UserContext MapToUserContext(HttpContext context)
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
            return new UserContext();

        var jwt = ReadToken(GetAccessToken(context));
        if (jwt == null)
            return new UserContext { IsAuthenticated = true };

        var idValue = GetClaimValue(jwt,
            ClaimTypes.NameIdentifier,
            SoapNameIdentifier,
            MsClaimsBase + "nameidentifier",
            "sub",
            "nameid");

        return new UserContext
        {
            IsAuthenticated = true,
            UserId = int.TryParse(idValue, out var id) ? id : 0,
            EbysUserId = GetClaimValue(jwt, "ebysUserId"),
            Email = GetClaimValue(jwt, ClaimTypes.Email, JwtRegisteredClaimNames.Email, "email"),
            UserName = GetClaimValue(jwt, ClaimTypes.Name, MsClaimsBase + "name", "name", JwtRegisteredClaimNames.Name),
            Roles = GetRoles(jwt),
            ProxyUserIds = jwt.Claims.Where(c => c.Type == "proxyUserId").Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    public static ClaimsPrincipal HydratePrincipal(ClaimsPrincipal existing, string token)
    {
        var jwt = ReadToken(token);
        if (jwt == null)
            return existing;

        var claims = new List<Claim> { new("AccessToken", token) };

        foreach (var claim in jwt.Claims)
            claims.Add(claim);

        var email = GetClaimValue(jwt, ClaimTypes.Email, JwtRegisteredClaimNames.Email, "email");
        if (!string.IsNullOrEmpty(email) && !claims.Any(c => c.Type == ClaimTypes.Email))
            claims.Add(new Claim(ClaimTypes.Email, email));

        var authType = existing.Identity?.AuthenticationType ?? CookieAuthenticationDefaults.AuthenticationScheme;
        var identity = new ClaimsIdentity(claims, authType, ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }
}
