using MongoDB.Driver;
using ProjectName.Api.Domain;
using ProjectName.Api.Infrastructure;

namespace ProjectName.Api.Repositories;

/// <summary>
/// Not scoped by anything but the username: login happens before any other
/// context is known, so usernames are globally unique.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    Task InsertAsync(User user, CancellationToken ct = default);

    Task<long> CountAsync(CancellationToken ct = default);
}

public sealed class UserRepository(MongoContext context) : IUserRepository
{
    private IMongoCollection<User> Collection => context.Users;

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await Collection.Find(u => u.Username == username).FirstOrDefaultAsync(ct);

    public Task InsertAsync(User user, CancellationToken ct = default) =>
        Collection.InsertOneAsync(user, cancellationToken: ct);

    public Task<long> CountAsync(CancellationToken ct = default) =>
        Collection.CountDocumentsAsync(FilterDefinition<User>.Empty, cancellationToken: ct);
}
