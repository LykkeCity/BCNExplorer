﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Providers.BlockChainReader
{
    public class HttpReader
    {
        public async Task<string> GetAsync(string absUrl)
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(absUrl);
                webRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; //ignore sertificate
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
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<HttpReadResult<T>> GetAsync<T>(string absUrl)
        {
            T parsedBody;
            try
            {
                var result = await GetAsync(absUrl);
                parsedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception e)
            {
                parsedBody = default(T);
            }

            return new HttpReadResult<T>
            {
                ParsedBody = parsedBody,
                Url = absUrl
            };
        }

        public class HttpReadResult<T>
        {
            public string Url { get; set; }
            public T ParsedBody { get; set; }
        }
    }
}
