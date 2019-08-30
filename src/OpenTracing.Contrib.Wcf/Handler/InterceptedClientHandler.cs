using System.ServiceModel.Channels;
using OpenTracing.Contrib.Wcf.Configuration;
using OpenTracing.Contrib.Wcf.Interceptors;
using OpenTracing.Contrib.Wcf.Propagation;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.Wcf.Handler
{
    internal sealed class InterceptedClientHandler
    {
        private readonly ClientTracingConfiguration _configuration;
        private ClientInterceptorContext _context;
        private WcfTraceLogger _logger;

        public InterceptedClientHandler(ClientTracingConfiguration configuration)
        {
            _configuration = configuration;
        }

        public InterceptedClientHandler BeforeSendRequest(ref Message request, ClientInterceptorContext context)
        {
            _context = context;
            var span = InitializeSpanWithHeaders();

            _logger = new WcfTraceLogger(span, _configuration);
            _configuration.Tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new MetadataCarrier(_context.Options.Headers));
            _logger.Request(ref request);

            return this;
        }

        public InterceptedClientHandler AfterReceiveReply(ref Message reply)
        {
            if (!reply.IsFault)
            {
                _logger.Response(ref reply);
                _logger.FinishSuccess();
            }
            else
            {
                var ex = Extensions.ReadFaultDetail(ref reply);
                _logger.FinishException(ex);
            }
            return this;
        }

        private ISpan InitializeSpanWithHeaders()
        {
            var operationName = _configuration.OperationNameConstructor.ConstructOperationName(_context.Method);
            var spanBuilder = _configuration.Tracer.BuildSpan(operationName)
                .WithTag(Constants.TAGS_PEER_ADDRESS, _context.Host)
                .WithTag(Tags.Component, Constants.TAGS_COMPONENT)
                .WithTag(Tags.SpanKind, Tags.SpanKindClient);

            foreach (var attribute in _configuration.TracedAttributes)
            {
                switch (attribute)
                {
                    case ClientTracingConfiguration.RequestAttribute.MethodName:
                        spanBuilder.WithTag(Constants.TAGS_WCF_METHOD_NAME, _context.Method?.FullName);
                        break;
                    case ClientTracingConfiguration.RequestAttribute.AllCallOptions:
                        spanBuilder.WithTag(Constants.TAGS_WCF_CALL_OPTIONS, _context.Options.ToReadableString());
                        break;
                    case ClientTracingConfiguration.RequestAttribute.Headers:
                        spanBuilder.WithTag(Constants.TAGS_WCF_HEADERS, _context.Options.Headers?.ToReadableString());
                        break;
                }
            }

            return spanBuilder.Start();
        }
    }
}