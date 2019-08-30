using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using OpenTracing.Contrib.Wcf.Configuration;

namespace OpenTracing.Contrib.Wcf
{
    internal sealed class WcfTraceLogger
    {
        private readonly ISpan _span;
        private readonly TracingConfiguration _configuration;

        public WcfTraceLogger(ISpan span, TracingConfiguration configuration)
        {
            _span = span;
            _configuration = configuration;

            if (_configuration.Verbose)
            {
                _span.Log("Started call");
            }
        }

        public void Request(ref Message req)
        {
            if (!(_configuration.Streaming || _configuration.Verbose)) return;

            const int maxBufferSize = int.MaxValue;
            var buffer = req.CreateBufferedCopy(maxBufferSize);
            try
            {
                var data = buffer.CreateMessage().ToString();
                _span.Log(new Dictionary<string, object>
                {
                    { LogFields.Event, "WCF request" },
                    { "data", data }
                });
            }
            finally
            {
                req = buffer.CreateMessage();
                buffer.Close();
            }
        }

        public void Response(ref Message rsp)
        {
            if (!(_configuration.Streaming || _configuration.Verbose)) return;

            const int maxBufferSize = int.MaxValue;
            var buffer = rsp.CreateBufferedCopy(maxBufferSize);
            try
            {
                var data = buffer.CreateMessage().ToString();
                _span.Log(new Dictionary<string, object>
                {
                    { LogFields.Event, "WCF response" },
                    { "data", data }
                });
            }
            finally
            {
                rsp = buffer.CreateMessage();
                buffer.Close();
            }
        }

        public void FinishSuccess()
        {
            if (_configuration.Verbose)
            {
                _span.Log("Call completed");
            }
            _span.Finish();
        }

        public void FinishException(Exception ex)
        {
            if (_configuration.Verbose)
            {
                _span.Log("Call failed");
            }
            _span.SetException(ex)
                .Finish();
        }
    }
}