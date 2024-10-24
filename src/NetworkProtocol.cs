﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using System.Globalization;

namespace Ipfs
{
    /// <summary>
    ///   Metadata on an IPFS network address protocol.
    /// </summary>
    /// <remarks>
    ///   Protocols are defined at <see href="https://github.com/multiformats/multiaddr/blob/master/protocols.csv"/>.
    /// </remarks>
    /// <seealso cref="MultiAddress"/>
    public abstract class NetworkProtocol
    {
        internal static Dictionary<string, Type> Names = new();
        internal static Dictionary<uint, Type> Codes = new();

        /// <summary>
        ///   Registers the standard network protocols for IPFS.
        /// </summary>
        static NetworkProtocol()
        {
            Register<Ipv4NetworkProtocol>();
            Register<Ipv6NetworkProtocol>();
            Register<TcpNetworkProtocol>();
            Register<UdpNetworkProtocol>();
            Register<P2pNetworkProtocol>();
            RegisterAlias<IpfsNetworkProtocol>();
            Register<QuicNetworkProtocol>();
            Register<HttpNetworkProtocol>();
            Register<HttpsNetworkProtocol>();
            Register<DccpNetworkProtocol>();
            Register<SctpNetworkProtocol>();
            Register<WsNetworkProtocol>();
            Register<Libp2pWebrtcStarNetworkProtocol>();
            Register<UdtNetworkProtocol>();
            Register<UtpNetworkProtocol>();
            Register<OnionNetworkProtocol>();
            Register<Libp2pWebrtcDirectNetworkProtocol>();
            Register<P2pCircuitNetworkProtocol>();
            Register<DnsNetworkProtocol>();
            Register<Dns4NetworkProtocol>();
            Register<Dns6NetworkProtocol>();
            Register<DnsAddrNetworkProtocol>();
            Register<WssNetworkProtocol>();
            Register<IpcidrNetworkProtocol>();
        }

        /// <summary>
        ///   Register a network protocol for use.
        /// </summary>
        /// <typeparam name="T">
        ///   A <see cref="NetworkProtocol"/> to register.
        /// </typeparam>
        public static void Register<T>() where T : NetworkProtocol, new()
        {
            var protocol = new T();

            if (Names.ContainsKey(protocol.Name))
                throw new ArgumentException(string.Format("The IPFS network protocol '{0}' is already defined.", protocol.Name));
            if (Codes.ContainsKey(protocol.Code))
                throw new ArgumentException(string.Format("The IPFS network protocol code ({0}) is already defined.", protocol.Code));

            Names.Add(protocol.Name, typeof(T));
            Codes.Add(protocol.Code, typeof(T));
        }

        /// <summary>
        ///   Register an alias to another network protocol.
        /// </summary>
        /// <typeparam name="T">
        ///   A <see cref="NetworkProtocol"/> to register.
        /// </typeparam>
        public static void RegisterAlias<T>() where T : NetworkProtocol, new()
        {
            var protocol = new T();

            if (Names.ContainsKey(protocol.Name))
                throw new ArgumentException(string.Format("The IPFS network protocol '{0}' is already defined.", protocol.Name));
            if (!Codes.ContainsKey(protocol.Code))
                throw new ArgumentException(string.Format("The IPFS network protocol code ({0}) is not defined.", protocol.Code));

            Names.Add(protocol.Name, typeof(T));
        }

        /// <summary>
        ///   The name of the protocol.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   The IPFS numeric code assigned to the network protocol.
        /// </summary>
        public abstract uint Code { get; }

        /// <summary>
        ///   The string value associated with the protocol.
        /// </summary>
        /// <remarks>
        ///   For tcp and udp this is the port number.  This can be <b>null</b> as is the case for http and https.
        /// </remarks>
        public string Value { get; set; }

        /// <summary>
        ///   Writes the binary representation to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to write to.
        /// </param>
        /// <remarks>
        ///   The binary representation of the <see cref="Value"/>.
        /// </remarks>
        public abstract void WriteValue(CodedOutputStream stream);

        /// <summary>
        ///   Writes the string representation to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="TextWriter"/> to write to.
        /// </param>
        /// <remarks>
        ///   The string representation of the optional <see cref="Value"/>.
        /// </remarks>
        public virtual void WriteValue(TextWriter stream)
        {
            if (Value != null)
            {
                stream.Write('/');
                stream.Write(Value);
            }
        }

        /// <summary>
        ///   Reads the binary representation from the specified <see cref="CodedInputStream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="CodedOutputStream"/> to read from.
        /// </param>
        /// <remarks>
        ///   The binary representation is an option <see cref="Value"/>.
        /// </remarks>
        public abstract void ReadValue(CodedInputStream stream);

