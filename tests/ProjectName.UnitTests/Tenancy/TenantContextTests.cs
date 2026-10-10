using System.Security.Claims;
using ProjectName.Api.Tenancy;
using Microsoft.AspNetCore.Http;

namespace ProjectName.UnitTests.Auth;

public class TenantContextTests
{
    [Fact]
    public void TenantId_WhenClaimPresent_ReturnsValue()
    {
        var context = BuildContext(("tenant_id", "tenant-abc"), ("role", "Admin"));

        Assert.Equal("tenant-abc", context.TenantId);
        Assert.Equal("Admin", context.Role);
    }

    [Fact]
    public void TenantId_WhenUnauthenticated_ThrowsInvalidOperationException()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var context = new HttpTenantContext(accessor);

        Assert.Throws<InvalidOperationException>(() => context.TenantId);
    }

    private static HttpTenantContext BuildContext(params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        return new HttpTenantContext(accessor);
    }
}
