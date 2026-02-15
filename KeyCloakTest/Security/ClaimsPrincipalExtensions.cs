using System.Security.Claims;

namespace KeyCloakTest.Security;

public static class ClaimsPrincipalExtensions
{
    public static bool HasScope(this ClaimsPrincipal principal, string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        var scopeClaims = principal.FindAll(DemoClaimTypes.Scope).SelectMany(claim =>
            claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        var scpClaims = principal.FindAll(DemoClaimTypes.Scp).Select(claim => claim.Value);

        return scopeClaims.Concat(scpClaims).Any(value =>
            string.Equals(value, scope, StringComparison.OrdinalIgnoreCase));
    }
}
