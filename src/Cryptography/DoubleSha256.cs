using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Ipfs.Cryptography
{
    internal class DoubleSha256 : HashAlgorithm
    {
        private readonly HashAlgorithm digest = SHA256.Create();
        private byte[] roundComputedHash;

        public override void Initialize()
        {
            digest.Initialize();
            roundComputedHash = null;
        }

        public override int HashSize => digest.HashSize;

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (roundComputedHash != null)
                throw new NotSupportedException("Already called.");

            roundComputedHash = digest.ComputeHash(array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            digest.Initialize();
            return digest.ComputeHash(roundComputedHash);
        }
    }
}