using KafkaProducer.Logic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaProducer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IProducerClass _producerClass;

        public Worker(ILogger<Worker> logger, IProducerClass producerClass)
        {
            _logger = logger;
            _producerClass = producerClass;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError("Producer Service is down. closing now.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Producer Worker is running now...");

                await _producerClass.ToProduce();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
