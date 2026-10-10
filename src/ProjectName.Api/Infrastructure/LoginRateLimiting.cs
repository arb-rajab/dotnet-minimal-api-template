namespace ProjectName.Api.Infrastructure;

/// <summary>
/// Threshold for the fixed-window rate limiter applied to
/// POST /api/auth/login. Partitioned per client IP so one attacker cannot
/// exhaust other callers' allowance; a caller past the limit gets 429 with no
/// queueing.
///
/// 5 attempts / 60s per IP: generous enough that a human mistyping a password
/// a couple of times never sees it, low enough to make single-IP password
/// guessing impractical on top of BCrypt's cost. Change the numbers here and
/// in docs/project-memory/04-security.md together.
/// </summary>
public static class LoginRateLimiting
{
    public const string PolicyName = "login";

    public const int PermitLimit = 5;

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
}
