namespace ReportService.Report.API.IntegrationEvents
{
    public class ReportCreatedIntegrationEvent
    {
        public Guid ReportId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FilePath { get; set; }  // Opsiyonel

        public ReportCreatedIntegrationEvent(Guid reportId, string status, DateTime createdAt, string? filePath = null)
        {
            ReportId = reportId;
            Status = status;
            CreatedAt = createdAt;
            FilePath = filePath;
        }
    }
}
