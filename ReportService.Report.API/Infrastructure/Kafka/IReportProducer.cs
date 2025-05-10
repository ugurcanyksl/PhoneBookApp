using ReportService.Report.API.Dtos;
using ReportService.Report.API.IntegrationEvents;

namespace ReportService.Report.API.Infrastructure.Kafka
{
    public interface IReportProducer
    {
        Task SendReportRequestAsync(ReportRequestDto reportRequestDto);
        Task SendReportCreatedEventAsync(ReportCreatedIntegrationEvent integrationEvent);
    }
}
