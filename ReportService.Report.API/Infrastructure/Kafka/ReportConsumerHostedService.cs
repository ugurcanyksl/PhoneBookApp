namespace ReportService.Report.API.Infrastructure.Kafka
{
    public class ReportConsumerHostedService : IHostedService
    {
        private readonly ReportConsumer _consumer;
        private readonly ILogger<ReportConsumerHostedService> _logger;
        private CancellationTokenSource _cts;

        public ReportConsumerHostedService(ReportConsumer consumer, ILogger<ReportConsumerHostedService> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => _consumer.StartConsuming(_cts.Token));
            _logger.LogInformation("ContactReport Consumer started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            _logger.LogInformation("ContactReport Consumer stopped.");
            return Task.CompletedTask;
        }
    }

}
