namespace OpenTracing.Contrib.Wcf
{
    /// <summary>Options for calls made by client.</summary>
    internal sealed class CallOptions
    {
        /// <summary>
        /// Creates a new instance of <c>CallOptions</c>.
        /// </summary>
        /// <param name="headers">Headers to be sent with the call.</param>
        public CallOptions(
            Metadata headers = null)
        {
            Headers = headers;
        }

        /// <summary>Headers to send at the beginning of the call.</summary>
        public Metadata Headers {get; private set; }

        public CallOptions WithHeaders(Metadata headers)
        {
            Headers = headers;
            return this;
        }
    }
}