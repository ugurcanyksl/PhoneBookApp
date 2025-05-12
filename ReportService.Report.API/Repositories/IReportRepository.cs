using PhoneBookMicroservices.Shared.Models;

namespace ReportService.Report.API.Repositories
{
    public interface IReportRepository
    {
        Task<ContactReport> GetByIdAsync(Guid id);
        Task AddAsync(ContactReport report);
        Task<IEnumerable<Person>> GetContactDataByLocationAsync(string location);
        Task<List<ContactReport>> GetAllAsync();
        Task<List<ContactReport>> GetAllPagedAsync(int page, int pageSize);
    }
}
