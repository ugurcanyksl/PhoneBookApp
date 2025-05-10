namespace ReportService.Report.API.Infrastructure.Configuration
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }
    }
}
