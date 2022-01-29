using KafkaConsumer.Logic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaConsumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumerClass _consumerClass;

        public Worker(ILogger<Worker> logger, IConsumerClass consumerClass)
        {
            _logger = logger;
            _consumerClass = consumerClass;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError("Consumer Service is down. closing now.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Consumer Worker is running now...");

                _consumerClass.ToConsume();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

