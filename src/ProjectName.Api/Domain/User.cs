using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProjectName.Api.Domain;

public enum UserRole
{
    Viewer,
    Admin
}

/// <summary>
/// A user account. Usernames are globally unique so login can locate the
/// account without any other context. <see cref="TenantId"/> is null unless
/// the optional Tenancy module is in use.
/// </summary>
public sealed class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("tenantId")]
    public string? TenantId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
