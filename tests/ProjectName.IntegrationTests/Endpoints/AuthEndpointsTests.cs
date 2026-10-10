using System.Net;
using System.Net.Http.Json;
using ProjectName.Api.Dtos;
using ProjectName.Api.Infrastructure;
using ProjectName.IntegrationTests.Fixtures;

namespace ProjectName.IntegrationTests.Endpoints;

[Collection("Mongo collection")]
public class AuthEndpointsTests(MongoContainerFixture mongoFixture) : IntegrationTestBase(mongoFixture)
{
    [Fact]
    public async Task Login_WithSeededAdminCredentials_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(ApiFactory.AdminUsername, ApiFactory.AdminPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(ApiFactory.AdminUsername, "wrong-password"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("nobody@nowhere.test", "whatever"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithToken_ReturnsCurrentUser()
    {
        using var client = AuthenticatedClient(await LoginAsync());

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.Equal(ApiFactory.AdminUsername, body!.Username);
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminToken_ReturnsOk()
    {
        using var client = AuthenticatedClient(await LoginAsync());

        var response = await client.GetAsync("/api/auth/admin-check");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_IsAnonymous()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ExceedingRateLimit_Returns429()
    {
        // Same HttpClient/factory for every call in this test, so the same
        // partition key. Wrong password so none succeed for another reason.
        for (var i = 0; i < LoginRateLimiting.PermitLimit; i++)
        {
            var withinLimit = await Client.PostAsJsonAsync(
                "/api/auth/login", new LoginRequest(ApiFactory.AdminUsername, "wrong-password"));
            Assert.Equal(HttpStatusCode.BadRequest, withinLimit.StatusCode);
        }

        var overLimit = await Client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(ApiFactory.AdminUsername, "wrong-password"));
        Assert.Equal((HttpStatusCode)429, overLimit.StatusCode);
    }
}
