using System.Security.Claims;
using System.Text.Json;

namespace BSUIR_Savchenko.Domain.Security;

public static class KeycloakRoleClaims
{
    public static void AddRoleClaims(ClaimsPrincipal? principal)
    {
        if (principal?.Identity is not ClaimsIdentity identity) return;

        var roles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var claim in principal.FindAll("roles")) ReadRoles(claim.Value, roles);
        foreach (var claim in principal.FindAll("realm_access")) ReadRoles(claim.Value, roles);

        foreach (var role in roles)
        {
            if (!principal.IsInRole(role)) identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }

    private static void ReadRoles(string raw, HashSet<string> roles)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        if (raw[0] != '[' && raw[0] != '{')
        {
            roles.Add(raw);
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            var value = document.RootElement;
            if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("roles", out var roleArray))
                value = roleArray;
            if (value.ValueKind != JsonValueKind.Array) return;
            foreach (var item in value.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } role)
                    roles.Add(role);
        }
        catch (JsonException)
        {
        }
    }
}
