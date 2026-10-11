using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SchoolERP.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SchoolErpDbContext>
{
    public SchoolErpDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SchoolErpDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=school_erp;Username=schoolerp;Password=schoolerp_dev")
            .Options;
        return new SchoolErpDbContext(options);
    }
}
