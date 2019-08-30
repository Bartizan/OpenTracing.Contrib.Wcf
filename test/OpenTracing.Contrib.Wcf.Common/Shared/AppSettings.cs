using System;
using System.Configuration;

namespace OpenTracing.Contrib.Wcf.Common.Shared
{
    public static class AppSettings
    {
        public static T Get<T>(string name, Func<T> defaultIfNull = null)
        {
            string value = ConfigurationManager.AppSettings[name];
            if (value == null)
            {
                if (defaultIfNull == null)
                {
                    return default(T);
                }

                return defaultIfNull();
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception($"Can't parse appsetting by name '{name}' with value '{value}' as {typeof(T)}", ex);
            }
        }

        public static T Get<T>(string name, T defaultValue) => Get(name, () => defaultValue);

    }
}