using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ReportService.Report.API.Infrastructure.Kafka
{
    public class ReportConsumerHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReportConsumerHostedService> _logger;
        private CancellationTokenSource _cts;

        public ReportConsumerHostedService(IServiceScopeFactory scopeFactory, ILogger<ReportConsumerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                using var scope = _scopeFactory.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<ReportConsumer>();
                consumer.StartConsuming(_cts.Token);
            });

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