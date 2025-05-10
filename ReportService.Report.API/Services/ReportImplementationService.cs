using AutoMapper;
using PhoneBookMicroservices.Shared.Models;
using ReportService.Report.API.Dtos;
using ReportService.Report.API.Repositories;

namespace ReportService.Report.API.Services
{
    public class ReportImplementationService : IReportImplementationService
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;

        public ReportImplementationService(IReportRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ReportDto> CreateAsync(ReportRequestDto dto)
        {
            var report = new ContactReport
            {
                RequestedAt = DateTime.UtcNow,
                Status = ReportStatus.Preparing,
                Details = new List<ReportDetail> { new ReportDetail { Location = dto.Location, TotalContacts = 0, TotalPhoneNumbers = 0 } }
            };

            await _repository.AddAsync(report);
            return _mapper.Map<ReportDto>(report);
        }

        public async Task<ReportDto> GetByIdAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            return _mapper.Map<ReportDto>(report);
        }
    }

}
