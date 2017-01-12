using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.Azure.WebJobs;

namespace PingJob
{
    public class PingFunctions
    {
        private readonly AppSettings _appSettings;
        private readonly ILog _log;

        public PingFunctions(AppSettings appSettings, ILog log)
        {
            _log = log;
            _appSettings = appSettings;
        }

        public async Task ProducePing([TimerTrigger("00:00:10", RunOnStartup = true)] TimerInfo timer)
        {
            var response = await GetWebjobState(_appSettings.PingUrl, _appSettings.PublishUserName, _appSettings.PublishPwd);
            Console.WriteLine($"Ping sent. Status code: {response.StatusCode}");
        }

        public async Task UpdateMainChainIndexer([TimerTrigger("00:01:00", RunOnStartup = true)] TimerInfo timer)
        {
            HttpClient client = new HttpClient();
            var resp = await client.GetAsync(_appSettings.UpdateMainChainIndexerUrl);

            await _log.WriteInfo("PingFunctions", "UpdateMainChainIndexer", null, $"done.Status code {resp.StatusCode}");
        }

        private static Task<HttpResponseMessage> GetWebjobState(string webjobUrl, string userName, string userPwd)
        {
            HttpClient client = new HttpClient();
            string auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ':' + userPwd));
            client.DefaultRequestHeaders.Add("authorization", auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client.GetAsync(webjobUrl);
        }
    }
}
