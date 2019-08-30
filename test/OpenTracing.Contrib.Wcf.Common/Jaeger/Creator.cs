using System;
using System.Configuration;
using System.Reflection;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using OpenTracing.Contrib.Wcf.Common.Jaeger.Config;
using OpenTracing.Noop;

namespace OpenTracing.Contrib.Wcf.Common.Jaeger
{
    public sealed class Creator
    {
        private const string SectionName = "jaeger";

        private static readonly Lazy<ITracer> LoggerLazy = new Lazy<ITracer>(CreateLogger);

        public static ITracer GetLogger() => LoggerLazy.Value;

        internal static ITracer CreateLogger()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var config = ConfigurationManager.OpenExeConfiguration(assembly.Location);
            var cfg = config.GetSection(SectionName) as JaegerConfigurationSection;
            if (cfg == null || !cfg.Enabled) return NoopTracerFactory.Create();

            var sender = default(ThriftSender);
            if (cfg.UseUdp)
            {
                var udp = cfg.UdpSender;
                sender = new UdpSender(udp.Host, udp.Port, udp.MaxPacketSize);
            }
            else
            {
                var http = cfg.HttpSender;
                sender = new HttpSender.Builder($"http://{http.Host}:{http.Port}/api/traces")
                    .WithMaxPacketSize(http.MaxPacketSize)
                    .Build();
            }
            var sampler = new ConstSampler(true);

            var reporter = new RemoteReporter.Builder()
                .WithSender(sender)
                .Build();

            return new Tracer.Builder(cfg.ServiceName)
                .WithReporter(reporter)
                .WithSampler(sampler)
                .Build();
        }
    }
}