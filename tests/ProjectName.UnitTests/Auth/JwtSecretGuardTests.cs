using ProjectName.Api.Auth;

namespace ProjectName.UnitTests.Auth;

/// <summary>
/// Pins the startup guard against the dev placeholder JWT secret reaching
/// Production. Exercises the static check directly instead of booting a host.
/// </summary>
public class JwtSecretGuardTests
{
    [Fact]
    public void EnsureNotPlaceholder_PlaceholderInProduction_Throws()
    {
        var ex = Record.Exception(() =>
            JwtSecretGuard.EnsureNotPlaceholder(JwtSecretGuard.KnownDevPlaceholder, isProduction: true));

        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void EnsureNotPlaceholder_PlaceholderInDevelopment_DoesNotThrow()
    {
        var ex = Record.Exception(() =>
            JwtSecretGuard.EnsureNotPlaceholder(JwtSecretGuard.KnownDevPlaceholder, isProduction: false));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureNotPlaceholder_RealSecretInProduction_DoesNotThrow()
    {
        var ex = Record.Exception(() =>
            JwtSecretGuard.EnsureNotPlaceholder("a-real-operator-provisioned-secret-value", isProduction: true));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureNotPlaceholder_NullSecretInProduction_DoesNotThrow()
    {
        // Not this guard's job -- the separate IsNullOrEmpty check in
        // Program.cs handles a missing secret.
        var ex = Record.Exception(() =>
            JwtSecretGuard.EnsureNotPlaceholder(null, isProduction: true));

        Assert.Null(ex);
    }
}
