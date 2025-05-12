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
                Id = Guid.NewGuid(),
                RequestedAt = DateTime.UtcNow,
                Status = ReportStatus.Preparing,
                Details = new List<ReportDetail>()
            };

            var contactData = await _repository.GetContactDataByLocationAsync(dto.Location);
            var totalContacts = contactData.Count();
            var totalPhoneNumbers = contactData.SelectMany(c => c.ContactInfos)
                                             .Count(ci => ci.InfoType == InfoType.PhoneNumber);

            report.Details.Add(new ReportDetail
            {
                Id = Guid.NewGuid(),
                Location = dto.Location,
                TotalContacts = totalContacts,
                TotalPhoneNumbers = totalPhoneNumbers
            });

            report.Status = ReportStatus.Completed;
            await _repository.AddAsync(report);
            return _mapper.Map<ReportDto>(report);
        }

        public async Task<ReportDto> GetByIdAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            return _mapper.Map<ReportDto>(report);
        }

        public async Task<List<ReportDto>> GetAllAsync()
        {
            var reports = await _repository.GetAllAsync();
            return _mapper.Map<List<ReportDto>>(reports);
        }

        public async Task<List<ReportDto>> GetAllPagedAsync(int page, int pageSize)
        {
            var reports = await _repository.GetAllPagedAsync(page, pageSize);
            return _mapper.Map<List<ReportDto>>(reports);
        }
    }
}
