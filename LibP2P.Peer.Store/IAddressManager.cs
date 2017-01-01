using System;
using System.Collections.Generic;
using GoContext;
using Multiformats.Address;
using NChannels;

namespace LibP2P.Peer.Store
{
    public interface IAddressManager
    {
        PeerId[] Peers { get; }
        Multiaddress[] Addresses(PeerId p);

        void AddAddress(PeerId p, Multiaddress addr, TimeSpan ttl);
        void AddAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl);
        void ClearAddresses(PeerId peer);
        Chan<Multiaddress> AddressStream(Context ctx, PeerId peer);
        void SetAddress(PeerId p, Multiaddress addr, TimeSpan ttl);
        void SetAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl);
    }
}