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

        public static T ReadGeneralSettingsLocal<T>()
        {
            var settingsStorage = new AzureBlobStorage("UseDevelopmentStorage=true");
            var settingsData = settingsStorage.GetAsync("settings", "bcnexplolersettings.json").Result.ToBytes();
            var str = Encoding.UTF8.GetString(settingsData);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }
    }
}
