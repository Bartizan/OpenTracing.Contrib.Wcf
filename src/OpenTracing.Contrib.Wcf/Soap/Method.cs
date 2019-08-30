using OpenTracing.Contrib.Wcf.Utils;

namespace OpenTracing.Contrib.Wcf
{
    /// <summary>A description of a remote method.</summary>
    public sealed class Method : IMethod
    {
        /// <summary>
        /// Gets full name of the method including the service name.
        /// </summary>
        internal static string GetFullName(string serviceName, string methodName) => $"/{serviceName}/{methodName}";

        public Method(
            string serviceName,
            string name)
        {
            ServiceName = WcfPreconditions.CheckNotNull(serviceName, nameof(serviceName));
            Name = WcfPreconditions.CheckNotNull(name, nameof(name));
            FullName = GetFullName(serviceName, name);
        }

        /// <summary>
        /// Gets the name of the service to which this method belongs.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>Gets the unqualified name of the method.</summary>
        public string Name { get; }

        /// <summary>
        /// Gets the fully qualified name of the method. On the server side, methods are dispatched
        /// based on this name.
        /// </summary>
        public string FullName { get; }
    }
}