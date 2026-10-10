using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ProjectName.Api.Auth;
using ProjectName.Api.Domain;
using ProjectName.Api.Endpoints;
using ProjectName.Api.Infrastructure;
using ProjectName.Api.Repositories;
using ProjectName.Api.Services;
using ProjectName.Api.Tenancy;
using Scalar.AspNetCore;

MongoConventions.Register();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection(MongoOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoOptions>>().Value;
    return new MongoClient(options.ConnectionString);
});
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddHostedService<MongoIndexInitializer>();
builder.Services.AddHostedService<DataSeeder>();

// Optional module: remove this line together with the Tenancy/ folder.
builder.Services.AddTenancy();

// UserRepository holds no per-request state (it only wraps the singleton
// MongoContext), so it is a Singleton. That lets DataSeeder, a Singleton
// IHostedService, depend on it. A Singleton hosted service must never depend
// on a Scoped service: `dotnet build` does not catch this, but a host started
// with scope validation on (WebApplicationFactory in Development) does.
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Fail fast at startup if misconfigured. The value used for signature
// validation is re-read from configuration *inside* the AddJwtBearer delegate
// below rather than captured here: that delegate runs lazily (after
// builder.Build()), and WebApplicationFactory-based tests merge their
// configuration overrides in at Build() time. A value captured in a local
// variable up here would be the pre-override one while JwtTokenService (which
// resolves IOptions<JwtOptions> lazily) signs with the overridden one, so
// every issued token would fail validation with a spurious 401.
var configuredJwtSecret = builder.Configuration[$"{JwtOptions.SectionName}:Secret"];
if (string.IsNullOrEmpty(configuredJwtSecret))
{
    throw new InvalidOperationException("Jwt:Secret is not configured.");
}

// A non-empty secret that is still the known dev placeholder must not boot in
// Production. This is a one-shot startup check, not a value reused for
// validation, so it does not fall into the stale-capture trap above.
JwtSecretGuard.EnsureNotPlaceholder(configuredJwtSecret, builder.Environment.IsProduction());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Without this, JwtSecurityTokenHandler silently remaps well-known
        // short claim types on validation ("sub" -> ClaimTypes.NameIdentifier,
        // "role" -> ClaimTypes.Role, ...). AppClaimTypes.Role ("role") and the
        // tenant context read the literal short names JwtTokenService issues,
        // so leaving the default mapping on makes RequireClaim("role", "Admin")
        // never match and every admin request gets a false 403.
        options.MapInboundClaims = false;

        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.Admin, policy => policy.RequireClaim(AppClaimTypes.Role, nameof(UserRole.Admin)));

// Threshold and rationale are documented once in LoginRateLimiting.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(LoginRateLimiting.PolicyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = LoginRateLimiting.PermitLimit,
                Window = LoginRateLimiting.Window,
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
});

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// TLS is terminated upstream (container platform / load balancer) in
// non-Development environments, so there is no local HTTPS endpoint to
// redirect to there.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapHealthEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
