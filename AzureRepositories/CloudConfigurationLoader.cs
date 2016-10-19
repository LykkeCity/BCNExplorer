using System;
using Microsoft.Azure;

namespace AzureRepositories
{
    public static class CloudConfigurationLoader
    {
        public static T ReadCloudConfiguration<T>() where T : new()
        {
            var properties = typeof(T).GetProperties();
            T result = new T();

            foreach (var property in properties)
            {
                if (property.GetSetMethod() != null)
                {
                    var type = property.PropertyType;
                    property.SetValue(result, Convert.ChangeType(CloudConfigurationManager.GetSetting(property.Name), type));
                }
            }

            return result;
        }
    }
}
