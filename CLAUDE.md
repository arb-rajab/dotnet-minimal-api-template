# CLAUDE.md

Notes for Claude Code sessions working in this repo. Add an item only when it
would have saved a round trip.

## Commands

```bash
dotnet build ProjectName.slnx --configuration Release     # about 5-30 s
dotnet format ProjectName.slnx --verify-no-changes --severity warn
dotnet test tests/ProjectName.UnitTests/ProjectName.UnitTests.csproj
dotnet test tests/ProjectName.IntegrationTests/ProjectName.IntegrationTests.csproj   # Docker
```

Run build and format before every push; both are cheap. If `rename.sh` still
exists, the project has not been renamed yet.

## SDK installation

If `dotnet` is missing in a fresh sandbox, `apt-get install -y dotnet-sdk-10.0`
(run `apt-get update` first). `dotnet-install.sh` downloads from
`builds.dotnet.microsoft.com`, which sandbox egress policies commonly deny.

## Sandbox limits

- Docker Hub's blob CDN is commonly blocked, so `docker pull mongo:*` and the
  Testcontainers Ryuk image fail. The integration suite is then verified by CI,
  not locally. Check the egress proxy status endpoint for a policy denial before
  debugging Testcontainers.
- `docker build` steps that run `dotnet restore` execute inside the container's
  own network namespace, which a sandbox egress proxy may not reach. Trust the
  `docker-build` CI job.
- `docker info` reporting no daemon does not always mean Docker is unusable:
  `service docker start` can fail on a `ulimit` error while `dockerd &` works.

## Fixed bugs: do not reintroduce

In `Program.cs` auth and DI wiring:

1. A Singleton `IHostedService` (`AddHostedService`) must not depend on a Scoped
   service. `dotnet build` and `dotnet run` do not catch this; a host with scope
   validation on (`WebApplicationFactory` in Development) does. Check dependency
   lifetimes when adding a hosted service. This is why `IUserRepository` is a
   Singleton and `ITenantContext` is Scoped.
2. Never capture a configuration value into a local variable before
   `builder.Build()` if a test host may override it. Overrides from
   `WebApplicationFactory.ConfigureWebHost` are merged in at `Build()`. Read
   configuration lazily inside options delegates (`AddJwtBearer(options => ...)`).
   `JwtSecretConfigurationTests` pins the mechanism.
3. `JwtBearerOptions.MapInboundClaims` defaults to `true` and silently remaps
   short claim types (`role`, `sub`) to long `ClaimTypes.*` URIs. This project
   issues and reads the short names (`AppClaimTypes`), so `MapInboundClaims =
   false` is required. `InboundClaimMappingTests` pins it.

## Fast check for DI problems without a database

Run the built app against an unreachable Mongo with scope validation on:

```bash
cd src/ProjectName.Api
ASPNETCORE_ENVIRONMENT=Development Mongo__ConnectionString='mongodb://localhost:1/?serverSelectionTimeoutMS=1500' \
  dotnet bin/Release/net10.0/ProjectName.Api.dll
```

`builder.Build()` throws immediately on an invalid graph. A healthy graph gets as
far as the first hosted service timing out on Mongo. Run from the project folder
so `appsettings.Development.json` is found.

## Where to extend

- New domain: copy the `User` shape (`Domain/` entity with `[BsonElement]`,
  `Repositories/` interface + class, `Services/`, `Endpoints/`, `Dtos/`), add the
  collection to `MongoContext` and its indexes to `MongoIndexInitializer`.
- Put query-building logic in `internal` static methods (the Api project already
  has `InternalsVisibleTo` for both test projects) so the shape of a Mongo
  pipeline can be unit tested as plain `BsonDocument` construction without a
  database. Only execution needs the integration suite.
- Tenancy and the worker host are optional; see their folder READMEs.
- Anything that talks to a real service belongs in `ProjectName.IntegrationTests`
  with a Testcontainers fixture, one container shared per collection and one
  database name per test class.

## Security rules

- Never commit real secrets. `appsettings.Development.json` holds a dev-only
  placeholder that `JwtSecretGuard` refuses in Production. Keep the constant in
  `JwtSecretGuard.KnownDevPlaceholder` and that file in sync.
- Login must return the same error for unknown user and wrong password.
- `osv-scanner.toml` and `.gitleaksignore` are empty on purpose. Exemptions must
  be dated and justified.
