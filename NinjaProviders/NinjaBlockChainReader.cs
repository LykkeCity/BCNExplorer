using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NinjaProviders
{
    public static class NinjaBlockChainReader
    {
        private static async Task<string> DoRequest(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
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

        public static async Task<T> DoRequest<T>(string url)
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
