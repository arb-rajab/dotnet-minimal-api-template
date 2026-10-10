namespace ProjectName.Api.Dtos;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt, string Role);

public sealed record CurrentUserResponse(string UserId, string Username, string Role);
