using System;
using System.Collections.Generic;
using Ipfs.Cryptography;
using System.Security.Cryptography;
using BC = Org.BouncyCastle.Crypto.Digests;
using SimhashLib;

namespace Ipfs.Registry
{
    /// <summary>
    ///   Metadata and implemetations of a IPFS hashing algorithms.
    /// </summary>
    /// <remarks>
    ///   IPFS assigns a unique <see cref="Name"/> and <see cref="Code"/> to a hashing algorithm.
    ///   See <see href="https://github.com/multiformats/multicodec/blob/master/table.csv">hashtable.csv</see>
    ///   for the currently defined hashing algorithms.
    ///   <para>
    ///   These algorithms are implemented:
    ///   <list type="bullet">
    ///   <item><description>blake2b-160, blake2b-256 blake2b-384 and blake2b-512</description></item>
    ///   <item><description>blake2s-128, blake2s-160, blake2s-224 a nd blake2s-256</description></item>
    ///   <item><description>keccak-224, keccak-256, keccak-384 and keccak-512</description></item>
    ///   <item><description>md4 and md5</description></item>
    ///   <item><description>sha1</description></item>
    ///   <item><description>sha2-256, sha2-512 and dbl-sha2-256</description></item>
    ///   <item><description>sha3-224, sha3-256, sha3-384 and sha3-512</description></item>
    ///   <item><description>shake-128 and shake-256</description></item>
    ///   </list>
    ///   </para>
    ///   <para>
    ///   The <c>identity</c> hash is also implemented;  which just returns the input bytes.
    ///   This is used to inline a small amount of data into a <see cref="Cid"/>.
    ///   </para>
    ///   <para>
    ///     Use <see cref="Register(string, int, int, Func{HashAlgorithm})"/> to add a new
    ///     hashing algorithm.
    ///   </para>
    /// </remarks>
    public sealed class HashingAlgorithm
    {
        internal static Dictionary<AlgorithmNames, HashingAlgorithm> Names = new();
        internal static Dictionary<int, HashingAlgorithm> Codes = new();

        /// <summary>
        ///   Register the standard hash algorithms for IPFS.
        /// </summary>
        /// <seealso href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        static HashingAlgorithm()
        {
            Register(AlgorithmNames.sha1, 0x11, 20, () => SHA1.Create());
            Register(AlgorithmNames.sha2_256, 0x12, 32, () => SHA256.Create());
            Register(AlgorithmNames.sha2_512, 0x13, 64, () => SHA512.Create());
            Register(AlgorithmNames.dbl_sha2_256, 0x56, 32, () => new DoubleSha256());
            Register(AlgorithmNames.keccak_224, 0x1A, 224 / 8, () => new KeccakManaged(224));
            Register(AlgorithmNames.keccak_256, 0x1B, 256 / 8, () => new KeccakManaged(256));
            Register(AlgorithmNames.keccak_384, 0x1C, 384 / 8, () => new KeccakManaged(384));
            Register(AlgorithmNames.keccak_512, 0x1D, 512 / 8, () => new KeccakManaged(512));
            Register(AlgorithmNames.sha3_224, 0x17, 224 / 8, () => new BouncyDigest(new BC.Sha3Digest(224)));
            Register(AlgorithmNames.sha3_256, 0x16, 256 / 8, () => new BouncyDigest(new BC.Sha3Digest(256)));
            Register(AlgorithmNames.sha3_384, 0x15, 384 / 8, () => new BouncyDigest(new BC.Sha3Digest(384)));
            Register(AlgorithmNames.sha3_512, 0x14, 512 / 8, () => new BouncyDigest(new BC.Sha3Digest(512)));
            Register(AlgorithmNames.shake_128, 0x18, 128 / 8, () => new BouncyDigest(new BC.ShakeDigest(128)));
            Register(AlgorithmNames.shake_256, 0x19, 256 / 8, () => new BouncyDigest(new BC.ShakeDigest(256)));
            Register(AlgorithmNames.blake2b_160, 0xb214, 160 / 8, () => new BouncyDigest(new BC.Blake2bDigest(160)));
            Register(AlgorithmNames.blake2b_256, 0xb220, 256 / 8, () => new BouncyDigest(new BC.Blake2bDigest(256)));
            Register(AlgorithmNames.blake2b_384, 0xb230, 384 / 8, () => new BouncyDigest(new BC.Blake2bDigest(384)));
            Register(AlgorithmNames.blake2b_512, 0xb240, 512 / 8, () => new BouncyDigest(new BC.Blake2bDigest(512)));
            Register(AlgorithmNames.blake2s_128, 0xb250, 128 / 8, () => new BouncyDigest(new BC.Blake2sDigest(128)));
            Register(AlgorithmNames.blake2s_160, 0xb254, 160 / 8, () => new BouncyDigest(new BC.Blake2sDigest(160)));
            Register(AlgorithmNames.blake2s_224, 0xb25c, 224 / 8, () => new BouncyDigest(new BC.Blake2sDigest(224)));
            Register(AlgorithmNames.blake2s_256, 0xb260, 256 / 8, () => new BouncyDigest(new BC.Blake2sDigest(256)));
            Register(AlgorithmNames.md4, 0xd4, 128 / 8, () => new BouncyDigest(new BC.MD4Digest()));
            Register(AlgorithmNames.md5, 0xd5, 128 / 8, () => MD5.Create());
            Register(AlgorithmNames.identity, 0, 0, () => new IdentityHash());
            Register(AlgorithmNames.simhash, 0x20, 256, () => new Cryptography.Simhash(256));
        }

