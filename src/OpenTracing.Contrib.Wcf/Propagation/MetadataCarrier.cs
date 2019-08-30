using System.Collections;
using System.Collections.Generic;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.Wcf.Propagation
{
    internal sealed class MetadataCarrier : ITextMap
    {
        private readonly Metadata _metadata;

        public MetadataCarrier(Metadata metadata)
        {
            _metadata = metadata;
        }

        public void Set(string key, string value)
        {
            _metadata.Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var entry in _metadata)
            {
                if (entry.IsBinary)
                    continue;

                yield return new KeyValuePair<string, string>(entry.Key, entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}