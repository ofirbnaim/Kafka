using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Confluent.Kafka;
using Spliter.Config;

namespace Spliter.Logic
{
    public class KafkaConnections : IKafkaConnections
    {

        public KafkaConfig _kafkaConfig;

        public KafkaConnections(KafkaConfig kafkaConfig)
        {
            _kafkaConfig = kafkaConfig;
        }

        public void ToConsume()
        {
            throw new NotImplementedException();
        }

        public void ToProduce()
        {
            //Producer configuration
            var producerConfig = new ProducerConfig
            {   
                //BootstrapServers = _kafkaConfig.kafkaConnectionsSection.KafkaBrokerAddress,
                BootstrapServers = "localhost:9092",
                ClientId = Dns.GetHostName(),
                
            };
            
            //The message value
            using (var producerBuilder = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                
                producerBuilder.Produce("ofir-topic", new Message<Null, string> 
                { 
                    Value = "ofir-test" 
                });   
            }
           
        }
    }
}
