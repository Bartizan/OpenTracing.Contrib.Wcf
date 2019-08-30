using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Wcf.Common.BusinessLogic
{
    [ServiceContract(Namespace = "https://opentracing.io/samples", SessionMode = SessionMode.Allowed)]
    public interface IThrowsService
    {
        [OperationContract]
        string ThrowsCLRException();

        [OperationContract]
        void ThrowsCLRExceptionOneWay();

        [OperationContract]
        string ThrowsFaultException();

        [OperationContract]
        void ThrowsFaultExceptionOneWay();

        [OperationContract]
        string ThrowsTypedCLRFaultException();

        [OperationContract]
        string ThrowsTypedCustomFaultException();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class ThrowsService : IThrowsService
    {
        private readonly ITracer _tracer;

        public ThrowsService()
        {
            _tracer = GlobalTracer.Instance;
        }

        [DataContract]
        public class MyCustomException
        {
            [DataMember]
            public string MyMessage { get; set; }
        }

        public string ThrowsCLRException()
        {
            // Throw a CLR exception
            throw new IndexOutOfRangeException("ThrowsCLRException: IndexOutOfRangeException");
        }

        public void ThrowsCLRExceptionOneWay()
        {
            throw new IndexOutOfRangeException("ThrowsCLRExceptionOneWay: IndexOutOfRangeException");
        }

        public string ThrowsFaultException()
        {
            // Throw a fault exception
            throw new FaultException("ThrowsFaultException: FaultException", new FaultCode("CalculationError"));
        }

        public void ThrowsFaultExceptionOneWay()
        {
            throw new FaultException("ThrowsFaultExceptionOneWay: FaultException");
        }

        public string ThrowsTypedCLRFaultException()
        {
            // Throw a typed FaultException with a CLR exception
            throw new FaultException<IndexOutOfRangeException>(new IndexOutOfRangeException(),
                new FaultReason("ThrowsTypedCLRFaultException: FaultException<IndexOutOfRangeException>"));
        }

        public string ThrowsTypedCustomFaultException()
        {
            // Throw a typed FaultException with a custom exception
            var fault = new MyCustomException
            {
                MyMessage = "ThrowsTypedCustomFaultException: FaultException<MyCustomException>"
            };
            throw new FaultException<MyCustomException>(fault, new FaultReason("No reason!"));
        }
    }
}