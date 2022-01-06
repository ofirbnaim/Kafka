using System;
using System.Collections.Generic;
using System.Text;

namespace Spliter.Config
{
    public class KafkaConfig
    {
        public KafkaConnectionsSection kafkaConnectionsSection { get; set; }
        
        // KafkaConnection Section object
        public class KafkaConnectionsSection
        {
            public string KafkaBrokerAddress { get; set; }
            public string TopicName { get; set; }
            public string GroupName { get; set; }
        }


    }
}
