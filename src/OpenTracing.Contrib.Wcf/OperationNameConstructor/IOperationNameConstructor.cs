namespace OpenTracing.Contrib.Wcf.OperationNameConstructor
{
    public interface IOperationNameConstructor
    {
        string ConstructOperationName(Method method);
        string ConstructOperationName(string method);
    }
}