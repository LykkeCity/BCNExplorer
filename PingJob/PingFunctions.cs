using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace PingJob
{
    public class PingFunctions
    {
        private readonly AppSettings _appSettings;

        public PingFunctions(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task ProducePing([TimerTrigger("00:00:10", RunOnStartup = true)] TimerInfo timer)
        {
            var response = await GetWebjobState(_appSettings.PingUrl, _appSettings.PublishUserName, _appSettings.PublishPwd);
            Console.WriteLine($"Ping sent. Status code: {response.StatusCode}");
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
