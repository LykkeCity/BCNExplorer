using System;
using System.Configuration;
using System.Text;
using AzureStorage.Blob;
using Common;

namespace AzureRepositories
{
    public class GeneralSettingsReader
    {
        public static T ReadGeneralSettings<T>(string connectionString)
        {
            try
            {
                var settingsStorage = new AzureBlobStorage(connectionString);
                var settingsData = settingsStorage.GetAsync("settings", "bcnexplolersettings.json").Result.ToBytes();
                var str = Encoding.UTF8.GetString(settingsData);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception)
            {
                throw new ArgumentException("Failed to get config file using given connection string", 
                    nameof(connectionString));
            }
        }
    }
}
