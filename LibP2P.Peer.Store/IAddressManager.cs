﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
        BlockingCollection<Multiaddress> AddressStream(PeerId peer, CancellationToken cancellationToken);
        void SetAddress(PeerId p, Multiaddress addr, TimeSpan ttl);
        void SetAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl);
    }
}