using ProjectName.Api.Auth;
using ProjectName.Api.Dtos;
using ProjectName.Api.Infrastructure;
using ProjectName.Api.Services;

namespace ProjectName.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, IAuthService authService, CancellationToken ct) =>
        {
            var result = await authService.LoginAsync(request, ct);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .RequireRateLimiting(LoginRateLimiting.PolicyName)
        .WithName("Login")
        .WithSummary("Exchange credentials for a JWT.");

        group.MapGet("/me", (HttpContext http) =>
        {
            var user = http.User;
            return Results.Ok(new CurrentUserResponse(
                user.FindFirst("sub")?.Value ?? string.Empty,
                user.FindFirst("unique_name")?.Value ?? string.Empty,
                user.FindFirst(AppClaimTypes.Role)?.Value ?? string.Empty));
        })
        .RequireAuthorization()
        .WithName("CurrentUser")
        .WithSummary("Return the identity carried by the presented JWT.");

        group.MapGet("/admin-check", () => Results.Ok(new { admin = true }))
        .RequireAuthorization(AuthPolicies.Admin)
        .WithName("AdminCheck")
        .WithSummary("Example of an endpoint restricted by the Admin policy.");
    }
}
