using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.Wcf
{
    internal static class Extensions
    {
        public static Exception ReadFaultDetail(ref Message reply)
        {
            const int maxBufferSize = int.MaxValue;
            var buffer = reply.CreateBufferedCopy(maxBufferSize);
            try
            {
                var message = buffer.CreateMessage();
                var fault = MessageFault.CreateFault(message, maxBufferSize);
                return new FaultException(fault);
            }
            finally
            {
                reply = buffer.CreateMessage();
                buffer.Close();
            }
        }

        public static ISpan SetException(this ISpan span, Exception ex)
        {
            return span?.SetTag(Tags.Error, true)
                .Log(new Dictionary<string, object>
                {
                    {LogFields.Event, Tags.Error.Key},
                    {LogFields.ErrorObject, ex},

                    // Those fields will be removed once Configration.WithExpandExceptionLogs is implemented
                    {LogFields.ErrorKind, ex.GetType().Name},
                    {LogFields.Message, ex.Message},
                    {LogFields.Stack, ex.StackTrace}
                });
        }

        public static string ToReadableString(this Metadata metadata)
        {
            if (metadata.Count == 0)
                return null;

            return string.Join(";", metadata.Select(e => $"{e.Key} = {e.Value}"));
        }

        public static string ToReadableString(this CallOptions options)
        {
            var headers = options.Headers.ToReadableString() ?? "Empty";

            return $"Headers: {headers}";
        }

        private static readonly SoapHeaderHelper m_soapHeaderHelper = new SoapHeaderHelper(typeof(Metadata));

        public static void Inject(this MessageHeaders headers, Metadata metadata)
        {
            foreach (var e in metadata)
            {
                m_soapHeaderHelper.AddHeader(headers, e.Key, e.Value);
            }
        }

        public static Metadata Extract(this MessageHeaders headers)
        {
            var metadata = new Metadata();
            foreach (var h in m_soapHeaderHelper.GetInputHeaders<string>(headers))
            {
                metadata.Add(h.Key, h.Value);
            }
            return metadata;
        }
    }
}