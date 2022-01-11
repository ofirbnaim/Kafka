using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Spliter.Config;

namespace Spliter.Logic
{
    public class KafkaConnections : IKafkaConnections
    {
        private readonly ILogger<KafkaConnections> _logger;
        public KafkaConfig _kafkaConfig;

        public KafkaConnections(ILogger<KafkaConnections> logger, KafkaConfig kafkaConfig)
        {
            _logger = logger;
            _kafkaConfig = kafkaConfig;
        }

        public void ToConsume()
        {
            throw new NotImplementedException();
        }

        public void ToProduce()
        {
            //var producerConfig = new ProducerConfig
            //{
            //    BootstrapServers = "localhost:9092",
            //    ClientId = Dns.GetHostName(),

            //};

            var producerConfig = new ProducerConfig();


            //Take producer values from configuration
            //for (int i = 0; i < _kafkaConfig.kafkaConnectionsSectionConfig.Brokers.Length; i++)   
            //{
            //    Brokers tempBroker = new Brokers();
            //    tempBroker.BrokerName = _kafkaConfig.kafkaConnectionsSectionConfig.Brokers[i].BrokerName;
            //    tempBroker.TopicName = _kafkaConfig.kafkaConnectionsSectionConfig.Brokers[i].TopicName;

            //    producerConfig.BootstrapServers = tempBroker.BrokerName;
            //    producerConfig.ClientId = Dns.GetHostName();

            //    //The message value
            //    using (var producerBuilder = new ProducerBuilder<Null, string>(producerConfig).Build())
            //    {
            //        try
            //        {
            //            for (int j = 0; j < tempBroker.TopicName.Length; j++)
            //            {
            //                producerBuilder.ProduceAsync(tempBroker.TopicName[j], new Message<Null, string>
            //                {
            //                    Value = "ofir-test"
            //                });

            //                _logger.LogInformation($"Success to write the meesege to topic: {tempBroker.TopicName[j]}");
            //            }

            //            //producerBuilder.Flush();
            //        }
            //        catch (Exception ex)
            //        {
            //            _logger.LogInformation(ex, "There was a problem to produce a message");
            //        }
            //    }
           
            //}
        }
    }
}
