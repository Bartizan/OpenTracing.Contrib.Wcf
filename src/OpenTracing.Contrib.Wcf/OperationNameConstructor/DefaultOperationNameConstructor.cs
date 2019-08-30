namespace OpenTracing.Contrib.Wcf.OperationNameConstructor
{
    internal sealed class DefaultOperationNameConstructor : IOperationNameConstructor
    {
        public string ConstructOperationName(Method method)
        {
            return method.FullName;
        }

        public string ConstructOperationName(string method)
        {
            return method;
        }
    }
}