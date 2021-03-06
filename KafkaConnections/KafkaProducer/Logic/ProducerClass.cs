using Confluent.Kafka;
using KafkaProducer.API;
using KafkaProducer.Config;
using KafkaProducer.DM;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Logic;

namespace KafkaProducer.Logic
{
    public class ProducerClass : IProducerClass
    {
        private readonly ILogger<ProducerClass> _logger;
        private readonly ConfigDM _configDM;
        public IApiHelper _api;

        public ProducerClass(ILogger<ProducerClass> logger, ConfigDM configDM)
        {
            _logger = logger;
            _configDM = configDM;
        }
        
        public async Task ToProduce()
        {
           _api = new ApiHelper(_configDM);
          
           #region Producer Configuration
            // Reliable Producer Configuration
            var _producerConfig = new ProducerConfig
            {
                ClientId = Dns.GetHostName(),
                EnableDeliveryReports = true,
                // Avoid connecting to IPv6 brokers:
                BrokerAddressFamily = BrokerAddressFamily.V4,
                Debug = "msg",
                // Retry settings:
                // Receive akcnowledgement from all sync replicas
                Acks = Acks.All,
                // Number of times to retry before giving up
                MessageSendMaxRetries = 3,
                // Duration to retry before next attempt
                RetryBackoffMs = 1000,
                // Set to true if you don't want to reorder messages on retry
                EnableIdempotence = true
            };
           #endregion

            foreach (var broker in _configDM.KafkaConnectionsConfig.Brokers)
            {
                _producerConfig.BootstrapServers = broker.BrokerName;

                using var producer = new ProducerBuilder<long, string>(_producerConfig)
                  .SetKeySerializer(Serializers.Int64)
                  .SetValueSerializer(Serializers.Utf8)
                  .SetLogHandler((_, message) =>
                      _logger.LogDebug($"Broker Name: {broker.BrokerName}, Facility: {message.Facility} - {message.Level} Message: {message.Message}"
                  ))
                  .SetErrorHandler((_, exception) =>
                      _logger.LogError($"Broker Name: {broker.BrokerName}, Error: {exception.Reason}. Is Fatal: {exception.IsFatal}"
                  ))
                  .Build();

                // Load data from API
                WeatherDM apiData = null;
                try
                {
                    apiData =await _api.LoadFromWeb<WeatherDM>();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"There was a problem with loading data from the web api", ex.Message);
                }

                // Producing Messages to all topics
                try
                {
                    foreach (var topic in broker.TopicsNames)
                    {
                        _logger.LogInformation("Producer loop started...");

                        //var personToSerialize = new PersonDM
                        //{
                        //    FirstName = "ofir",
                        //    LastName = "Ben Naim",
                        //    Date = DateTimeOffset.UtcNow
                        //};

                        // Serialize
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        //var message = JsonSerializer.Serialize(personToSerialize, options);
                        var message = JsonSerializer.Serialize(apiData, options);

                        var deliveryReport = await producer.ProduceAsync(topic, new Message<long, string>
                        {
                            Key = DateTime.UtcNow.Ticks,
                            Value = message
                        });

                        _logger.LogInformation($"Message sent:\n'{message}' \nwith the key: '{deliveryReport.Key}', to topic: {topic} in partiotion: {deliveryReport.Partition.Value}, at Offset: {deliveryReport.Offset.Value}. Delivery status: {deliveryReport.Status}\n");

                        if (deliveryReport.Status != PersistenceStatus.Persisted)
                        {
                            // Delivery might have failed after retries. This message requires manual proccessing.
                            _logger.LogError($"Error: Message not ack'd by all brokers (value: '{message}'. Delivery status: {deliveryReport.Status}");
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                catch (ProduceException<long, string> ex)
                {
                    // Log this message for manual processing
                    _logger.LogError($"Permanent error: {ex.Message} for message (value: '{ex.DeliveryResult.Value}')");
                    _logger.LogError("Exiting producer...");
                }
            }
        }
    }
}

