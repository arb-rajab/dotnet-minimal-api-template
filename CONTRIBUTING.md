# Contributing

Thanks for helping improve this project.

## Workflow

1. Open an issue (or comment on an existing one) before large changes.
2. Branch from `main`; keep branches focused on one change.
3. Run the build, lint/format checks and tests locally (see the README) before
   pushing. CI runs the same checks plus security scans.
4. Open a pull request using the template. Describe what changed, why, and how
   you verified it.
5. Keep commits readable. Squash noise before merge.

## Standards

- Add or update tests with every behaviour change.
- Never commit secrets. Use `.env.example` (or the equivalent) for variable
  names only. CI runs a secret scan.
- Dependency exemptions for the vulnerability scanner must be dated and carry a
  reason (see `osv-scanner.toml`).
- Record significant decisions in `docs/project-memory/07-decisions.md`.

## Reporting security issues

Do not open a public issue. See `SECURITY.md`.
