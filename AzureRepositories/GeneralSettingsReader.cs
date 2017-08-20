using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using AzureStorage.Blob;
using Common;

namespace AzureRepositories
{
    public class GeneralSettingsReader
    {
        public static T ReadGeneralSettingsViaHttp<T>(string url)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            var settingsData = httpClient.GetStringAsync("").Result;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(settingsData);
        }

        public static T ReadGeneralSettingsLocal<T>(string path)
        {
            var content = File.ReadAllText(path);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
        }
    }
}
