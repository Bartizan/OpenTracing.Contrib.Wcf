using System;
using System.ServiceModel;
using System.Threading;

namespace OpenTracing.Contrib.Wcf.Common.BusinessLogic
{
    [ServiceContract(Namespace = "https://opentracing.io/samples", SessionMode = SessionMode.Allowed)]
    public interface IImageService
    {
        [OperationContract]
        object GetImage(Guid uid);
    }

    // legacy service
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class ImageService : IImageService
    {
        public object GetImage(Guid uid)
        {
            Thread.Sleep(50);
            return new object();
        }
    }
}