using MongoDB.Driver;
using ProjectName.Api.Infrastructure;

namespace ProjectName.Api.Tenancy;

public interface ITenantRepository
{
    Task InsertAsync(Tenant tenant, CancellationToken ct = default);

    Task<long> CountAsync(CancellationToken ct = default);

    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
}

public sealed class TenantRepository(MongoContext context) : ITenantRepository
{
    private IMongoCollection<Tenant> Collection => context.Database.GetCollection<Tenant>("tenants");

    public Task InsertAsync(Tenant tenant, CancellationToken ct = default) =>
        Collection.InsertOneAsync(tenant, cancellationToken: ct);

    public Task<long> CountAsync(CancellationToken ct = default) =>
        Collection.CountDocumentsAsync(FilterDefinition<Tenant>.Empty, cancellationToken: ct);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await Collection.Find(t => t.Slug == slug).FirstOrDefaultAsync(ct);
}
