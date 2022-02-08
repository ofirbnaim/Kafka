using KafkaProducer.API;
using KafkaProducer.Config;
using KafkaProducer.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KafkaProducer
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
                Log.Information(@"Producer Service is up and running!");
                CreateHostBuilder(args).Build().Run();
                return;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, @"There was a problem starting the Producer service");
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
                   ConfigDM config = hostContext.Configuration.GetSection("kafkaSection").Get<ConfigDM>();
                   services.AddSingleton(config);

                   //Add singeltons
                   services.AddSingleton<IProducerClass, ProducerClass>();
                   services.AddSingleton<IApiHelper, ApiHelper>();

                   //Add worker
                   services.AddHostedService<Worker>();
               })
               .UseSerilog();
        }
    }
}
