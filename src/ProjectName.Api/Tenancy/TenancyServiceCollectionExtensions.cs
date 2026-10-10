namespace ProjectName.Api.Tenancy;

public static class TenancyServiceCollectionExtensions
{
    /// <summary>
    /// Registers the optional tenant module. Remove the <c>AddTenancy()</c>
    /// call in Program.cs and delete this folder (and the matching unit tests
    /// in tests/ProjectName.UnitTests/Tenancy) to drop it.
    /// </summary>
    public static IServiceCollection AddTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Scoped: it reads the claims of the current request. Anything that
        // depends on ITenantContext must be Scoped too. Singletons (such as
        // hosted services) must not depend on it.
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<ITenantRepository, TenantRepository>();
        services.AddHostedService<TenantIndexInitializer>();
        return services;
    }
}
