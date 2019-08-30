using System.ServiceModel.Channels;
using OpenTracing.Contrib.Wcf.Configuration;
using OpenTracing.Contrib.Wcf.Interceptors;
using OpenTracing.Contrib.Wcf.Propagation;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.Wcf.Handler
{
    internal sealed class InterceptedServerHandler
    {
        private readonly ServerTracingConfiguration _configuration;
        private ServerInterceptorContext _context;
        private WcfTraceLogger _logger;

        public InterceptedServerHandler(ServerTracingConfiguration configuration)
        {
            _configuration = configuration;
        }

        public InterceptedServerHandler AfterReceiveRequest(ref Message request, ServerInterceptorContext context)
        {
            _context = context;
            var span = GetSpanFromContext();
            _logger = new WcfTraceLogger(span, _configuration);
            _logger.Request(ref request);
            return this;
        }

        public InterceptedServerHandler BeforeSendReply(ref Message reply)
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

        private ISpan GetSpanFromContext()
        {
            var spanBuilder = GetSpanBuilderFromHeaders()
                .WithTag(Constants.TAGS_PEER_ADDRESS, _context.Peer)
                .WithTag(Tags.Component, Constants.TAGS_COMPONENT)
                .WithTag(Tags.SpanKind, Tags.SpanKindServer);

            foreach (var attribute in _configuration.TracedAttributes)
            {
                switch (attribute)
                {
                    case ServerTracingConfiguration.RequestAttribute.MethodName:
                        spanBuilder.WithTag(Constants.TAGS_WCF_METHOD_NAME, _context.Method);
                        break;
                    case ServerTracingConfiguration.RequestAttribute.Headers:
                        spanBuilder.WithTag(Constants.TAGS_WCF_HEADERS, _context.RequestHeaders?.ToReadableString());
                        break;
                }
            }
            return spanBuilder.StartActive(false).Span;
        }

        private ISpanBuilder GetSpanBuilderFromHeaders()
        {
            var operationName = _configuration.OperationNameConstructor.ConstructOperationName(_context.Method);
            var spanBuilder = _configuration.Tracer.BuildSpan(operationName);

            var parentSpanCtx = _configuration.Tracer.Extract(BuiltinFormats.HttpHeaders, new MetadataCarrier(_context.RequestHeaders));
            if (parentSpanCtx != null)
            {
                spanBuilder = spanBuilder.AsChildOf(parentSpanCtx);
            }
            return spanBuilder;
        }
    }
}
 