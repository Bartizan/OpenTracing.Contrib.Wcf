using System.Collections.Concurrent;

namespace OpenTracing.Contrib.Wcf.Common.BusinessLogic
{
    public class Db
    {
        public static readonly ConcurrentDictionary<string, object> Files = new ConcurrentDictionary<string, object>();
    }
}