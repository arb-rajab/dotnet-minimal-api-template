using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProjectName.Api.Domain;

namespace ProjectName.Api.Infrastructure;

/// <summary>
/// Single place that knows collection names. Repositories depend on this
/// instead of calling IMongoDatabase.GetCollection directly, so a renamed
/// collection is a one-line change.
/// </summary>
public sealed class MongoContext
{
    public MongoContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        Database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<User> Users => Database.GetCollection<User>("users");
}
