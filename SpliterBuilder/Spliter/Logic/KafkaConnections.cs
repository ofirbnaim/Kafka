using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spliter.Config;
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
                GroupId = _kafkaConfig.kafkaConnectionsConfig.GroupName,

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
                    //foreach (var topic in broker.TopicName)
                    //{
                        consumer.Subscribe(broker.TopicName);

                        _logger.LogInformation("Consumer loop started...");

                        // This is indeed an infinite loop. Consumers are usually long-running applications that continuously poll Kafka for more data.
                        while (true)
                        {
                        // Blocks until a consume result is available, or the time out period has elapsed
                        var result = consumer.Consume(TimeSpan.FromMilliseconds(_consumerConfig.MaxPollIntervalMs - 1000 ?? 250000));


                            // Get the message value - null is possible
                            var message = result?.Message?.Value;
                            if(message == null)
                            {
                                continue;
                            }

                            _logger.LogInformation($"Message received: {result.Message.Key}: {message} from Topic: {broker.TopicName} In Partition: {result.Partition.Value}");

                            /* The committed position, is the last offset that has been stored securely.
                             Should the process fail and restart, this is the offset that the consumer will recover to */
                            consumer.Commit(result);

                            consumer.StoreOffset(result);
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                        }
                    //}
                }
                catch(KafkaException ex)
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
                    foreach (var topic in broker.TopicName)
                    {
                        _logger.LogInformation("Producer loop started...");

                        for (var character = 'A'; character <= 'A'; character++)
                        {
                            // Write a character with Uniqe Stemp Time
                            var message = $"Character #{character} sent at {DateTime.Now:yyyy-MM-dd-HH:mm:ss}";

                            var deliveryReport = await producer.ProduceAsync(topic, new Message<long, string>
                            {
                                Key = DateTime.UtcNow.Ticks,
                                Value = message
                            });

                            _logger.LogInformation($"Message sent (value: '{message}') to topic: {topic} in partiotion: {deliveryReport.Partition.Value}. Delivery status: {deliveryReport.Status}");

                            if (deliveryReport.Status != PersistenceStatus.Persisted)
                            {
                                // Delivery might have failed after retries. This message requires manual proccessing.
                                _logger.LogError($"Error: Message not ack'd by all brokers (value: '{message}'. Delivery status: {deliveryReport.Status}");
                            }

                            Thread.Sleep(TimeSpan.FromSeconds(2));
                        }
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
