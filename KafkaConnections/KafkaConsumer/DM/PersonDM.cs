using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaConsumer.DM
{
    public class PersonDM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
