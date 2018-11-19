using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Unidark
{
    public static class HashHelper
    {
        // This should probably go into a file
        static readonly Dictionary<string, int> _knownHashes = new Dictionary<string, int>
        {
            {"D22AE3BCE358EFDBC4CF850853C07C4E", 586848}, // 5.6.3p1
            {"A524B969A64304C8E663DBAA24224FD6", 20467872} // 2018.2.16f1
        };

        public static bool IsKnownFile(FileStream stream, out int offset)
        {
            var hash = GetHashString(stream);

            return _knownHashes.TryGetValue(hash, out offset);
        }
        
        static string GetHashString(FileStream fs)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
                hash = md5.ComputeHash(fs);

            return hash.Aggregate(new StringBuilder(32), (sb, b) => sb.Append(b.ToString("X2"))).ToString();
        }
    }
}
