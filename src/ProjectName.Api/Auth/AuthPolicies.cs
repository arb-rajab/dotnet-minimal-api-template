namespace ProjectName.Api.Auth;

public static class AuthPolicies
{
    /// <summary>Requires the <c>role</c> claim to be <c>Admin</c>.</summary>
    public const string Admin = "Admin";
}
