using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KafkaProducer.Logic
{
    public interface IProducerClass
    {
        public Task ToProduce();
    }
}
