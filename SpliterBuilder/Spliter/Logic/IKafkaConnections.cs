using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spliter.Logic
{
    public interface IKafkaConnections
    {
        public Task ToProduce();
        public void ToConsume();

    }
}
