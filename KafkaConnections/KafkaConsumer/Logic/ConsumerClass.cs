using Confluent.Kafka;
using KafkaConsumer.Config;
using KafkaConsumer.DM;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using Watcher.Logic;

namespace KafkaConsumer.Logic
{
    public class ConsumerClass : IConsumerClass
    {
        private readonly ILogger<ConsumerClass> _logger;
        private readonly ConfigDM _configDM;

        public ConsumerClass(ILogger<ConsumerClass> logger, ConfigDM configDM)
        {
            _logger = logger;
            _configDM = configDM;
        }

        public void ToConsume()
        {
            var _consumerConfig = new ConsumerConfig
            {
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                MaxPollIntervalMs = 300000,
                GroupId = _configDM.kafkaConnectionsConfig.GroupID,
                // Avoid connecting to IPv6 brokers:
                BrokerAddressFamily = BrokerAddressFamily.V4,
                // Read messages from start if no commit exists.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            foreach (var broker in _configDM.kafkaConnectionsConfig.Brokers)
            {
                _consumerConfig.BootstrapServers = broker.BrokerName;

                using var consumer = new ConsumerBuilder<long, string>(_consumerConfig)
                    .SetKeyDeserializer(Deserializers.Int64)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .SetLogHandler((_, message) => _logger.LogDebug($"Facility: {message.Facility}, Message: {message}"))
                    .SetErrorHandler((_, exception) => _logger.LogError($"Error: {exception.Reason}, Is Fatal: {exception.IsFatal},  Code:{exception.Code},  Is Local:{exception.IsLocalError}, Is Broker Error:{exception.IsBrokerError}"))
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
                        var result = consumer.Consume(TimeSpan.FromMilliseconds(_consumerConfig.MaxPollIntervalMs - 1000 ?? 300000));

                        // Get the message value - null is possible
                        var message = result?.Message?.Value;

                        //PersonDM personToDeserialize = JsonSerializer.Deserialize<PersonDM>(message);
                        WeatherDM personToDeserialize = JsonSerializer.Deserialize<WeatherDM>(message);

                        if (message == null)
                        {
                            continue;
                        }

                        File.WriteAllText(@"C:\Users\ofirb\source\repos\ofirbnaim\Kafka\KafkaConnections\Files\File_" + Guid.NewGuid().ToString() + ".json", message);

                        _logger.LogInformation($"Message received:\n'{message}'\nwith the key: '{result.Message.Key}', In Partition: {result.Partition.Value}, at Offset: {result.Offset.Value}\n");

                        /* The committed position, is the last offset that has been stored securely.
                        Should the process fail and restart, this is the offset that the consumer will recover to */
                        consumer.Commit(result);
                        consumer.StoreOffset(result);

                        //Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                catch (KafkaException ex)
                {
                    _logger.LogError($"Consumer Error: {ex.Message}");
                    _logger.LogError("Exiting Consumer...");
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
    }
}
