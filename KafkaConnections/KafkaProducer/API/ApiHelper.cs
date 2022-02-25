using KafkaProducer.Config;
using KafkaProducer.DM;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace KafkaProducer.API
{
    public class ApiHelper : IApiHelper
    {
        private readonly ConfigDM _configDM;
        public static HttpClient _httpClient { get; set; }

        public ApiHelper(ConfigDM configDM) 
        {
            _configDM = configDM;
            _httpClient = new HttpClient();
            // Gets the API address from the config
            _httpClient.BaseAddress = new Uri(_configDM.ApiSectionConfig.BaseAddress);
            // Clear the header
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            // Give me header in json format
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Meaning of 'where T : class'
        // Limit that 'T' has to be a class and it can't be a int/string/double or any type wich is not a class
        public async Task<T> LoadFromWeb<T>() where T : class
        {
            using (HttpResponseMessage reaspons = await _httpClient.GetAsync(_httpClient.BaseAddress))
            {
                if (reaspons.IsSuccessStatusCode)
                {
                    // Deserialize the object of T from the reaspons
                    T t = await reaspons.Content.ReadAsAsync<T>();
                    return t;
                }
                else
                {
                    throw new Exception(reaspons.ReasonPhrase);
                }
            }
        }
    }
}
