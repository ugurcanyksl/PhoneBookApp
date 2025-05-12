using ReportService.Report.API.Dtos;

namespace ReportService.Report.API.Services
{
    public interface IReportImplementationService
    {
        Task<ReportDto> CreateAsync(ReportRequestDto dto);
        Task<ReportDto> GetByIdAsync(Guid id);
        Task<List<ReportDto>> GetAllAsync();
        Task<List<ReportDto>> GetAllPagedAsync(int page, int pageSize);
    }
}
