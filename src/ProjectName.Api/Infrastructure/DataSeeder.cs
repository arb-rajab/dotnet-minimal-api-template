using ProjectName.Api.Auth;
using ProjectName.Api.Domain;
using ProjectName.Api.Repositories;

namespace ProjectName.Api.Infrastructure;

/// <summary>
/// Options for the development-only seeded admin account. Seeding happens only
/// when <c>Seed:AdminPassword</c> is configured, so nothing is created by
/// default and no password is committed.
/// </summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminUsername { get; set; } = "admin@example.test";

    public string AdminPassword { get; set; } = string.Empty;
}

/// <summary>
/// Seeds one admin user outside Production, only when the users collection is
/// empty and <c>Seed:AdminPassword</c> is set (for example through
/// <c>dotnet user-secrets</c> or the <c>Seed__AdminPassword</c> environment
/// variable).
/// </summary>
public sealed class DataSeeder(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    Microsoft.Extensions.Options.IOptions<SeedOptions> options,
    IHostEnvironment environment,
    ILogger<DataSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var seed = options.Value;
        if (environment.IsProduction() || string.IsNullOrEmpty(seed.AdminPassword))
        {
            return;
        }

        if (await users.CountAsync(cancellationToken) > 0)
        {
            logger.LogInformation("Users already present, skipping seeding");
            return;
        }

        await users.InsertAsync(new User
        {
            Username = seed.AdminUsername,
            PasswordHash = passwordHasher.Hash(seed.AdminPassword),
            Role = UserRole.Admin,
        }, cancellationToken);

        logger.LogInformation("Seeded admin user {Username}", seed.AdminUsername);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
