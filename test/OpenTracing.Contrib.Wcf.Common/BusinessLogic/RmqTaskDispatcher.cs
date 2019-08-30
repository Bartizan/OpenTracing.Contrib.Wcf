using System.Collections.Concurrent;

namespace OpenTracing.Contrib.Wcf.Common.BusinessLogic
{
    public class RmqTaskDispatcher
    {
        public static readonly ConcurrentDictionary<string, string> Messages = new ConcurrentDictionary<string, string>();
    }
}