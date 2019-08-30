using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using OpenTracing.Contrib.Wcf.Common.Client;
using OpenTracing.Contrib.Wcf.Common.Objects;
using OpenTracing.Contrib.Wcf.Common.Shared;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Wcf.Common.BusinessLogic
{
    [ServiceContract(Namespace = "https://opentracing.io/samples", SessionMode = SessionMode.Allowed)]
    public interface IExportService
    {
        [OperationContract]
        string AddTask(int reportId);

        [OperationContract]
        object GetInfo(Guid uid);

        [OperationContract]
        object GetFile(string token);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class ExportService : IExportService
    {
        private readonly ITracer _tracer;
        private readonly ConcurrentDictionary<string, string> _rmq;
        private readonly ConcurrentDictionary<string, object> _db;

        public ExportService()
        {
            _tracer = GlobalTracer.Instance;
            _rmq = RmqTaskDispatcher.Messages;
            _db = Db.Files;
        }

        public string AddTask(int reportId)
        {
            using (var scope = _tracer.BuildSpan("Server.AddTask").StartActive(true))
            {
                var traceId = scope.Span.GetBaggageItem(Common.Objects.Constants.TRACE_ID);
                if (string.IsNullOrEmpty(traceId)) throw new ApplicationException("Not found 'traceId'");

                var token = $"{traceId}-{reportId}";

                var ctx = new SpanContext(scope.Span.Context);
                if (!_rmq.TryAdd(token, ctx.SerializeObject()))
                {
                    throw new ApplicationException("Failed to add 'task'");
                }

                Task.Factory.StartNew(() =>
                {
                    if (!_rmq.TryGetValue(token, out var context))
                        throw new ApplicationException("Not found 'parentSpanCtx'");
                    var parentSpanCtx = context.DeserializeToObject<SpanContext>();

                    using (var inner = _tracer.BuildSpan("Server.TaskProcess").AsChildOf(parentSpanCtx).StartActive(true))
                    {
                        var innerTraceId = inner.Span.GetBaggageItem(Common.Objects.Constants.TRACE_ID);
                        if (string.IsNullOrEmpty(innerTraceId)) throw new ApplicationException("Not found 'innerTraceId'");

                        foreach (var id in Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()))
                        {
                            AppServerUtility.MakeRequest((IExportService proxy) => proxy.GetInfo(id));
                        }

                        foreach (var uid in Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()))
                        {
                            AppServerUtility.MakeRequest((IImageService proxy) => proxy.GetImage(uid));
                        }

                        using (_tracer.BuildSpan("Server.SomeLongActions").StartActive(true))
                        {
                            Thread.Sleep(2500);
                        }

                        if (!_db.TryAdd(token, $"{token}.docx"))
                        {
                            throw new ApplicationException("Failed to add 'file'");
                        }
                    }
                });

                using (_tracer.BuildSpan("Server.SomeActions").StartActive(true))
                {
                    Thread.Sleep(500);
                }
                
                return token;
            }
        }

        public object GetFile(string token)
        {
            _db.TryGetValue(token, out var file);
            return file;
        }

        public object GetInfo(Guid uid)
        {
            var traceId = _tracer.ActiveSpan.GetBaggageItem(Common.Objects.Constants.TRACE_ID);
            if (string.IsNullOrEmpty(traceId)) throw new ApplicationException("Not found 'traceId'");

            Thread.Sleep(50);
            return new object();
        }
    }
}