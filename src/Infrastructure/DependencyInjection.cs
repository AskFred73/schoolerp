using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Infrastructure.Data;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        bool useInMemoryDatabase = false)
    {
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<AuditSaveChangesInterceptor>(sp =>
        {
            var accessor = sp.GetRequiredService<ITenantContextAccessor>();
            return new AuditSaveChangesInterceptor(
                () => accessor.TenantId,
                () => accessor.UserId);
        });

        services.AddDbContext<SchoolErpDbContext>((sp, options) =>
        {
            if (useInMemoryDatabase)
                options.UseInMemoryDatabase("SchoolERP-Dev");
            else
                options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IStudent360Service, Student360Service>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IAcademicsService, AcademicsService>();
        services.AddScoped<ISmsNotificationService, NoOpSmsNotificationService>();

        return services;
    }
}
