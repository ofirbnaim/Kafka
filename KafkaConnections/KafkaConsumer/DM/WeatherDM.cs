using System;

namespace KafkaConsumer.DM
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

