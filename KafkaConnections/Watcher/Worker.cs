using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Config;
using Watcher.Logic;

namespace Watcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWatcherClass _watcher;
        private readonly ConfigDM _configDM;

        public Worker(ILogger<Worker> logger, IWatcherClass watcher, ConfigDM configDM)
        {
            _logger = logger;
            _watcher = watcher;
            _configDM = configDM;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError("Watcher Service is down. Closing now.");
            
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Watcher Service is running now...");

                //_watcher.Watch(new FileSystemWatcher(), _configDM.WatcherConfig.ScanPath);
                
                FileSystemWatcher watcher = new FileSystemWatcher();
                _watcher.Watch(watcher, _configDM.WatcherConfig.ScanPath);
                watcher.Dispose();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
