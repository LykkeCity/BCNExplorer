using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Core.Settings;

namespace Providers.BlockChainReader
{
    public class LykkeBlockChainReader
    {
        private readonly string _ninjaBaseUrl;
        private readonly HttpReader _httpReader;

        public LykkeBlockChainReader(BaseSettings baseSettings, HttpReader httpReader1)
        {
            _httpReader = httpReader1;
            _ninjaBaseUrl = baseSettings.NinjaUrl;
        }

        public Task<string> Get(string url)
        {
            return _httpReader.Get(_ninjaBaseUrl + url);
        }

        public Task<T> Get<T>(string url)
        {
            return _httpReader.Get<T>(_ninjaBaseUrl + url);
        }
    }
}
