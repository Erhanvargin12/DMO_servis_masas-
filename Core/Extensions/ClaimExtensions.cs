using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Extensions;

public static class ClaimExtensions
{
    public static void AddEmail(this ICollection<Claim> claims, string email)
    {
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
    }

    public static void AddName(this ICollection<Claim> claims, string name)
    {
        claims.Add(new Claim(ClaimTypes.Name, name));
    }

    public static void AddNameIdentifier(this ICollection<Claim> claims, string nameIdentifier)
    {
        claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));
    }

    public static void AddNameUniqueIdentifier(this ICollection<Claim> claims, string nameUniqueIdentifier)
    {
        claims.Add(new Claim(ClaimTypes.SerialNumber, nameUniqueIdentifier));
    }

    public static void AddRoles(this ICollection<Claim> claims, string[] roles)
    {
        roles.ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));
    }

    public static void AddBranchId(this ICollection<Claim> claims, string? branchId)
    {
        if (!string.IsNullOrEmpty(branchId))
            claims.Add(new Claim("branchId", branchId));
    }
    public static void AddEbysUserId(this ICollection<Claim> claims, string? registrationId)
    {
        if (!string.IsNullOrEmpty(registrationId))
            claims.Add(new Claim("ebysUserId", registrationId));
    }
    public static void AddProxyUserIds(this ICollection<Claim> claims, List<string?> proxyUserIds)
    {
        proxyUserIds.ForEach(id => claims.Add(new Claim("proxyUserId", id ?? string.Empty)));
    }
}