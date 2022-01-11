using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spliter.Config;
using Spliter.Logic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spliter
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IKafkaConnections _kafkaConnections;

        public Worker(ILogger<Worker> logger, IKafkaConnections kafkaConnections)
        {
            _logger = logger;
            _kafkaConnections = kafkaConnections;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError("Service is down. closing now.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                
                //Create new producer
                _kafkaConnections.ToProduce();
                

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
