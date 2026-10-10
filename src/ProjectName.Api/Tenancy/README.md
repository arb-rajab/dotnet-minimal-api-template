# Tenancy (optional module)

Multi-tenant building blocks. Nothing outside this folder depends on it except
one `AddTenancy()` call in `Program.cs`, the nullable `User.TenantId` field, and
the `tenant_id` claim that `JwtTokenService` adds when a user has a tenant.

To remove it: delete this folder, delete `tests/ProjectName.UnitTests/Tenancy/`,
and remove `builder.Services.AddTenancy();` from `Program.cs`.

To use it:

- Give each tenant-owned document a `TenantId` and index it with `tenantId`
  first.
- Make repositories for tenant-owned collections Scoped and take
  `ITenantContext`; always filter by `tenantContext.TenantId`. Never accept a
  tenant id from the request body, query string or a header.
- Set `User.TenantId` when creating users; login then issues a token carrying
  the claim.
- Hosted services (Singleton) cannot depend on `ITenantContext`.
