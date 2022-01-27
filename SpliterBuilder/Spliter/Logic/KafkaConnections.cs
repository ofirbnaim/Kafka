using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spliter.Config;
using Spliter.DM;
using static Spliter.Config.KafkaConfig;

namespace Spliter.Logic
{
    public class KafkaConnections : IKafkaConnections
    {
        private readonly ILogger<KafkaConnections> _logger;
        private readonly KafkaConfig _kafkaConfig;

        public KafkaConnections(ILogger<KafkaConnections> logger, KafkaConfig kafkaConfig)
        {
            _logger = logger;
            _kafkaConfig = kafkaConfig;
        }

        public void ToConsume()
        {
            var _consumerConfig = new ConsumerConfig
            {
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                MaxPollIntervalMs = 300000,
                GroupId = _kafkaConfig.kafkaConnectionsConfig.GroupID,

                // Read messages from start if no commit exists.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            foreach (var broker in _kafkaConfig.kafkaConnectionsConfig.Brokers)
            {
                _consumerConfig.BootstrapServers = broker.BrokerName;

                using var consumer = new ConsumerBuilder<long, string>(_consumerConfig)
                    .SetKeyDeserializer(Deserializers.Int64)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .SetLogHandler((_, message) => _logger.LogDebug($"Facility: {message.Facility}, Message: {message}"
                    ))
                    .SetErrorHandler((_, exception) => _logger.LogError($"Error: {exception.Reason}, Is Fatal: {exception.IsFatal}"
                    ))
                    .Build();

                try
                {
                    // Subscribe to all Topics (more than one)
                    consumer.Subscribe(broker.TopicsNames);

                   _logger.LogInformation("Consumer loop started...");

                    // This is indeed an infinite loop. Consumers are usually long-running applications that continuously poll Kafka for more data.
                    while (true)
                    {
                        // Blocks until a consume result is available, or the time out period has elapsed
                        var result = consumer.Consume(TimeSpan.FromMilliseconds(_consumerConfig.MaxPollIntervalMs - 1000 ?? 250000));

                        // Get the message value - null is possible
                        var message = result?.Message?.Value;
                        PersonDM personToDeserialize = JsonSerializer.Deserialize<PersonDM>(message);

                        if (message == null)
                        {
                            continue;
                        }

                        File.WriteAllText(@"C:\Users\ofirb\source\repos\ofirbnaim\Kafka\SpliterBuilder\Files\File_" + Guid.NewGuid().ToString()+".json", message);

                       _logger.LogInformation($"Message received:\n'{message}'\nwith the key: '{result.Message.Key}', In Partition: {result.Partition.Value}, at Offset: {result.Offset.Value}\n");

                        /* The committed position, is the last offset that has been stored securely.
                        Should the process fail and restart, this is the offset that the consumer will recover to */
                        consumer.Commit(result);
                        consumer.StoreOffset(result);
                     
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                catch (KafkaException ex)
                {
                    _logger.LogError($"Consumer Error: {ex.Message}");
                    _logger.LogError("Exiting producer...");
                }
                finally
                {
                    consumer.Close();
                }
            }
        }

        public async Task ToProduce()
        {
            // Reliable Producer Configuration
            var _producerConfig = new ProducerConfig
            {
                ClientId = Dns.GetHostName(),
                EnableDeliveryReports = true,

                Debug = "msg",
                #region 
                // Retry settings:
                // Receive akcnowledgement from all sync replicas
                Acks = Acks.All,
                // Number of times to retry before giving up
                MessageSendMaxRetries = 3,
                // Duration to retry before next attempt
                RetryBackoffMs = 1000,
                // Set to true if you don't want to reorder messages on retry
                EnableIdempotence = true
                #endregion
            };

            foreach (var broker in _kafkaConfig.kafkaConnectionsConfig.Brokers)
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

                // Producing Messages to all topics
                try
                {
                    foreach (var topic in broker.TopicsNames)
                    {
                        _logger.LogInformation("Producer loop started...");

                        var personToSerialize = new PersonDM
                        {
                            FirstName = "ofir",
                            LastName = "Ben Naim",
                            Date = DateTimeOffset.Now
                        };

                        // Serialize
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        var message = JsonSerializer.Serialize(personToSerialize, options);

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
