using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTracing.Contrib.Wcf.Utils;

namespace OpenTracing.Contrib.Wcf
{
    internal sealed class Metadata : IList<Metadata.Entry>
    {
        /// <summary>
        /// An read-only instance of metadata containing no entries.
        /// </summary>
        public static readonly Metadata Empty = new Metadata().Freeze();
        /// <summary>All binary headers should have this suffix.</summary>
        public const string BinaryHeaderSuffix = "-bin";

        private readonly List<Entry> _entries;
        private bool _readOnly;

        public Metadata()
        {
            _entries = new List<Entry>();
        }

        internal Metadata Freeze()
        {
            _readOnly = true;
            return this;
        }

        public int IndexOf(Entry item)
        {
            return _entries.IndexOf(item);
        }

        public void Insert(int index, Entry item)
        {
            WcfPreconditions.CheckNotNull(item, nameof(item));
            CheckWriteable();
            _entries.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            CheckWriteable();
            _entries.RemoveAt(index);
        }

        public Entry this[int index]
        {
            get => _entries[index];
            set
            {
                WcfPreconditions.CheckNotNull(value);
                CheckWriteable();
                _entries[index] = value;
            }
        }

        public void Add(Entry item)
        {
            WcfPreconditions.CheckNotNull(item, nameof(item));
            CheckWriteable();
            _entries.Add(item);
        }

        public void Add(string key, string value)
        {
            Add(new Entry(key, value));
        }

        public void Add(string key, byte[] valueBytes)
        {
            Add(new Entry(key, valueBytes));
        }

        public void Clear()
        {
            CheckWriteable();
            _entries.Clear();
        }

        public bool Contains(Entry item)
        {
            return _entries.Contains(item);
        }

        public void CopyTo(Entry[] array, int arrayIndex)
        {
            _entries.CopyTo(array, arrayIndex);
        }

        public int Count => _entries.Count;

        public bool IsReadOnly => _readOnly;

        public bool Remove(Entry item)
        {
            CheckWriteable();
            return _entries.Remove(item);
        }

        public IEnumerator<Entry> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();

        private void CheckWriteable()
        {
            WcfPreconditions.CheckState(!_readOnly, "Object is read only");
        }

        public class Entry
        {
            private static readonly Regex ValidKeyRegex = new Regex("^[a-z0-9_-]+$");

            /// <summary>
            /// Returns <c>true</c> if the key has "-bin" binary header suffix.
            /// </summary>
            private static bool HasBinaryHeaderSuffix(string key)
            {
                var length = key.Length;
                return length >= 4 && key[length - 4] == '-' && (key[length - 3] == 'b' && key[length - 2] == 'i') && key[length - 1] == 'n';
            }

            private static string NormalizeKey(string key)
            {
                var lowerInvariant = WcfPreconditions.CheckNotNull(key, nameof(key)).ToLowerInvariant();
                WcfPreconditions.CheckArgument(ValidKeyRegex.IsMatch(lowerInvariant), "Metadata entry key not valid. Keys can only contain lowercase alphanumeric characters, underscores and hyphens.");
                return lowerInvariant;
            }

            private readonly string _key;
            private readonly string _value;
            private readonly byte[] _valueBytes;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:OpenTracing.Contrib.Wcf.Metadata.Entry" /> struct with a binary value.
            /// </summary>
            /// <param name="key">Metadata key, needs to have suffix indicating a binary valued metadata entry.</param>
            /// <param name="valueBytes">Value bytes.</param>
            public Entry(string key, byte[] valueBytes)
            {
                _key = NormalizeKey(key);
                WcfPreconditions.CheckArgument(Entry.HasBinaryHeaderSuffix(this._key), "Key for binary valued metadata entry needs to have suffix indicating binary value.");
                _value = default(string);
                WcfPreconditions.CheckNotNull(valueBytes, nameof(valueBytes));
                _valueBytes = new byte[valueBytes.Length];
                Buffer.BlockCopy(valueBytes, 0, _valueBytes, 0, valueBytes.Length);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Grpc.Core.Metadata.Entry" /> struct holding an ASCII value.
            /// </summary>
            /// <param name="key">Metadata key, must not use suffix indicating a binary valued metadata entry.</param>
            /// <param name="value">Value string. Only ASCII characters are allowed.</param>
            public Entry(string key, string value)
            {
                _key = NormalizeKey(key);
                WcfPreconditions.CheckArgument(!HasBinaryHeaderSuffix(this._key), "Key for ASCII valued metadata entry cannot have suffix indicating binary value.");
                _value = WcfPreconditions.CheckNotNull<string>(value, nameof(value));
                _valueBytes = default(byte[]);
            }

            /// <summary>Gets the metadata entry key.</summary>
            public string Key => this._key;

            /// <summary>Gets the binary value of this metadata entry.</summary>
            public byte[] ValueBytes
            {
                get
                {
                    if (_valueBytes == null)
                        return MarshalUtils.GetBytesASCII(_value);
                    var numArray = new byte[_valueBytes.Length];
                    Buffer.BlockCopy(_valueBytes, 0, numArray, 0, _valueBytes.Length);
                    return numArray;
                }
            }

            /// <summary>Gets the string value of this metadata entry.</summary>
            public string Value
            {
                get
                {
                    WcfPreconditions.CheckState(!this.IsBinary, "Cannot access string value of a binary metadata entry");
                    return _value ?? MarshalUtils.GetStringASCII(_valueBytes);
                }
            }

            /// <summary>
            /// Returns <c>true</c> if this entry is a binary-value entry.
            /// </summary>
            public bool IsBinary => _value == null;

            /// <summary>
            /// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Grpc.Core.Metadata.Entry" />.
            /// </summary>
            public override string ToString()
            {
                if (IsBinary)
                    return $"[Entry: key={_key}, valueBytes={_valueBytes}]";
                return $"[Entry: key={_key}, value={_value}]";
            }
        }
    }
}