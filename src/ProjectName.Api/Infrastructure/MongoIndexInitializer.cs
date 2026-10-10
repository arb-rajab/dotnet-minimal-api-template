using MongoDB.Driver;
using ProjectName.Api.Domain;

namespace ProjectName.Api.Infrastructure;

/// <summary>
/// Creates indexes idempotently at startup (CreateOneAsync is a no-op if an
/// equivalent index already exists). Add one method per collection as the
/// domain grows.
/// </summary>
public sealed class MongoIndexInitializer(MongoContext context, ILogger<MongoIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CreateUserIndexesAsync(cancellationToken);
        logger.LogInformation("MongoDB indexes ensured");
    }

    private async Task CreateUserIndexesAsync(CancellationToken cancellationToken)
    {
        var keys = Builders<User>.IndexKeys;
        await context.Users.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(keys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true, Name = "username_unique" }),
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
