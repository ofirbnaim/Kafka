using KafkaProducer.DM;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KafkaProducer.API
{
    public interface IApiHelper
    {
        // Meaning of where T : class
        // class - limit that 'T' has to be a class and it can't be a int/string/double or any type wich is not a class
        public Task<T> LoadFromWeb<T>() where T : class;
    }
}