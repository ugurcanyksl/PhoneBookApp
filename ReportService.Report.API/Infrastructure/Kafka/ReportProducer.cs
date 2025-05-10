using Confluent.Kafka;
using Newtonsoft.Json;
using ReportService.Report.API.Dtos;

namespace ReportService.Report.API.Infrastructure.Kafka
{
    public class ReportProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<ReportProducer> _logger;

        public ReportProducer(IConfiguration config, ILogger<ReportProducer> logger)
        {
            var configProperties = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"]
            };
            _producer = new ProducerBuilder<Null, string>(configProperties).Build();
            _logger = logger;
        }

        public async Task SendReportRequestAsync(ReportRequestDto reportRequestDto)
        {
            try
            {
                var message = JsonConvert.SerializeObject(reportRequestDto);
                var deliveryResult = await _producer.ProduceAsync("report-request-topic", new Message<Null, string> { Value = message });
                _logger.LogInformation($"ContactReport request sent to Kafka: {deliveryResult.Value}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending report request: {ex.Message}");
            }
        }
    }

}
