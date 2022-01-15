using System;
using System.Collections.Generic;
using System.Text;

namespace Spliter.Config
{
    public class KafkaConfig
    {
        public kafkaConnectionsSection kafkaConnectionsConfig { get; set; }

        // KafkaConnection Section object
        public class kafkaConnectionsSection
        {
            public Brokers[] Brokers { get; set; }  
            public string GroupName { get; set; }
        }

        public class Brokers
        {
            public string BrokerName { get; set; }
            public string[] TopicName { get; set; }
        }



    }
}
