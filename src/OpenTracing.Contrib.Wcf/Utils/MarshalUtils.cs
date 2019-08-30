using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenTracing.Contrib.Wcf.Utils
{
    /// <summary>Useful methods for native/managed marshalling.</summary>
    internal static class MarshalUtils
    {
        private static readonly Encoding EncodingUTF8 = Encoding.UTF8;
        private static readonly Encoding EncodingASCII = Encoding.ASCII;

        /// <summary>
        /// Converts <c>IntPtr</c> pointing to a UTF-8 encoded byte array to <c>string</c>.
        /// </summary>
        public static string PtrToStringUTF8(IntPtr ptr, int len)
        {
            byte[] numArray = new byte[len];
            Marshal.Copy(ptr, numArray, 0, len);
            return MarshalUtils.EncodingUTF8.GetString(numArray);
        }

        /// <summary>
        /// Returns byte array containing UTF-8 encoding of given string.
        /// </summary>
        public static byte[] GetBytesUTF8(string str)
        {
            return MarshalUtils.EncodingUTF8.GetBytes(str);
        }

        /// <summary>Get string from a UTF8 encoded byte array.</summary>
        public static string GetStringUTF8(byte[] bytes)
        {
            return MarshalUtils.EncodingUTF8.GetString(bytes);
        }

        /// <summary>
        /// Returns byte array containing ASCII encoding of given string.
        /// </summary>
        public static byte[] GetBytesASCII(string str)
        {
            return MarshalUtils.EncodingASCII.GetBytes(str);
        }

        /// <summary>Get string from an ASCII encoded byte array.</summary>
        public static string GetStringASCII(byte[] bytes)
        {
            return MarshalUtils.EncodingASCII.GetString(bytes);
        }
    }
}