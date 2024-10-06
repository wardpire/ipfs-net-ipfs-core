using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Ipfs.Cryptography
{
    /// <summary>
    /// Digest bytes are expected to be UTF-8 string bytes
    /// </summary>
    class Simhash : HashAlgorithm
    {
        SimhashLib.Simhash simhash;

        public Simhash(int bitcount = 64)
        {
            simhash = new SimhashLib.Simhash(bitcount);
        }

        public override void Initialize()
        {
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var origString = Encoding.UTF8.GetString(array, ibStart, cbSize);
            simhash.GenerateSimhash(origString);
        }

        protected override byte[] HashFinal()
        {
            return simhash.Value;
        }
    }
}
