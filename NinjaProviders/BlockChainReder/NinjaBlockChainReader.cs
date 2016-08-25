using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Core.Settings;

namespace NinjaProviders.BlockChainReder
{
    public class NinjaBlockChainReader
    {
        private readonly string _ninjaBaseUrl;

        public NinjaBlockChainReader(BaseSettings baseSettings)
        {
            _ninjaBaseUrl = baseSettings.NinjaUrl;
        }

        private async Task<string> DoRequest(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(_ninjaBaseUrl + url);
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var webResponse = await webRequest.GetResponseAsync();
            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    return await sr.ReadToEndAsync();
                }

            }
        }

        public async Task<T> DoRequest<T>(string url)
        {
            try
            {
                var result = await DoRequest(url);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
