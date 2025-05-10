using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace ReportService.Report.API.Infrastructure
{
    public class ReportDbContextFactory : IDesignTimeDbContextFactory<ReportDbContext>
    {
        public ReportDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ReportDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=PhoneBookDb;Username=postgres;Password=Admin");

            return new ReportDbContext(optionsBuilder.Options);
        }
    }
}
