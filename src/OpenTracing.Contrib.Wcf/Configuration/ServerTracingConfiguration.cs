using System.Collections.Generic;
using OpenTracing.Contrib.Wcf.OperationNameConstructor;

namespace OpenTracing.Contrib.Wcf.Configuration
{
    public sealed class ServerTracingConfiguration : TracingConfiguration
    {
        public enum RequestAttribute
        {
            Headers,
            MethodName,
        }

        public ISet<RequestAttribute> TracedAttributes { get; }

        internal ServerTracingConfiguration(ITracer tracer) : base(tracer)
        {
            TracedAttributes = new HashSet<RequestAttribute>();
        }

        internal ServerTracingConfiguration(ITracer tracer, IOperationNameConstructor operationNameConstructor, bool streaming, bool verbose, ISet<RequestAttribute> tracedAttributes) 
            : base(tracer, operationNameConstructor, streaming, verbose)
        {
            TracedAttributes = tracedAttributes ?? new HashSet<RequestAttribute>();
        }
    }
}