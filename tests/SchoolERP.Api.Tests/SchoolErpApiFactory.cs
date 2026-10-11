using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Infrastructure.Data;
using SchoolERP.Infrastructure.Seeding;
using Testcontainers.PostgreSql;

namespace SchoolERP.Api.Tests;

public class SchoolErpApiFactory : WebApplicationFactory<Program>
{
    private PostgreSqlContainer? _postgres;
    private readonly bool _useTestcontainers;
    private readonly string _databaseName = $"schoolerp-tests-{Guid.NewGuid():N}";
    private bool _initialized;
    private readonly object _initLock = new();

    public SchoolErpApiFactory()
    {
        _useTestcontainers = Environment.GetEnvironmentVariable("SCHOOLERP_USE_TESTCONTAINERS") == "1";
        if (_useTestcontainers)
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("school_erp_test")
                .WithUsername("schoolerp")
                .WithPassword("schoolerp_test")
                .Build();
        }
    }

    public void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;
            if (_postgres is not null)
                _postgres.StartAsync().GetAwaiter().GetResult();

            CreateClient();
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolErpDbContext>();
            if (_useTestcontainers)
                db.Database.Migrate();
            else
                db.Database.EnsureCreated();

            DemoDataSeeder.SeedAsync(db).GetAwaiter().GetResult();
            _initialized = true;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<SchoolErpDbContext>) ||
                d.ServiceType == typeof(SchoolErpDbContext)).ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            if (_useTestcontainers)
            {
                services.AddDbContext<SchoolErpDbContext>(options =>
                    options.UseNpgsql(_postgres!.GetConnectionString()));
            }
            else
            {
                services.AddDbContext<SchoolErpDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _postgres is not null)
            _postgres.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose(disposing);
    }
}