        /// <summary>
        ///   Reads the string representation from the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="stream">
        ///   The <see cref="TextReader"/> to read from
        /// </param>
        /// <remarks>
        ///   The string representation is "/<see cref="Name"/>" followed by
        ///   an optional "/<see cref="Value"/>".
        /// </remarks>
        public virtual void ReadValue(TextReader stream)
        {
            Value = string.Empty;
            int c;
            while (-1 != (c = stream.Read()) && c != '/')
            {
                Value += (char)c;
            }
        }

        /// <summary>
        ///   The <see cref="Name"/> and optional <see cref="Value"/> of the network protocol.
        /// </summary>
        public override string ToString()
        {
            using (var s = new StringWriter())
            {
                s.Write('/');
                s.Write(Name);
                WriteValue(s);
                return s.ToString();
            }
        }
    }

    internal class TcpNetworkProtocol : NetworkProtocol
    {
        public ushort Port { get; set; }
        public override string Name
        { get { return "tcp"; } }
        public override uint Code
        { get { return 6; } }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            try
            {
                Port = ushort.Parse(Value);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid port number.", Value), e);
            }
        }

        public override void ReadValue(CodedInputStream stream)
        {
            var bytes = stream.ReadSomeBytes(2);
            Port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = Port.ToString(CultureInfo.InvariantCulture);
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Port));
            stream.WriteSomeBytes(bytes);
        }
    }

    internal class UdpNetworkProtocol : TcpNetworkProtocol
    {
        public override string Name
        { get { return "udp"; } }
        public override uint Code
        { get { return 273; } }
    }

    internal class DccpNetworkProtocol : TcpNetworkProtocol
    {
        public override string Name
        { get { return "dccp"; } }
        public override uint Code
        { get { return 33; } }
    }

    internal class SctpNetworkProtocol : TcpNetworkProtocol
    {
        public override string Name
        { get { return "sctp"; } }
        public override uint Code
        { get { return 132; } }
    }

    internal abstract class IpNetworkProtocol : NetworkProtocol
    {
        public IPAddress Address { get; set; }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            try
            {
                // Remove the scope id.
                int i = Value.LastIndexOf('%');
                if (i != -1)
                    Value = Value.Substring(0, i);

                Address = IPAddress.Parse(Value);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid IP address.", Value), e);
            }
        }

        public override void WriteValue(TextWriter stream)
        {
            stream.Write('/');
            stream.Write(Address.ToString());
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            var ip = Address.GetAddressBytes();
            stream.WriteSomeBytes(ip);
        }
    }

    internal class Ipv4NetworkProtocol : IpNetworkProtocol
    {
        private static readonly int AddressSize = IPAddress.Any.GetAddressBytes().Length;

        public override string Name
        { get { return "ip4"; } }
        public override uint Code
        { get { return 4; } }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            if (Address.AddressFamily != AddressFamily.InterNetwork)
                throw new FormatException(string.Format("'{0}' is not a valid IPv4 address.", Value));
        }

        public override void ReadValue(CodedInputStream stream)
        {
            var a = stream.ReadSomeBytes(AddressSize);
            Address = new IPAddress(a);
            Value = Address.ToString();
        }
    }

    internal class Ipv6NetworkProtocol : IpNetworkProtocol
    {
        private static readonly int AddressSize = IPAddress.IPv6Any.GetAddressBytes().Length;

        public override string Name
        { get { return "ip6"; } }
        public override uint Code
        { get { return 41; } }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            if (Address.AddressFamily != AddressFamily.InterNetworkV6)
                throw new FormatException(string.Format("'{0}' is not a valid IPv6 address.", Value));
        }

        public override void ReadValue(CodedInputStream stream)
        {
            var a = stream.ReadSomeBytes(AddressSize);
            Address = new IPAddress(a);
            Value = Address.ToString();
        }
    }

    internal class P2pNetworkProtocol : NetworkProtocol
    {
        public MultiHash MultiHash { get; private set; }
        public override string Name
        { get { return "p2p"; } }
        public override uint Code
        { get { return 421; } }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            MultiHash = new MultiHash(Value);
        }

        public override void ReadValue(CodedInputStream stream)
        {
            stream.ReadLength();
            MultiHash = new MultiHash(stream);
            Value = MultiHash.ToBase58();
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = MultiHash.ToArray();
            stream.WriteLength(bytes.Length);
            stream.WriteSomeBytes(bytes);
        }
    }

    internal class IpfsNetworkProtocol : P2pNetworkProtocol
    {
        public override string Name
        { get { return "ipfs"; } }
    }

    internal class OnionNetworkProtocol : NetworkProtocol
    {
        public byte[] Address { get; private set; }
        public ushort Port { get; private set; }
        public override string Name
        { get { return "onion"; } }
        public override uint Code
        { get { return 444; } }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            var parts = Value.Split(':');
            if (parts.Length != 2)
                throw new FormatException(string.Format("'{0}' is not a valid onion address, missing the port number.", Value));
            if (parts[0].Length != 16)
                throw new FormatException(string.Format("'{0}' is not a valid onion address.", Value));
            try
            {
                Port = ushort.Parse(parts[1]);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid onion address, invalid port number.", Value), e);
            }
            if (Port < 1)
                throw new FormatException(string.Format("'{0}' is not a valid onion address, invalid port number.", Value));
            Address = parts[0].ToUpperInvariant().FromBase32();
        }

        public override void ReadValue(CodedInputStream stream)
        {
            Address = stream.ReadSomeBytes(10);
            var bytes = stream.ReadSomeBytes(2);
            Port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = Address.ToBase32().ToLowerInvariant() + ":" + Port.ToString(CultureInfo.InvariantCulture);
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            stream.WriteSomeBytes(Address);
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Port));
            stream.WriteSomeBytes(bytes);
        }
    }

    internal abstract class ValuelessNetworkProtocol : NetworkProtocol
    {
        public override void ReadValue(CodedInputStream stream)
        {
            // No value to read
        }

        public override void ReadValue(TextReader stream)
        {
            // No value to read
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            // No value to write
        }
    }

    internal class QuicNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "quic"; } }
        public override uint Code
        { get { return 460; } }
    }

    internal class HttpNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "http"; } }
        public override uint Code
        { get { return 480; } }
    }

    internal class HttpsNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "https"; } }
        public override uint Code
        { get { return 443; } }
    }

    internal class WsNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "ws"; } }
        public override uint Code
        { get { return 477; } }
    }

    internal class WssNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "wss"; } }
        public override uint Code
        { get { return 478; } }
    }

    internal class Libp2pWebrtcStarNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "libp2p-webrtc-star"; } }
        public override uint Code
        { get { return 275; } }
    }

    internal class Libp2pWebrtcDirectNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "libp2p-webrtc-direct"; } }
        public override uint Code
        { get { return 276; } }
    }

    internal class UdtNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "udt"; } }
        public override uint Code
        { get { return 301; } }
    }

    internal class UtpNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "utp"; } }
        public override uint Code
        { get { return 302; } }
    }

    internal class P2pCircuitNetworkProtocol : ValuelessNetworkProtocol
    {
        public override string Name
        { get { return "p2p-circuit"; } }
        public override uint Code
        { get { return 290; } }
    }

    internal abstract class DomainNameNetworkProtocol : NetworkProtocol
    {
        public string DomainName { get; set; }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            DomainName = Value;
        }

        public override void ReadValue(CodedInputStream stream)
        {
            Value = stream.ReadString();
            DomainName = Value;
        }

        public override void WriteValue(TextWriter stream)
        {
            stream.Write('/');
            stream.Write(DomainName.Trim());
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            stream.WriteString(DomainName);
        }
    }

    internal class DnsNetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name
        { get { return "dns"; } }
        public override uint Code
        { get { return 53; } }
    }

    internal class DnsAddrNetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name
        { get { return "dnsaddr"; } }
        public override uint Code
        { get { return 56; } }
    }

    internal class Dns4NetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name
        { get { return "dns4"; } }
        public override uint Code
        { get { return 54; } }
    }

    internal class Dns6NetworkProtocol : DomainNameNetworkProtocol
    {
        public override string Name
        { get { return "dns6"; } }
        public override uint Code
        { get { return 55; } }
    }

    internal class IpcidrNetworkProtocol : NetworkProtocol
    {
        public ushort RoutingPrefix { get; set; }
        public override string Name
        { get { return "ipcidr"; } }

        // TODO: https://github.com/multiformats/multiaddr/issues/60
        public override uint Code
        { get { return 999; } }

        public override void ReadValue(TextReader stream)
        {
            base.ReadValue(stream);
            try
            {
                RoutingPrefix = ushort.Parse(Value);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format("'{0}' is not a valid routing prefix.", Value), e);
            }
        }

        public override void ReadValue(CodedInputStream stream)
        {
            var bytes = stream.ReadSomeBytes(2);
            RoutingPrefix = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0));
            Value = RoutingPrefix.ToString(CultureInfo.InvariantCulture);
        }

        public override void WriteValue(CodedOutputStream stream)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)RoutingPrefix));
            stream.WriteSomeBytes(bytes);
        }
    }
}