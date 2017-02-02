using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Common.Validation
{
    public static class ValidationHelper
    {
        public static void ValidateObjectRecursive<T>(T obj)
        {
            Validator.ValidateObject(obj, new ValidationContext(obj, null, null));

            var properties = obj.GetType().GetProperties().Where(prop => prop.CanRead
                && !prop.GetIndexParameters().Any()).ToList();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string)
                    || property.PropertyType == typeof(decimal)
                    || property.PropertyType.IsValueType 
                    || property.PropertyType.IsPrimitive 
                    || property.PropertyType.IsEnum
                    || property.PropertyType.IsGenericType
                    || property.PropertyType.IsArray)
                    continue;

                var value = property.GetValue(obj);

                if (value == null)
                    continue;

                ValidateObjectRecursive(value);
            }
        }
    }
}

