using MongoDB.Driver;
using ProjectName.Api.Infrastructure;

namespace ProjectName.Api.Tenancy;

/// <summary>
/// Ensures the tenant collection's indexes. Tenant-owned collections should
/// lead their compound indexes with <c>tenantId</c> and add them here or in
/// their own initializer.
/// </summary>
public sealed class TenantIndexInitializer(MongoContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var keys = Builders<Tenant>.IndexKeys;
        await context.Database.GetCollection<Tenant>("tenants").Indexes.CreateOneAsync(
            new CreateIndexModel<Tenant>(keys.Ascending(t => t.Slug),
                new CreateIndexOptions { Unique = true, Name = "slug_unique" }),
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
