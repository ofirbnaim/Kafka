using KafkaConsumer.Config;
using KafkaProducer.API;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaProducer.DM
{
    public class WeatherDM
    {
        public SunModel Results { get; set; }
        public string Status { get; set; }

        public class SunModel 
        { 
            public DateTime Sunrise { get; set; }
            public DateTime Sunset { get; set; }
        }
    }
}

