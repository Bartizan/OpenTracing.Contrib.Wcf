using System;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Wcf.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var tracer = Common.Jaeger.Creator.GetLogger();
            GlobalTracer.Register(tracer);

            var service = new MiddleLayerService();
            service.OnStart();
            try
            {
                Console.WriteLine("Server started.");

                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey();
            }
            finally
            {
                service.OnStop();
            }
        }
    }
}
