using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OpenTracing.Contrib.Wcf.Common.Objects
{
    public enum SerializeObjectMode
    {
        Default = 1,
        RemoveAllNamespaces,
    }

    public static class XmlUtils
    {
        private static readonly XmlSerializerNamespaces EmptyNamespaces = new XmlSerializerNamespaces(
            new[]
            {
                XmlQualifiedName.Empty,
            }
        );

        public static string SerializeObject<T>(this T obj, SerializeObjectMode mode = SerializeObjectMode.Default)
        {
            if (obj == null) return null;
            var serializer = new XmlSerializer(typeof(T));

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, obj, EmptyNamespaces);
                var xmlDocument = stream.ToString();
                if (mode == SerializeObjectMode.RemoveAllNamespaces)
                {
                    var xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));
                    xmlDocument = xmlDocumentWithoutNs.ToString();
                }
                return xmlDocument;
            }
        }

        public static XElement RemoveAllNamespaces(XElement e)
        {
            return new XElement(e.Name.LocalName,
                (from n in e.Nodes()
                 select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
                (e.HasAttributes) ? (from a in e.Attributes()
                                     where (!a.IsNamespaceDeclaration)
                                     select new XAttribute(a.Name.LocalName, a.Value)) : null);
        }

        public static T DeserializeToObject<T>(this string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString)) return default(T);
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static List<T> CloneList<T>(this List<T> oldList)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, oldList);
                stream.Position = 0;
                return (List<T>)formatter.Deserialize(stream);
            }
        }
    }
}