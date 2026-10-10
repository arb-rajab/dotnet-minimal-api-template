using System.IdentityModel.Tokens.Jwt;
using ProjectName.Api.Auth;
using ProjectName.Api.Domain;
using Microsoft.Extensions.Options;

namespace ProjectName.UnitTests.Auth;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int expiryMinutes = 60) =>
        new(Options.Create(new JwtOptions
        {
            Secret = "unit-test-signing-secret-at-least-32-bytes-long",
            Issuer = "projectname-tests",
            Audience = "projectname-tests-clients",
            ExpiryMinutes = expiryMinutes,
        }));

    [Fact]
    public void CreateToken_EmbedsTenantIdAndRoleClaims()
    {
        var service = CreateService();
        var user = new User
        {
            Id = "user-1",
            TenantId = "tenant-abc",
            Username = "admin@example.test",
            Role = UserRole.Admin,
        };

        var token = service.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("tenant-abc", jwt.Claims.Single(c => c.Type == AppClaimTypes.TenantId).Value);
        Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == AppClaimTypes.Role).Value);
        Assert.Equal("user-1", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("projectname-tests", jwt.Issuer);
    }

    [Fact]
    public void CreateToken_WithoutTenant_OmitsTenantClaim()
    {
        var service = CreateService();
        var user = new User { Id = "user-2", Username = "viewer@example.test", Role = UserRole.Viewer };

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(service.CreateToken(user));

        Assert.DoesNotContain(jwt.Claims, c => c.Type == AppClaimTypes.TenantId);
        Assert.Equal("Viewer", jwt.Claims.Single(c => c.Type == AppClaimTypes.Role).Value);
    }

    [Fact]
    public void CreateToken_SetsExpiryFromOptions()
    {
        var service = CreateService(expiryMinutes: 15);
        var user = new User { Id = "u", TenantId = "t", Username = "x@y.test", Role = UserRole.Viewer };

        var token = service.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        Assert.True(Math.Abs((jwt.ValidTo - expectedExpiry).TotalMinutes) < 1);
    }
}
