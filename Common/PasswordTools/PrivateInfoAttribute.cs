using System;
using System.Reflection;

namespace Common.PasswordTools
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrivateInfoAttribute : Attribute { }

    public static class PrivateInfoExt
    {
        public static object HidePrivateInfo<T>(this object record) where T : new()
        {
            var copy = new T();

            foreach (PropertyInfo propertyInfo in record.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    propertyInfo.SetValue(copy,
                        Attribute.IsDefined(propertyInfo, typeof (PrivateInfoAttribute))
                            ? GetHiddenValue(propertyInfo.GetValue(record).GetType())
                            : propertyInfo.GetValue(record));
                }
            }
            return copy;
        }

        private static object GetHiddenValue(Type t)
        {
            if (t == typeof(string))
                return "***";

            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
