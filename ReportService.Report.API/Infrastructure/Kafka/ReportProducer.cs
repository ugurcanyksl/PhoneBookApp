using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReportService.Report.API.Dtos;
using ReportService.Report.API.Infrastructure.Configuration;
using ReportService.Report.API.IntegrationEvents;

namespace ReportService.Report.API.Infrastructure.Kafka
{
    public class ReportProducer : IReportProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<ReportProducer> _logger;
        private readonly string _reportRequestTopic;
        private readonly string _reportCreatedTopic;

        public ReportProducer(IConfiguration config, ILogger<ReportProducer> logger, IOptions<KafkaSettings> kafkaSettings)
        {
            var configProperties = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"]
            };
            _producer = new ProducerBuilder<Null, string>(configProperties).Build();
            _logger = logger;

            _reportRequestTopic = config["Kafka:ReportRequestTopic"] ?? "report-request-topic";
            _reportCreatedTopic = kafkaSettings.Value.Topic ?? "report-created-event";
        }

        
        public async Task SendReportRequestAsync(ReportRequestDto reportRequestDto)
        {
            try
            {
                var message = JsonConvert.SerializeObject(reportRequestDto);
                var deliveryResult = await _producer.ProduceAsync(_reportRequestTopic, new Message<Null, string> { Value = message });
                _logger.LogInformation($"ContactReport request sent to Kafka: {deliveryResult.Value}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending report request: {ex.Message}");
            }
        }

        
        public async Task SendReportCreatedEventAsync(ReportCreatedIntegrationEvent integrationEvent)
        {
            try
            {
                var message = JsonConvert.SerializeObject(integrationEvent);
                var deliveryResult = await _producer.ProduceAsync(_reportCreatedTopic, new Message<Null, string> { Value = message });
                _logger.LogInformation("Kafka - Rapor oluşturma eventi gönderildi: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka - report-created-event gönderilirken hata.");
                throw;
            }
        }
    }

}
