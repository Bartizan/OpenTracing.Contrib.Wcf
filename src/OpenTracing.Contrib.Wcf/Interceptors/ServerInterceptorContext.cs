namespace OpenTracing.Contrib.Wcf.Interceptors
{
    /// <summary>Context for a server-side call.</summary>
    internal sealed class ServerInterceptorContext
    {
        internal ServerInterceptorContext(
            string peer,
            string method,
            string host,
            Metadata requestHeaders)
        {
            Peer = peer;
            Method = method;
            Host = host;
            RequestHeaders = requestHeaders;
            ResponseTrailers = new Metadata();
        }

        /// <summary>
        /// Sends response headers for the current call to the client. This method may only be invoked once for each call and needs to be invoked
        /// before any response messages are written.
        /// </summary>
        /// <param name="responseHeaders">The response headers to send.</param>
        public void WriteResponseHeaders(Metadata responseHeaders)
        {
            ResponseTrailers = responseHeaders;
        }

        /// <summary>Name of method called in this RPC.</summary>
        public string Method { get; }

        /// <summary>Name of host called in this RPC.</summary>
        public string Host { get; }

        /// <summary>Address of the remote endpoint in URI format.</summary>
        public string Peer { get; }

        /// <summary>Initial metadata sent by client.</summary>
        public Metadata RequestHeaders { get; }

        /// <summary>Trailers to send back to client after RPC finishes.</summary>
        public Metadata ResponseTrailers { get; private set; }
    }
}