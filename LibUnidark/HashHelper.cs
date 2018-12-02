using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.IO;

namespace LibUnidark
{
    public static class HashHelper
    {
        // This should probably go into a file
        private static readonly Dictionary<string, int> _knownHashes = new Dictionary<string, int>
        {
            {"BE83B24D", 586848}, // 5.6.3p1
            {"37BB3023", 20461904} // 2018.2.17f1
        };

        public static bool IsKnownFile(Stream stream, out int offset)
        {
            try
            {
                var hash = GetHashString(stream);

                return _knownHashes.TryGetValue(hash, out offset);
            }
            catch (FileNotFoundException)
            {
                // Cannot find the xxHash DLL, might as well pretend the hash is unknown to stop crashes
                offset = -1;
                return false;
            }
        }
        
        static string GetHashString(Stream stream)
        {
            var xxHash = xxHashFactory.Instance.Create();
            var hash = xxHash.ComputeHash(stream);

            return hash.AsHexString(true);
        }
    }
}
