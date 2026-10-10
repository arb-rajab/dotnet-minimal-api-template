using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProjectName.Api.Auth;
using ProjectName.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ProjectName.UnitTests.Auth;

/// <summary>
/// JwtSecurityTokenHandler silently remaps well-known short claim types on
/// validation ("role" -> ClaimTypes.Role, "sub" -> ClaimTypes.NameIdentifier,
/// ...) unless JwtBearerOptions.MapInboundClaims is set to false. The app reads
/// the literal short names JwtTokenService issues, so with the default mapping
/// left on, RequireClaim("role", "Admin") would never match and every admin
/// request would get a false 403.
/// </summary>
public class InboundClaimMappingTests
{
    private static string IssueTestToken()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "claim-mapping-test-secret-at-least-32-bytes-long",
            Issuer = "projectname-tests",
            Audience = "projectname-tests-clients",
            ExpiryMinutes = 60,
        });
        var user = new User { Id = "u1", TenantId = "t1", Username = "admin@example.test", Role = UserRole.Admin };
        return new JwtTokenService(options).CreateToken(user);
    }

    private static TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "projectname-tests",
        ValidAudience = "projectname-tests-clients",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("claim-mapping-test-secret-at-least-32-bytes-long")),
    };

    [Fact]
    public void WithDefaultInboundMapping_RoleClaimTypeIsSilentlyRemapped()
    {
        var token = IssueTestToken();
        var handler = new JwtSecurityTokenHandler(); // default handler: mapping left on

        var principal = handler.ValidateToken(token, ValidationParameters(), out _);

        // The literal "role" claim AppClaimTypes.Role reads is gone: it has
        // been remapped to ClaimTypes.Role instead.
        Assert.Null(principal.FindFirst(AppClaimTypes.Role));
        Assert.NotNull(principal.FindFirst(ClaimTypes.Role));
    }

    [Fact]
    public void WithMapInboundClaimsDisabled_RoleClaimTypeSurvivesAsIssued()
    {
        var token = IssueTestToken();
        var handler = new JwtSecurityTokenHandler
        {
            // Mirrors options.MapInboundClaims = false in Program.cs.
            InboundClaimTypeMap = new Dictionary<string, string>(),
        };

        var principal = handler.ValidateToken(token, ValidationParameters(), out _);

        var roleClaim = principal.FindFirst(AppClaimTypes.Role);
        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim!.Value);
    }
}
