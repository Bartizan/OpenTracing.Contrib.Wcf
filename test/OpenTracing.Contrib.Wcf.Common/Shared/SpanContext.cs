using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenTracing.Contrib.Wcf.Common.Shared
{
    [Serializable]
    [XmlRoot("spanContext")]
    public class SpanContext : ISpanContext
    {
        [Serializable]
        public class BaggageItem
        {
            public static BaggageItem[] ToConvert(IEnumerable<KeyValuePair<string, string>> list)
            {
                var d = new List<BaggageItem>();
                foreach (var it in list) d.Add(new BaggageItem(it));
                return d.ToArray();
            }

            public static ICollection<KeyValuePair<string, string>> ToConvert(BaggageItem[] array)
            {
                var d = new List<KeyValuePair<string, string>>(array?.Length ?? 0);
                if (array != null)
                {
                    foreach (var it in array) d.Add(new KeyValuePair<string, string>(it.Key, it.Value));
                }
                return d;
            }

            public BaggageItem() { }

            public BaggageItem(KeyValuePair<string, string> it)
            {
                Key = it.Key;
                Value = it.Value;
            }

            [XmlAttribute("key")]
            public string Key { get; set; }

            [XmlText]
            public string Value { get; set; }
        }

        public SpanContext() {}

        public SpanContext(ISpanContext ctx)
        {
            TraceId = ctx.TraceId;
            SpanId = ctx.SpanId;
            BaggageItems = BaggageItem.ToConvert(ctx.GetBaggageItems());
        }

        [XmlAttribute("traceId")]
        public string TraceId { get; set; }

        [XmlAttribute("spanId")]
        public string SpanId { get; set; }

        [XmlArray("baggageItems"), XmlArrayItem("item")]
        public BaggageItem[] BaggageItems { get; set; }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems() => BaggageItem.ToConvert(BaggageItems);
    }
}