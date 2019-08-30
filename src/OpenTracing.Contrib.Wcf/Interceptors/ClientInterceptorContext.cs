namespace OpenTracing.Contrib.Wcf.Interceptors
{
    /// <summary>
    /// Carries along the context associated with intercepted invocations on the client side.
    /// </summary>
    internal sealed class ClientInterceptorContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="ClientInterceptorContext" />
        /// with the specified method, host, and call options.
        /// </summary>
        /// <param name="method">A <see cref="Method" /> object representing the method to be invoked.</param>
        /// <param name="host">The host to dispatch the current call to.</param>
        /// <param name="options">A <see cref="CallOptions" /> instance containing the call options of the current call.</param>
        public ClientInterceptorContext(
            Method method,
            string host,
            CallOptions options)
        {
            Method = method;
            Host = host;
            Options = options;
        }

        public Method Method { get; set; }

        /// <summary>
        /// Gets the host that the current invocation will be dispatched to.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the <see cref="CallOptions" /> structure representing the
        /// call options associated with the current invocation.
        /// </summary>
        public CallOptions Options { get; }
    }
}