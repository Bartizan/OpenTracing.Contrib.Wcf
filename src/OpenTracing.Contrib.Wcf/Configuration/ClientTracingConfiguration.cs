using System.Collections.Generic;
using OpenTracing.Contrib.Wcf.OperationNameConstructor;

namespace OpenTracing.Contrib.Wcf.Configuration
{
    public class ClientTracingConfiguration : TracingConfiguration
    {
        public enum RequestAttribute
        {
            MethodName,
            AllCallOptions,
            Headers
        }

        public ISet<RequestAttribute> TracedAttributes { get; }

        internal ClientTracingConfiguration(ITracer tracer) : base(tracer)
        {
            TracedAttributes = new HashSet<RequestAttribute>();
        }

        internal ClientTracingConfiguration(ITracer tracer, IOperationNameConstructor operationNameConstructor, bool streaming, bool verbose, ISet<RequestAttribute> tracedAttributes)
            : base(tracer, operationNameConstructor, streaming, verbose)
        {
            TracedAttributes = tracedAttributes ?? new HashSet<RequestAttribute>();
        }
    }
}