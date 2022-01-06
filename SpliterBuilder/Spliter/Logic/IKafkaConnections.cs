using System;
using System.Collections.Generic;
using System.Text;

namespace Spliter.Logic
{
    public interface IKafkaConnections
    {
        public void ToProduce();
        public void ToConsume();

    }
}
