using Microsoft.Extensions.Options;
using ProjectName.Api.Auth;
using ProjectName.Api.Dtos;
using ProjectName.Api.Repositories;

namespace ProjectName.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, ct);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Deliberately identical error for "no such user" and "wrong
            // password": distinguishing them lets an attacker enumerate
            // valid usernames.
            throw new ValidationException("Invalid username or password.");
        }

        var token = tokenService.CreateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes);

        return new LoginResponse(token, expiresAt, user.Role.ToString());
    }
}
