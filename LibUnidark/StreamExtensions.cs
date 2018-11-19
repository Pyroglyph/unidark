using System;
using System.IO;
using System.Linq;

namespace LibUnidark
{
    public static class StreamExtensions
    {
        // Slightly modified version of this: https://stackoverflow.com/a/1472689
        public static int GetPositionOf(this Stream stream, byte[] byteSequence)
        {
            if (byteSequence.Length > stream.Length)
                return -1;

            var buffer = new byte[byteSequence.Length];

            var bufStream = new BufferedStream(stream, byteSequence.Length);
            
            while (bufStream.Read(buffer, 0, byteSequence.Length) == byteSequence.Length)
            {
                if (byteSequence.SequenceEqual(buffer))
                    return (int) bufStream.Position - byteSequence.Length;

                bufStream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
            }
            
            return -1;
        }

        static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            var i = 1;
            while (i < bytes.Length)
            {
                var n = bytes.Length - i;
                var aux1 = new byte[n];
                var aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                    return i;
                i++;
            }
            return i;
        }
    }
}
