using Microsoft.EntityFrameworkCore;
using ReportService.Report.API.Infrastructure;
using PhoneBookMicroservices.Shared.Models;

namespace ReportService.Report.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDbContext _dbContext;

        public ReportRepository(ReportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ContactReport> GetByIdAsync(Guid id) =>
            await _dbContext.Reports.Include(r => r.Details).FirstOrDefaultAsync(r => r.Id == id);

        public async Task AddAsync(ContactReport report) =>
            await _dbContext.Reports.AddAsync(report);
    }
}
