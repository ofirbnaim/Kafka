using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Spliter.Config;
using Spliter.Logic;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Spliter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
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
            return Host.CreateDefaultBuilder(args)
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
