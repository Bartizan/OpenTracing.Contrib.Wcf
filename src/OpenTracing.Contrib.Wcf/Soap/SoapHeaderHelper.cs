using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using OpenTracing.Contrib.Wcf.Utils;

namespace OpenTracing.Contrib.Wcf
{
    internal class SoapHeaderHelper
    {
        private static string GetNamespace(Type type)
        {
            var result = default(string);
            var daAttrib = ReflectionUtils.GetDataContractAttribute(type);
            if (daAttrib != null)
            {
                result = daAttrib.Namespace;
            }
            if (result == null)
            {
                var clrNs = type.Namespace;
                result = GetGlobalDataContractNamespace(clrNs, type.Module);
                if (result == null)
                    result = GetGlobalDataContractNamespace(clrNs, type.Assembly);
            }
            if (result != null)
                return result;
            return string.Empty;
        }

        private static string GetGlobalDataContractNamespace(string clrNs, ICustomAttributeProvider customAttributeProvider)
        {
            var customAttributes = customAttributeProvider.GetCustomAttributes(typeof(ContractNamespaceAttribute), false);
            for (var i = 0; i < customAttributes.Length; i++)
            {
                var attribute = (ContractNamespaceAttribute)customAttributes[i];
                string clrNamespace = attribute.ClrNamespace;
                if (clrNamespace == null)
                {
                    clrNamespace = string.Empty;
                }
                if (clrNamespace == clrNs)
                {
                    return attribute.ContractNamespace;
                }
            }
            return null;
        }

        private readonly Type _type;
        private readonly string _headerNamespace;

        public SoapHeaderHelper(Type type)
        {
            _type = type;
            _headerNamespace = GetNamespace(_type);
        }

        public object GetInputHeader(MessageHeaders headers, string name)
        {
            return GetHeader(headers, name);
        }

        internal object GetHeader(MessageHeaders headers, string name)
        {
            var index = headers.FindHeader(name, _headerNamespace);
            if (index >= 0)
            {
                var serializer = new DataContractSerializer(_type, name, _headerNamespace, null, 
                    int.MaxValue, false, false, null);
                return headers.GetHeader<object>(index, serializer);
            }
            return null;
        }

        public IEnumerable<KeyValuePair<string, T>> GetInputHeaders<T>(MessageHeaders headers)
        {
            var list = new List<KeyValuePair<string, T>>();
            var index = -1;
            foreach (var header in headers)
            {
                index++;
                if (!_headerNamespace.Equals(header.Namespace, StringComparison.InvariantCultureIgnoreCase)) continue;
                var serializer = new DataContractSerializer(typeof(T), header.Name, _headerNamespace, null,
                    int.MaxValue, false, false, null);
                var obj = headers.GetHeader<T>(index, serializer);
                list.Add(new KeyValuePair<string, T>(header.Name, obj));
            }
            return list;
        }

        public void SetOutputHeader(MessageHeaders headers, string name, object value)
        {
            AddHeader(headers, name, value);
        }

        internal void AddHeader(MessageHeaders headers, string name, object value)
        {
            var header = MessageHeader.CreateHeader(name, _headerNamespace, value);
            headers.Add(header);
        }
    }
}