# net-ipfs-core 

![Version](https://img.shields.io/nuget/v/IpfsShipyard.Ipfs.Core.svg)

The core objects and interfaces of the [IPFS](https://github.com/ipfs/ipfs) (Inter Planetary File System) for .Net (C#, VB, F# etc.)

The interplanetary file system is the permanent web. It is a new hypermedia distribution protocol, addressed by content and identities. IPFS enables the creation of completely distributed applications. It aims to make the web faster, safer, and more open.

It supports .NET Standard 2.0.

### 🚧 NOTICE 🚧
We've only [just](https://github.com/richardschneider/net-ipfs-http-client/issues/72) moved into the shipyard, reviving a project that has been abandoned since 2019.

**We're actively working to make it usable again.**

## Major objects

- [MerkleDag](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.DagNode.html)
- [MultiAddress](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.MultiAddress.html)
- [MultiHash](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.MultiHash.html)

See the [API Documentation](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.html) for a list of all objects.

### MultiHash

All hashes in IPFS are encoded with [multihash](https://github.com/multiformats/multihash), a self-describing hash format. The actual hash function used depends on security requirements. The cryptosystem of IPFS is upgradeable, meaning that as hash functions are broken, networks can shift to stronger hashes. There is no free lunch, as objects may need to be rehashed, or links duplicated. But ensuring that tools built do not assume a pre-defined length of hash digest means tools that work with today's hash functions will also work with tomorrows longer hash functions too.

### MultiAddress

A standard way to represent a networks address that supports [multiple network protocols](https://github.com/multiformats/multiaddr). It is represented as a series of tuples, a protocol code and an optional value.  For example, an IPFS file at a sepcific address over ipv4 and tcp is 

    /ip4/10.1.10.10/tcp/80/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC

### Merkle DAG

The [DagNode](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.DagNode.html) is a directed acyclic graph whose edges are a 
[DagLink](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.DagLink.html). This means that links to objects can authenticate 
the objects themselves, and that every object contains a secure 
representation of its children.

Every Merkle is a directed acyclic graph (DAG) because each node is accessed via its name (the hash of `DagNode`). Each branch of Merkle is the hash of its local content (data and links);  naming children by their hash instead of their full contents. So after creation there is no way to edit a DagNode. This prevents cycles (assuming there are no hash collisions) since one can not link the first created node to the last note to create the last reference.

## Base58

Most binary data (objects) in IPFS is represented as a [Base-58](https://en.wikipedia.org/wiki/Base58) string; the BitCoin alphabet is used.

> Base58 is a group of binary-to-text encoding schemes used to represent large integers as alphanumeric text. It is similar to Base64 but has been modified to avoid both non-alphanumeric characters and letters which might look ambiguous when printed. It is therefore designed for human users who manually enter the data, copying from some visual source, but also allows easy copy and paste because a double-click will usually select the whole string. 

## Related Projects

- [IPFS DSL](https://github.com/cloveekprojeqt/ipfs-dsl) - A declarative embedded language for building compositional programs and protocols over the InterPlanetary File System.
- [IPFS HTTP Client](https://github.com/ipfs-shipyard/net-ipfs-http-client) - A .Net client library for the IPFS HTTP API.
- [IPFS HTTP Gateway](https://github.com/richardschneider/net-ipfs-http-gateway) - Serves IPFS files/directories via HTTP.
- [IPFS Engine](https://github.com/richardschneider/net-ipfs-engine) - Implements the Core API.
- [Peer Talk](https://github.com/richardschneider/peer-talk) - Peer to peer communication.

## License
The IPFS Core library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-ipfs-core/blob/master/LICENSE) file for more information.

