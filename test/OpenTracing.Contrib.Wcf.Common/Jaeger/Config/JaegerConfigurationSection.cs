using System;
using System.Configuration;

namespace OpenTracing.Contrib.Wcf.Common.Jaeger.Config
{
    public sealed class JaegerConfigurationSection : ConfigurationSection
    {
        public sealed class UdpSenderElement : ConfigurationElement
        {
            private static ConfigurationPropertyCollection m_properties;

            public UdpSenderElement()
            {
                if (UdpSenderElement.m_properties == null) UdpSenderElement.m_properties = InitPropertyCollection();
            }

            private ConfigurationPropertyCollection InitPropertyCollection()
            {
                var properties = new ConfigurationPropertyCollection
                {
                    new ConfigurationProperty("host", typeof(string), "localhost", ConfigurationPropertyOptions.None),
                    new ConfigurationProperty("port", typeof(int), 6831, ConfigurationPropertyOptions.None), 
                    new ConfigurationProperty("max_packet_size", typeof(int), 65000, ConfigurationPropertyOptions.None)
                };

                return properties;
            }

            public string Host => (string)this["host"];
            public int Port => (int)this["port"];
            public int MaxPacketSize => (int)this["max_packet_size"];

            protected override ConfigurationPropertyCollection Properties => UdpSenderElement.m_properties;
        }

        public sealed class HttpSenderElement : ConfigurationElement
        {
            private static ConfigurationPropertyCollection m_properties;

            public HttpSenderElement()
            {
                if (HttpSenderElement.m_properties == null) HttpSenderElement.m_properties = InitPropertyCollection();
            }

            private ConfigurationPropertyCollection InitPropertyCollection()
            {
                var properties = new ConfigurationPropertyCollection
                {
                    new ConfigurationProperty("host", typeof(string), "localhost", ConfigurationPropertyOptions.None),
                    new ConfigurationProperty("port", typeof(int), 14268, ConfigurationPropertyOptions.None),
                    new ConfigurationProperty("max_packet_size", typeof(int), 1048576, ConfigurationPropertyOptions.None)
                };

                return properties;
            }

            public string Host => (string)this["host"];
            public int Port => (int)this["port"];
            public int MaxPacketSize => (int)this["max_packet_size"];

            protected override ConfigurationPropertyCollection Properties => HttpSenderElement.m_properties;
        }

        private static ConfigurationPropertyCollection m_properties;

        public JaegerConfigurationSection()
        {
            if (JaegerConfigurationSection.m_properties == null) JaegerConfigurationSection.m_properties = InitPropertyCollection();
        }

        private ConfigurationPropertyCollection InitPropertyCollection()
        {
            var properties = new ConfigurationPropertyCollection
            {
                new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.IsRequired),
                new ConfigurationProperty("service_name", typeof(string), "service", ConfigurationPropertyOptions.None),
                new ConfigurationProperty("use", typeof(string), "udp", ConfigurationPropertyOptions.None),
                new ConfigurationProperty("udpSender", typeof(UdpSenderElement), null, ConfigurationPropertyOptions.None),
                new ConfigurationProperty("httpSender", typeof(HttpSenderElement), null, ConfigurationPropertyOptions.None)
            };


            return properties;
        }

        public bool Enabled => (bool)this["enabled"];
        public string ServiceName => (string)this["service_name"];
        public bool UseUdp => "udp".Equals((string)this["use"], StringComparison.InvariantCultureIgnoreCase);

        public UdpSenderElement UdpSender => this["udpSender"] as UdpSenderElement;
        public HttpSenderElement HttpSender => this["httpSender"] as HttpSenderElement;

        protected override ConfigurationPropertyCollection Properties => JaegerConfigurationSection.m_properties;
    }
}