        /// <summary>
        ///   The name of the algorithm.
        /// </summary>
        /// <value>
        ///   A unique name.
        /// </value>
        public AlgorithmNames Name { get; private set; }

        /// <summary>
        ///   The IPFS number assigned to the hashing algorithm.
        /// </summary>
        /// <value>
        ///   Valid hash codes at <see href="https://github.com/multiformats/multicodec/blob/master/table.csv">hashtable.csv</see>.
        /// </value>
        public int Code { get; private set; }

        /// <summary>
        ///   The size, in bytes, of the digest value.
        /// </summary>
        /// <value>
        ///   The digest value size in bytes. Zero indicates that the digest
        ///   is non fixed.
        /// </value>
        public int DigestSize { get; private set; }

        /// <summary>
        ///   Returns a cryptographic hash algorithm that can compute
        ///   a hash (digest).
        /// </summary>
        public Func<HashAlgorithm> Hasher { get; private set; }

        /// <summary>
        ///   Use <see cref="Register"/> to create a new instance of a <see cref="HashingAlgorithm"/>.
        /// </summary>
        private HashingAlgorithm()
        {
        }

        /// <summary>
        ///   The <see cref="Name"/> of the hashing algorithm.
        /// </summary>
        public override string ToString()
        {
            return Enum.GetName(Name)??string.Empty;
        }

        /// <summary>
        ///   Register a new IPFS hashing algorithm.
        /// </summary>
        /// <param name="name">
        ///   The name of the algorithm.
        /// </param>
        /// <param name="code">
        ///   The IPFS number assigned to the hashing algorithm.
        /// </param>
        /// <param name="digestSize">
        ///   The size, in bytes, of the digest value.
        /// </param>
        /// <param name="hasher">
        ///   A <c>Func</c> that returns a <see cref="HashAlgorithm"/>.  If not specified, then a <c>Func</c> is created to
        ///   throw a <see cref="NotImplementedException"/>.
        /// </param>
        /// <returns>
        ///   A new <see cref="HashingAlgorithm"/>.
        /// </returns>
        public static HashingAlgorithm Register(AlgorithmNames name, int code, int digestSize, Func<HashAlgorithm> hasher = null)
        {
            if (Names.ContainsKey(name))
                throw new ArgumentException(string.Format("The IPFS hashing algorithm '{0}' is already defined.", name));
            if (Codes.ContainsKey(code))
                throw new ArgumentException(string.Format("The IPFS hashing algorithm code 0x{0:x2} is already defined.", code));

            hasher ??= () => throw new NotImplementedException(string.Format("The IPFS hashing algorithm '{0}' is not implemented.", name));

            var a = new HashingAlgorithm
            {
                Name = name,
                Code = code,
                DigestSize = digestSize,
                Hasher = hasher
            };
            Names[name] = a;
            Codes[code] = a;

            return a;
        }

        /// <summary>
        ///   Remove an IPFS hashing algorithm from the registry.
        /// </summary>
        /// <param name="algorithm">
        ///   The <see cref="HashingAlgorithm"/> to remove.
        /// </param>
        public static void Deregister(HashingAlgorithm algorithm)
        {
            Names.Remove(algorithm.Name);
            Codes.Remove(algorithm.Code);
        }

        /// <summary>
        ///   A sequence consisting of all <see cref="HashingAlgorithm">hashing algorithms</see>.
        /// </summary>
        /// <value>
        ///   The currently registered hashing algorithms.
        /// </value>
        public static IEnumerable<HashingAlgorithm> All
        {
            get { return Names.Values; }
        }

        /// <summary>
        ///   Gets the <see cref="HashAlgorithm"/> with the specified IPFS multi-hash name.
        /// </summary>
        /// <param name="name">
        ///   The name of a hashing algorithm, see <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        ///   for IPFS defined names.
        /// </param>
        /// <returns>
        ///   The hashing implementation associated with the <paramref name="name"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="name"/> is not registered.
        /// </exception>
        public static HashAlgorithm GetAlgorithm(AlgorithmNames name)
        {
            return GetAlgorithmMetadata(name).Hasher();
        }

        /// <summary>
        ///   Gets the metadata with the specified IPFS multi-hash name.
        /// </summary>
        /// <param name="name">
        ///   The name of a hashing algorithm, see <see href="https://github.com/multiformats/multicodec/blob/master/table.csv"/>
        ///   for IPFS defined names.
        /// </param>
        /// <returns>
        ///   The metadata associated with the hashing <paramref name="name"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   When <paramref name="name"/> is not registered.
        /// </exception>
        public static HashingAlgorithm GetAlgorithmMetadata(AlgorithmNames name)
        {
            try
            {
                return Names[name];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Hash algorithm '{name}' is not registered.");
            }
        }
    }
}