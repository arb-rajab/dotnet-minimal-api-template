namespace ProjectName.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC signing secret. MUST be overridden via environment variable /
    /// secret store in any non-local environment. The value in
    /// appsettings.Development.json is a local-development placeholder only.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "projectname";

    public string Audience { get; set; } = "projectname-clients";

    public int ExpiryMinutes { get; set; } = 60;
}
