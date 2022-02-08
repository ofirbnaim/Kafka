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
        private readonly ILogger<ApiHelper> _logger;
        private readonly ConfigDM _configDM;


        public ApiHelper(ILogger<ApiHelper> logger, ConfigDM config)
        {
            _logger = logger;
            _configDM = config;
        }



        public static HttpClient httpClient { get; set; }
        public void InitializeHttpClient()
        {
            httpClient = new HttpClient();
            // Gets the API address from the config
            httpClient.BaseAddress = new Uri(_configDM.ApiSectionConfig.BaseAddress);
            // Clear the header
            httpClient.DefaultRequestHeaders.Accept.Clear();
            // Give me header in json format
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        // Meaning of where T : class
        // class - limit that 'T' has to be a class and it can't be a int/string/double or any type wich is not a class

        public async Task<T> LoadFromWeb<T>() where T : class
        {
            using (HttpResponseMessage reaspons = await httpClient.GetAsync(httpClient.BaseAddress))
            {
                if (reaspons.IsSuccessStatusCode)
                {
                    // Deserialize the object of T from the reaspons
                    T t = await reaspons.Content.ReadAsAsync<T>();

                    return t;
                }
                else
                {
                    _logger.LogError($"There was a problem with the API reaspons: ", reaspons.ReasonPhrase);
                    throw new Exception(reaspons.ReasonPhrase);
                }
            }
        }
    }
}
