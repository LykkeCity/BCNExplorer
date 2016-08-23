﻿using System.Text;
using AzureStorage.Blob;
using Common;

namespace AzureRepositories
{
    public class GeneralSettingsReader
    {
        public static T ReadGeneralSettings<T>(string connectionString)
        {
            var settingsStorage = new AzureBlobStorage(connectionString);
            var settingsData = settingsStorage.GetAsync("settings", "generalsettings.json").Result.ToBytes();
            var str = Encoding.UTF8.GetString(settingsData);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }
    }
}
