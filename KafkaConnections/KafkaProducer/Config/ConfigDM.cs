using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaProducer.Config
{
    public class ConfigDM
    {
        public kafkaConnectionsSection KafkaConnectionsConfig { get; set; }
        public ApiSection ApiSectionConfig { get; set; }

        // Kafka Connection Section
        public class kafkaConnectionsSection
        {
            public Brokers[] Brokers { get; set; }
            public string GroupID { get; set; }
        }

        public class Brokers
        {
            public string BrokerName { get; set; }
            public IEnumerable<string> TopicsNames { get; set; }
        }
        
        // Api Section Config
        public class ApiSection
        {
            public string BaseAddress { get; set; }
        }
    }
}
