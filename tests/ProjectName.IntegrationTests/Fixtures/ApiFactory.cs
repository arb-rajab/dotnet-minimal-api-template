using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ProjectName.IntegrationTests.Fixtures;

/// <summary>
/// Boots the real API (Program.cs) against a real mongod, pointed at a unique
/// per-instance database name so each test class starts from an empty
/// database, seeded only with the admin user the factory configures.
/// </summary>
public sealed class ApiFactory(string mongoConnectionString) : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin@example.test";

    public const string AdminPassword = "Passw0rd!";

    public string DatabaseName { get; } = $"projectname_test_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development enables DI scope validation, which catches a Singleton
        // hosted service depending on a Scoped service.
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mongo:ConnectionString"] = mongoConnectionString,
                ["Mongo:DatabaseName"] = DatabaseName,
                ["Jwt:Secret"] = "integration-test-signing-secret-at-least-32-bytes",
                ["Jwt:Issuer"] = "projectname",
                ["Jwt:Audience"] = "projectname-clients",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Seed:AdminUsername"] = AdminUsername,
                ["Seed:AdminPassword"] = AdminPassword,
            });
        });
    }
}
