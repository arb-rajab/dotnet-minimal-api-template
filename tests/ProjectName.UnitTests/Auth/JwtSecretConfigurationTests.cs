using System.Text;
using ProjectName.Api.Auth;
using Microsoft.IdentityModel.Tokens;

namespace ProjectName.UnitTests.Auth;

/// <summary>
/// Pins the mechanism behind a subtle startup-ordering bug: if Program.cs read
/// Jwt:Secret into a local variable before builder.Build() and closed over that
/// value in the AddJwtBearer delegate, WebApplicationFactory-based tests (which
/// merge configuration overrides at Build() time) would validate signatures
/// against the pre-override secret while JwtTokenService signed with the
/// overridden one, so every authenticated request would fail with 401.
///
/// A token signed with one secret must fail validation against a different key
/// and succeed against the key that signed it. The full wiring is covered by
/// the integration suite.
/// </summary>
public class JwtSecretConfigurationTests
{
    [Fact]
    public void TokenValidation_WithSameSecretUsedToSign_Succeeds()
    {
        var secret = "matching-secret-used-for-both-signing-and-validation";
        var token = CreateTestToken(secret);

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parameters = ValidationParametersFor(secret);

        var exception = Record.Exception(() => handler.ValidateToken(token, parameters, out _));

        Assert.Null(exception);
    }

    [Fact]
    public void TokenValidation_WithDifferentSecretThanSigning_FailsWithSecurityTokenException()
    {
        // Sign with the post-override secret, validate with a stale one.
        var signingSecret = "the-secret-jwttokenservice-actually-signed-with";
        var staleValidationSecret = "a-different-stale-secret-captured-too-early";
        var token = CreateTestToken(signingSecret);

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parameters = ValidationParametersFor(staleValidationSecret);

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(
            () => handler.ValidateToken(token, parameters, out _));
    }

    private static string CreateTestToken(string secret)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Secret = secret,
            Issuer = "projectname-tests",
            Audience = "projectname-tests-clients",
            ExpiryMinutes = 60,
        });
        var service = new JwtTokenService(options);
        var user = new ProjectName.Api.Domain.User
        {
            Id = "u1",
            TenantId = "t1",
            Username = "test@example.test",
            Role = ProjectName.Api.Domain.UserRole.Admin,
        };
        return service.CreateToken(user);
    }

    private static TokenValidationParameters ValidationParametersFor(string secret) => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "projectname-tests",
        ValidAudience = "projectname-tests-clients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.FromSeconds(30),
    };
}
