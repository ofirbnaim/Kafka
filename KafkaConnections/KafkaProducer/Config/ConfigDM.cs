using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaProducer.Config
{
    public class ConfigDM
    {
        public kafkaConnectionsSection kafkaConnectionsConfig { get; set; }

        // KafkaConnection Section object
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
    }
}
