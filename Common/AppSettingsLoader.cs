using System;
using System.IO;

namespace Common
{
    public static class AppSettingsLoader
    {
        public static T ReadAppSettins<T>(string path = null, string fileName = "settings.json")
        {
            if (path == null)
                path = AppDomain.CurrentDomain.BaseDirectory;

            path = path.AddLastSymbolIfNotExists('\\');

            try
            {
                var json = File.ReadAllText(path + fileName);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading settings.json file: " + ex.Message);
                throw;
            }
        }
    }
}
