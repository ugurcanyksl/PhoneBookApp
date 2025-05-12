using Confluent.Kafka;
using Newtonsoft.Json;
using ReportService.Report.API.Dtos;
using ReportService.Report.API.Services;

namespace ReportService.Report.API.Infrastructure.Kafka
{
    public class ReportConsumer
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<ReportConsumer> _logger;
        private readonly IReportImplementationService _reportService;

        public ReportConsumer(IConfiguration config, ILogger<ReportConsumer> logger, IReportImplementationService reportService)
        {
            var configProperties = new ConsumerConfig
            {
                GroupId = "report-service-group",
                BootstrapServers = config["Kafka:BootstrapServers"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Ignore, string>(configProperties).Build();
            _logger = logger;
            _reportService = reportService;
        }

        public void StartConsuming(CancellationToken cancellationToken)
        {
            _consumer.Subscribe("report-request-topic");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);
                        var reportRequestDto = JsonConvert.DeserializeObject<ReportRequestDto>(consumeResult.Message.Value);

                        _logger.LogInformation($"ContactReport request received: {consumeResult.Message.Value}");

                        
                        _reportService.CreateAsync(reportRequestDto).Wait();
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError($"Error while consuming message: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer was cancelled.");
            }
            finally
            {
                _consumer.Close();
            }
        }
    }

}
