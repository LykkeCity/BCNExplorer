﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Common;
using Core.Settings;

namespace Providers.BlockChainReader
{
    public class NinjaBlockChainReader
    {
        private readonly string _ninjaBaseUrl;
        private readonly HttpReader _httpReader;

        public NinjaBlockChainReader(BaseSettings baseSettings, 
            HttpReader httpReader)
        {
            _httpReader = httpReader;
            _ninjaBaseUrl = baseSettings.NinjaUrl.RemoveLastSymbolIfExists('/');
        }

        public Task<string> GetAsync(string url)
        {
            url = url.RemoveFirstSymbolIfExists('/');
            return _httpReader.GetAsync(_ninjaBaseUrl + "/" + url);
        }

        public async Task<T> GetAsync<T>(string url)
        {
            url = url.RemoveFirstSymbolIfExists('/');
            return (await _httpReader.GetAsync<T>(_ninjaBaseUrl + "/" + url)).ParsedBody;
        }
    }
}
