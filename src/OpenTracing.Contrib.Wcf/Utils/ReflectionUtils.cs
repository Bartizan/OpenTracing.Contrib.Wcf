using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenTracing.Contrib.Wcf.Utils
{
    internal static class ReflectionUtils
    {
        internal static DataContractAttribute GetDataContractAttribute(Type type)
        {
            return GetAttribute<DataContractAttribute>(type);
        }

        private static T GetAttribute<T>(MemberInfo memberInfo) where T : class
        {
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), false);
            if (customAttributes.Length > 0)
                return (T)customAttributes[0];
            return null;
        }
    }
}