using AutoMapper;
using PhoneBookMicroservices.Shared.Models;
using ReportService.Report.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService.Tests.Dtos
{
    public class ReportDtoTests
    {
        private readonly IMapper _mapper;

        public ReportDtoTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ContactReport, ReportDto>()
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
                cfg.CreateMap<ReportRequestDto, ContactReport>();
                cfg.CreateMap<ReportDetail, ReportDetailDto>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void ContactReport_ShouldInitializeWithDefaultValues()
        {
            var report = new ContactReport();

            Assert.Equal(Guid.Empty, report.Id);
            Assert.Equal(default(DateTime), report.RequestedAt);
            Assert.Equal(ReportStatus.Preparing, report.Status);
            Assert.Null(report.Details);
        }

        [Fact]
        public void ContactReport_ShouldSetPropertiesCorrectly()
        {
            var reportId = Guid.NewGuid();
            var requestedAt = DateTime.Now;

            var report = new ContactReport
            {
                Id = reportId,
                RequestedAt = requestedAt,
                Status = ReportStatus.Completed,
                Details = new List<ReportDetail>
                {
                    new ReportDetail { Id = Guid.NewGuid(), Location = "Istanbul", TotalContacts = 10, TotalPhoneNumbers = 5 }
                }
            };

            Assert.Equal(reportId, report.Id);
            Assert.Equal(requestedAt, report.RequestedAt);
            Assert.Equal(ReportStatus.Completed, report.Status);
            Assert.Single(report.Details);
            Assert.Equal("Istanbul", report.Details[0].Location);
        }

        [Fact]
        public void ContactReport_ShouldHandleFutureRequestedAt()
        {
            var reportId = Guid.NewGuid();
            var futureDate = DateTime.Now.AddDays(1);

            var report = new ContactReport
            {
                Id = reportId,
                RequestedAt = futureDate,
                Status = ReportStatus.Preparing
            };

            Assert.Equal(futureDate, report.RequestedAt);
            Assert.True(report.RequestedAt > DateTime.Now);
        }

        [Fact]
        public void ContactReport_To_ReportDto_Mapping_ShouldWork()
        {
            var contactReport = new ContactReport
            {
                Id = Guid.NewGuid(),
                RequestedAt = DateTime.Now,
                Status = ReportStatus.Completed,
                Details = new List<ReportDetail>
                {
                    new ReportDetail { Id = Guid.NewGuid(), Location = "Istanbul", TotalContacts = 10, TotalPhoneNumbers = 5 }
                }
            };

            var reportDto = _mapper.Map<ReportDto>(contactReport);

            Assert.Equal(contactReport.Id, reportDto.Id);
            Assert.Equal(contactReport.RequestedAt, reportDto.RequestedAt);
            Assert.Equal(contactReport.Status.ToString(), reportDto.Status);
            Assert.NotNull(reportDto.Details);
            Assert.Single(reportDto.Details);
            Assert.Equal(contactReport.Details[0].Location, reportDto.Details[0].Location);
        }

        [Fact]
        public void ReportDto_ShouldInitializeWithDefaultValues()
        {
            var reportDto = new ReportDto();

            Assert.Equal(Guid.Empty, reportDto.Id);
            Assert.Equal(default(DateTime), reportDto.RequestedAt);
            Assert.Null(reportDto.Status);
            Assert.Null(reportDto.Details);
        }

        [Fact]
        public void ReportDto_ShouldSetPropertiesCorrectly()
        {
            var reportId = Guid.NewGuid();
            var requestedAt = DateTime.Now;

            var reportDto = new ReportDto
            {
                Id = reportId,
                RequestedAt = requestedAt,
                Status = "Completed",
                Details = new List<ReportDetailDto>
                {
                    new ReportDetailDto { Id = Guid.NewGuid(), Location = "Istanbul", TotalContacts = 10, TotalPhoneNumbers = 5 }
                }
            };

            Assert.Equal(reportId, reportDto.Id);
            Assert.Equal(requestedAt, reportDto.RequestedAt);
            Assert.Equal("Completed", reportDto.Status);
            Assert.Single(reportDto.Details);
            Assert.Equal("Istanbul", reportDto.Details[0].Location);
        }
    }
}
