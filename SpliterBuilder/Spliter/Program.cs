using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Spliter.Config;
using Spliter.Logic;
using System;
using Spliter.Config;
using System.IO;

namespace Spliter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Need to move it to the json file configuration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(@"C:\Dev\SpliterBuilder\Logs\log.txt")
                .CreateLogger();

            try
            {
                Log.Information(@"Service is up and running!");
                CreateHostBuilder(args).Build().Run();
                return;
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, @"There was a problem starting the service");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        { 
          return  Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    //Add Configuration
                    KafkaConfig kafkaConfig = hostContext.Configuration.GetSection("kafkaSection").Get<KafkaConfig>();
                    services.AddSingleton(kafkaConfig);

                    //Add singeltons
                    services.AddSingleton<IKafkaConnections, KafkaConnections>();

                    //Add worker
                    services.AddHostedService<Worker>();
                })
                .UseSerilog();
        }
    }
}
