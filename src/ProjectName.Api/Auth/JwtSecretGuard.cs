namespace ProjectName.Api.Auth;

/// <summary>
/// Startup-time guard against the known local-development placeholder JWT
/// secret reaching a real deployment. A pure, static check so it is
/// unit-testable without a full host.
/// </summary>
public static class JwtSecretGuard
{
    /// <summary>
    /// Must match the literal value committed in appsettings.Development.json.
    /// Intentionally duplicated as a constant (not read back from that file):
    /// the point is to catch this specific string reaching Production,
    /// however it got there.
    /// </summary>
    public const string KnownDevPlaceholder = "dev-only-signing-secret-change-me-please-32-bytes-min";

    /// <summary>
    /// Throws if <paramref name="secret"/> is the known dev placeholder and
    /// <paramref name="isProduction"/> is true. A missing or empty secret is a
    /// separate check in Program.cs.
    /// </summary>
    public static void EnsureNotPlaceholder(string? secret, bool isProduction)
    {
        if (isProduction && secret == KnownDevPlaceholder)
        {
            throw new InvalidOperationException(
                "Jwt:Secret is set to the known local-development placeholder value. " +
                "Refusing to start in Production. Set Jwt__Secret (or PROJECTNAME_JWT_SECRET " +
                "for docker-compose) to a real, unique secret.");
        }
    }
}
