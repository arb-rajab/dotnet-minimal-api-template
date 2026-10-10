using MongoDB.Bson.Serialization.Conventions;

namespace ProjectName.Api.Infrastructure;

/// <summary>
/// Registers a camelCase element-name convention for any class in the Domain
/// namespace that is not explicitly mapped with [BsonElement], so documents
/// match the camelCase names System.Text.Json produces over the wire. Must
/// run once, before the driver touches any of the types.
/// </summary>
public static class MongoConventions
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        var pack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("projectname-camel-case", pack, t => t.Namespace == "ProjectName.Api.Domain");
        _registered = true;
    }
}
