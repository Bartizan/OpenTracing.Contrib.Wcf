using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
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
    public class ServerTracingInterceptor : IDispatchMessageInspector
    {
        private readonly ServerTracingConfiguration _configuration;

        public ServerTracingInterceptor(ITracer tracer)
        {
            _configuration = new ServerTracingConfiguration(tracer);
        }

        private ServerTracingInterceptor(ServerTracingConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var context = InitializeContextWithHeaders(request, instanceContext);

            return new InterceptedServerHandler(_configuration)
                .AfterReceiveRequest(ref request, context);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var handler = correlationState as InterceptedServerHandler;
            handler?.BeforeSendReply(ref reply);
        }

        private static ServerInterceptorContext InitializeContextWithHeaders(Message request, InstanceContext instanceContext)
        {
            var peer = (request.Properties[RemoteEndpointMessageProperty.Name] is RemoteEndpointMessageProperty remoteEndpoint) ? 
                $"{remoteEndpoint.Address}:{remoteEndpoint.Port}" : "<unknown>";

            var host = Dns.GetHostName();
            var instanceName = instanceContext.GetServiceInstance().GetType().FullName;
            var operationName = request.Headers.Action.Substring(request.Headers.Action.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
            var method = $"{instanceName}.{operationName}";

            var requestHeaders = request.Headers.Extract();
            return new ServerInterceptorContext(peer, method, host, requestHeaders);
        }

        public class Builder
        {
            private readonly ITracer _tracer;
            private IOperationNameConstructor _operationNameConstructor;
            private bool _streaming;
            private bool _verbose;
            private ISet<ServerTracingConfiguration.RequestAttribute> _tracedAttributes;

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

            /// <param name="tracedAttributes">to set as tags on client spans created by this intercepter</param>
            /// <returns>this Builder configured to trace attributes</returns>
            public Builder WithTracedAttributes(params ServerTracingConfiguration.RequestAttribute[] tracedAttributes)
            {
                _tracedAttributes = new HashSet<ServerTracingConfiguration.RequestAttribute>(tracedAttributes);
                return this;
            }

            public ServerTracingInterceptor Build()
            {
                var configuration = new ServerTracingConfiguration(_tracer, _operationNameConstructor, _streaming, _verbose, _tracedAttributes);
                return new ServerTracingInterceptor(configuration);
            }
        }
    }

    public class ServerTracingBehavior : IServiceBehavior
    {
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var tracer = GlobalTracer.Instance;
            var inspector = new ServerTracingInterceptor.Builder(tracer)
                .WithTracedAttributes(
                    ServerTracingConfiguration.RequestAttribute.Headers,
                    ServerTracingConfiguration.RequestAttribute.MethodName)
                .Build();
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpointDispatcher in dispatcher.Endpoints)
                {
                    var added = false;
                    var dispatchRuntime = endpointDispatcher.DispatchRuntime;
                    foreach (var messageInspector in dispatchRuntime.MessageInspectors)
                    {
                        if (messageInspector is ServerTracingInterceptor)
                        {
                            added = true;
                            break;
                        }
                    }
                    if (!added) dispatchRuntime.MessageInspectors.Add(inspector);
                }
            }
        }
    }

    public class ServerTracingExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior() => new ServerTracingBehavior();

        public override Type BehaviorType => typeof(ServerTracingBehavior);
    }

    public class ServerTracingInterceptorAttribute : Attribute, IServiceBehavior
    {
        private readonly IServiceBehavior _behavior;

        public ServerTracingInterceptorAttribute()
        {
            _behavior = new ServerTracingBehavior();
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) =>
            _behavior.Validate(serviceDescription, serviceHostBase);

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters) =>
            _behavior.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters);

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) =>
            _behavior.ApplyDispatchBehavior(serviceDescription, serviceHostBase);
    }
}