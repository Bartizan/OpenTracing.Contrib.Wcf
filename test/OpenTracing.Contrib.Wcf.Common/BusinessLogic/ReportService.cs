using System.ServiceModel;
using OpenTracing.Contrib.Wcf.Common.Client;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Wcf.Common.BusinessLogic
{
    [ServiceContract(Namespace = "https://opentracing.io/samples", SessionMode = SessionMode.Allowed)]
    public interface IReportService
    {
        [OperationContract]
        void Touch();

        [OperationContract]
        int Silence(int depth);

        [OperationContract]
        int Noise(int depth);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    //[ServerTracingInterceptor]
    public class ReportService : IReportService
    {
        private readonly ITracer _tracer;

        public ReportService()
        {
            _tracer = GlobalTracer.Instance;
        }

        public void Touch()
        {
            using (var scope = _tracer.BuildSpan("Server.Touch").StartActive(true))
            {
                
            }
        }

        public int Silence(int depth)
        {
            if (depth > 1)
            {
                AppServerUtility.MakeRequest((IReportService proxy) => proxy.Silence(depth - 1));
            }
            return depth;
        }

        public int Noise(int depth)
        {
            if (depth > 1)
            {
                using (_tracer.BuildSpan($"Server.Noise[{depth}]").StartActive(true))
                {
                    AppServerUtility.MakeRequest((IReportService proxy) => proxy.Noise(depth - 1));
                }
            }
            return depth;
        }
    }
}