using PhoneBookMicroservices.Shared.Models;

namespace ReportService.Report.API.Repositories
{
    public interface IReportRepository
    {
        Task<ContactReport> GetByIdAsync(Guid id);
        Task AddAsync(ContactReport report);
    }
}
