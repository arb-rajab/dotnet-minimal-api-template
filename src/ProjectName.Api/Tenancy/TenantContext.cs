using System.Security.Claims;
using ProjectName.Api.Auth;

namespace ProjectName.Api.Tenancy;

/// <summary>
/// Per-request tenant identity, resolved exclusively from the validated JWT,
/// never from a client-supplied header or query parameter (a header can be
/// forged; the JWT is signed). Tenant-scoped repositories should take this as
/// a dependency so there is no code path that queries a tenant-owned
/// collection without a tenant filter.
/// </summary>
public interface ITenantContext
{
    string TenantId { get; }

    string UserId { get; }

    string Role { get; }
}

public sealed class HttpTenantContext : ITenantContext
{
    private readonly ClaimsPrincipal? _user;

    public HttpTenantContext(IHttpContextAccessor accessor)
    {
        _user = accessor.HttpContext?.User;
    }

    public string TenantId => GetClaim(AppClaimTypes.TenantId);

    public string UserId => GetClaim("sub");

    public string Role => GetClaim(AppClaimTypes.Role);

    private string GetClaim(string type)
    {
        var value = _user?.FindFirst(type)?.Value;
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"Tenant context requested claim '{type}' outside an authenticated request. " +
                "This indicates an endpoint is missing [Authorize] / RequireAuthorization().");
        }

        return value;
    }
}
