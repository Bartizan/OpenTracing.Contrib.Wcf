using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using OpenTracing.Contrib.Wcf.Configuration;
using OpenTracing.Contrib.Wcf.Handler;
using OpenTracing.Contrib.Wcf.OperationNameConstructor;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Wcf.Interceptors
{
    public class ClientTracingInterceptor : IClientMessageInspector
    {
        private readonly ClientTracingConfiguration _configuration;

        public ClientTracingInterceptor(ITracer tracer)
        {
            _configuration = new ClientTracingConfiguration(tracer);
        }

        private ClientTracingInterceptor(ClientTracingConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var context = InitializeContextWithHeaders(request, channel);

            var handler = new InterceptedClientHandler(_configuration)
                .BeforeSendRequest(ref request, context);

            request.Headers.Inject(context.Options.Headers);

            return handler;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var handler = correlationState as InterceptedClientHandler;
            handler?.AfterReceiveReply(ref reply);
        }

        private ClientInterceptorContext InitializeContextWithHeaders(Message request, IContextChannel channel)
        {
            var host = channel.RemoteAddress.Uri.ToString();
            var action = new Uri(request.Headers.Action);
            var contractName = action.Segments[action.Segments.Length - 2];
            var serviceName = contractName.Substring(0, contractName.Length - 1);
            var operationName = action.Segments[action.Segments.Length - 1];

            var method = new Method(serviceName, operationName);
            var requestHeaders = request.Headers.Extract();
            var options = new CallOptions()
                .WithHeaders(requestHeaders);
            return new ClientInterceptorContext(method, host, options);
        }

        public class Builder
        {
            private readonly ITracer _tracer;
            private IOperationNameConstructor _operationNameConstructor;
            private bool _streaming;
            private bool _verbose;
            private ISet<ClientTracingConfiguration.RequestAttribute> _tracedAttributes;

            public Builder(ITracer tracer)
            {
                _tracer = tracer;
            }

            /// <param name="operationNameConstructor">to name all spans created by this intercepter</param>
            /// <returns>this Builder with configured operation name</returns>
            public Builder WithOperationName(IOperationNameConstructor operationNameConstructor)
            {
                _operationNameConstructor = operationNameConstructor;
                return this;
            }

            /// <summary>
            /// Logs streaming events to client spans.
            /// </summary>
            /// <returns>this Builder configured to log streaming events</returns>
            public Builder WithStreaming()
            {
                _streaming = true;
                return this;
            }

            /// <summary>
            /// Logs all request life-cycle events to client spans.
            /// </summary>
            /// <returns>this Builder configured to be verbose</returns>
            public Builder WithVerbosity()
            {
                _verbose = true;
                return this;
            }

            /// <param name="tracedAttributes">to set as tags on client spans created by this interceptor</param>
            /// <returns>this Builder configured to trace attributes</returns>
            public Builder WithTracedAttributes(params ClientTracingConfiguration.RequestAttribute[] tracedAttributes)
            {
                _tracedAttributes = new HashSet<ClientTracingConfiguration.RequestAttribute>(tracedAttributes);
                return this;
            }

            public ClientTracingInterceptor Build()
            {
                var configuration = new ClientTracingConfiguration(_tracer, _operationNameConstructor, _streaming, _verbose, _tracedAttributes);
                return new ClientTracingInterceptor(configuration);
            }
        }
    }

    public class ClientTracingBehavior : IEndpointBehavior
    {
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            foreach (var messageInspector in clientRuntime.MessageInspectors)
            {
                if (messageInspector is ClientTracingInterceptor)
                {
                    return;
                }
            }

            var tracer = GlobalTracer.Instance;
            var inspector = new ClientTracingInterceptor.Builder(tracer)
                .WithTracedAttributes(
                    ClientTracingConfiguration.RequestAttribute.Headers,
                    ClientTracingConfiguration.RequestAttribute.AllCallOptions,
                    ClientTracingConfiguration.RequestAttribute.MethodName)
                .Build();
            clientRuntime.ClientMessageInspectors.Add(inspector);
        }
    }

    public class ClientTracingExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior() => new ClientTracingBehavior();

        public override Type BehaviorType => typeof(ClientTracingBehavior);
    }
}