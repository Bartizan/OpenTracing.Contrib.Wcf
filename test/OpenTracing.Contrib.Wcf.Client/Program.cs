using System;
using System.ServiceModel;
using System.Threading;
using OpenTracing.Contrib.Wcf.Common.BusinessLogic;
using OpenTracing.Contrib.Wcf.Common.Client;
using OpenTracing.Tag;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Wcf.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var tracer = Common.Jaeger.Creator.GetLogger();
            GlobalTracer.Register(tracer);

            Console.WriteLine("Client started.");
            Console.WriteLine("Press any key to start requests . . .");
            Console.ReadKey();
            Console.WriteLine("Client sending requests.");

            if (true)
            {
                AppServerUtility.MakeRequest((IReportService proxy) => proxy.Touch());
                using (tracer.BuildSpan("Client.Touch").StartActive(true))
                {
                    AppServerUtility.MakeRequest((IReportService proxy) => proxy.Touch());
                }

                AppServerUtility.MakeRequest((IReportService proxy) => proxy.Silence(7));
                using (tracer.BuildSpan("Client.Noise").StartActive(true))
                {
                    AppServerUtility.MakeRequest((IReportService proxy) => proxy.Noise(11));
                }
            }

            if (true)
            {
                using (tracer.BuildSpan("ThrowsCLRException").StartActive(true))
                {
                    try
                    {
                        AppServerUtility.MakeRequest((IThrowsService proxy) =>
                            proxy.ThrowsCLRException());
                    }
                    catch (FaultException)
                    {
                        /* ignore */
                    }
                }

                using (tracer.BuildSpan("ThrowsCLRExceptionOneWay").StartActive(true))
                {
                    try
                    {
                        AppServerUtility.MakeRequest((IThrowsService proxy) =>
                            proxy.ThrowsCLRExceptionOneWay());
                    }
                    catch (FaultException)
                    {
                        /* ignore */
                    }
                }

                using (tracer.BuildSpan("ThrowsFaultException").StartActive(true))
                {
                    try
                    {
                        AppServerUtility.MakeRequest((IThrowsService proxy) =>
                            proxy.ThrowsFaultException());
                    }
                    catch (FaultException)
                    {
                        /* ignore */
                    }
                }

                using (tracer.BuildSpan("ThrowsFaultExceptionOneWay").StartActive(true))
                {
                    try
                    {
                        AppServerUtility.MakeRequest((IThrowsService proxy) =>
                            proxy.ThrowsFaultExceptionOneWay());
                    }
                    catch (FaultException)
                    {
                        /* ignore */
                    }
                }

                using (tracer.BuildSpan("ThrowsTypedCLRFaultException").StartActive(true))
                {
                    try
                    {
                        AppServerUtility.MakeRequest((IThrowsService proxy) =>
                            proxy.ThrowsTypedCLRFaultException());
                    }
                    catch (FaultException)
                    {
                        /* ignore */
                    }
                }

                using (tracer.BuildSpan("ThrowsTypedCustomFaultException").StartActive(true))
                {
                    try
                    {
                        AppServerUtility.MakeRequest((IThrowsService proxy) =>
                            proxy.ThrowsTypedCustomFaultException());
                    }
                    catch (FaultException)
                    {
                        /* ignore */
                    }
                }
            }

            if (true)
            {
                var traceId = Guid.NewGuid().ToString("N");
                const int reportId = 2209619;

                var scope = tracer.BuildSpan($"Client.Export(reportId={reportId})").StartActive(true);
                scope.Span.SetBaggageItem(Common.Objects.Constants.TRACE_ID, traceId);
                
                var token = default(string);
                using (var inner = tracer.BuildSpan("Client.AddTask").AsChildOf(scope.Span.Context).StartActive(true))
                {
                    token = AppServerUtility.MakeRequest((IExportService proxy) => proxy.AddTask(reportId));

                    if (!$"{traceId}-{reportId}".Equals(token))
                    {
                        inner.Span.Log("Not supported 'token'");
                        Tags.Error.Set(inner.Span, true);

                        throw new ApplicationException("Not supported 'token'");
                    }
                }

                using (var inner = tracer.BuildSpan("Client.WaitFile").AsChildOf(scope.Span.Context).StartActive(true))
                {
                    var file = default(object);
                    do
                    {
                        Thread.Sleep(1000);
                        file = AppServerUtility.MakeRequest((IExportService proxy) => proxy.GetFile(token));
                    } while (file == null);

                    if (!$"{file}".Equals($"{token}.docx"))
                    {
                        inner.Span.Log("Not supported 'file'");
                        Tags.Error.Set(inner.Span, true);

                        throw new ApplicationException("Not supported 'file'");
                    }
                }

                scope.Span.Finish();
            }

            Console.WriteLine("Press any key to continue . . .");
            Console.ReadKey();
        }
    }
}
