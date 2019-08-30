namespace OpenTracing.Contrib.Wcf
{
    /// <summary>A non-generic representation of a remote method.</summary>
    internal interface IMethod
    {
        /// <summary>
        /// Gets the name of the service to which this method belongs.
        /// </summary>
        string ServiceName { get; }

        /// <summary>Gets the unqualified name of the method.</summary>
        string Name { get; }

        /// <summary>
        /// Gets the fully qualified name of the method. On the server side, methods are dispatched
        /// based on this name.
        /// </summary>
        string FullName { get; }
    }
}