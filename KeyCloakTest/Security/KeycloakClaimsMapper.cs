using System.Security.Claims;
using System.Text.Json;

namespace KeyCloakTest.Security;

public static class KeycloakClaimsMapper
{
    public static void AddRoleClaims(ClaimsIdentity identity, ClaimsPrincipal principal, string? apiClientId)
    {
        var existingRoles = new HashSet<string>(
            identity.FindAll(DemoClaimTypes.Role).Select(claim => claim.Value),
            StringComparer.OrdinalIgnoreCase);

        foreach (var role in ReadRealmRoles(principal))
        {
            if (existingRoles.Add(role))
            {
                identity.AddClaim(new Claim(DemoClaimTypes.Role, role));
            }
        }

        foreach (var role in ReadClientRoles(principal, apiClientId))
        {
            if (existingRoles.Add(role))
            {
                identity.AddClaim(new Claim(DemoClaimTypes.Role, role));
            }
        }
    }

    private static IReadOnlyCollection<string> ReadRealmRoles(ClaimsPrincipal principal)
    {
        var roles = new List<string>();
        var realmAccessJson = principal.FindFirst(DemoClaimTypes.RealmAccess)?.Value;
        if (string.IsNullOrWhiteSpace(realmAccessJson))
        {
            return roles;
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccessJson);
            if (!document.RootElement.TryGetProperty("roles", out var rolesElement))
            {
                return roles;
            }

            foreach (var role in rolesElement.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    roles.Add(value);
                }
            }
        }
        catch (JsonException)
        {
        }

        return roles;
    }

    private static IReadOnlyCollection<string> ReadClientRoles(ClaimsPrincipal principal, string? apiClientId)
    {
        var roles = new List<string>();
        if (string.IsNullOrWhiteSpace(apiClientId))
        {
            return roles;
        }

        var resourceAccessJson = principal.FindFirst(DemoClaimTypes.ResourceAccess)?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccessJson))
        {
            return roles;
        }

        try
        {
            using var document = JsonDocument.Parse(resourceAccessJson);
            if (!document.RootElement.TryGetProperty(apiClientId, out var apiClientElement))
            {
                return roles;
            }

            if (!apiClientElement.TryGetProperty("roles", out var rolesElement))
            {
                return roles;
            }

            foreach (var role in rolesElement.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    roles.Add(value);
                }
            }
        }
        catch (JsonException)
        {
        }

        return roles;
    }
}
