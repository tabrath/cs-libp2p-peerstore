﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GoContext;
using Multiformats.Address;
using NChannels;
using NUnit.Framework;

namespace LibP2P.Peer.Store.Tests
{
    [TestFixture]
    public class StoreTests
    {
        private static Multiaddress[] MakeAddresses(int count) => Enumerable.Range(0, count)
            .Select(i => Multiaddress.Decode($"/ip4/127.0.0.1/tcp/{i}")).ToArray();

        [Test]
        public void TestAddrStream()
        {
            var addrs = MakeAddresses(100);
            var pid = new PeerId("testpeer");
            var ps = new PeerStore();
            ps.AddAddresses(pid, addrs.Take(10), TimeSpan.FromHours(1));
            Action cancel;
            var ctx = Context.Background.WithCancel(out cancel);
            var addrch = ps.AddressStream(ctx, pid);

            for (var i = 10; i < 20; i++)
            {
                ps.AddAddress(pid, addrs[i], TimeSpan.FromHours(1));
            }

            for (var i = 0; i < 20; i++)
            {
                new Select()
                    .Case(addrch, (x, y) => { })
                    .Case(TimeSpan.FromSeconds(10).After(), (x, y) => Assert.Fail("timed out"))
                    .End()
                    .Wait();
            }

            var done = new Chan<object>();
            Task.Run(() =>
            {
                for (var i = 20; i <= 80; i++)
                {
                    ps.AddAddress(pid, addrs[i], TimeSpan.FromHours(1));
                }
            }).ContinueWith(_ => done.Close());

            new Select()
                .Case(addrch, (x, y) => { })
                .Case(TimeSpan.FromSeconds(10).After(), (x, y) => { })
                .End()
                .Wait();

            done.Receive().Wait();

            for (var i = 0; i < 20; i++)
            {
                new Select()
                    .Case(addrch, (x, y) => { })
                    .Case(TimeSpan.FromSeconds(10).After(), (x, y) => { })
                    .End()
                    .Wait();
            }

            cancel();

            foreach (var a in addrs.Skip(80))
            {
                ps.AddAddress(pid, a, TimeSpan.FromHours(1));
            }
        }

        [Test]
        public void TestGetStreamBeforePeerAdded()
        {
            var addrs = MakeAddresses(10);
            var pid = new PeerId("testpeer");
            var ps = new PeerStore();
            Action cancel;
            var ctx = Context.Background.WithCancel(out cancel);
            try
            {
                var ach = ps.AddressStream(ctx, pid);
                for (var i = 0; i < 10; i++)
                {
                    ps.AddAddress(pid, addrs[i], TimeSpan.FromHours(1));
                }

                var received = new List<string>();
                for (var i = 0; i < 10; i++)
                {
                    var res = ach.Receive().Result;

                    Assert.That(res.IsSuccess, Is.True);
                    Assert.That(res.Result, Is.Not.Null);
                    Assert.That(received, Does.Not.Contain(res.Result.ToString()));

                    received.Add(res.Result.ToString());
                }

                Assert.That(ach.Receive().Wait(1), Is.False);
                Assert.That(received.Count, Is.EqualTo(10));
                foreach (var a in addrs)
                {
                    Assert.That(received, Does.Contain(a));
                }
            }
            finally
            {
                cancel();
            }
        }

        [Test]
        public void TestBasicStore()
        {
            var ps = new PeerStore();
            var addrs = MakeAddresses(10);
            var i = 0;
            var pids = new List<PeerId>();
            foreach (var a in addrs)
            {
                var p = new PeerId((i++).ToString());
                pids.Add(p);
                ps.AddAddress(p, a, AddressManager.PermanentAddrTTL);
            }

            Assert.That(ps.Peers.Length, Is.EqualTo(10));

            var pinfo = ps.PeerInfo(pids[0]);
            Assert.That(pinfo.Addresses[0], Is.EqualTo(addrs[0]));
        }

        [Test]
        public void TestStoreProtoStore()
        {
            var ps = new PeerStore();
            var p1 = (PeerId) "ef2ea34a";
            var protos = new[] {"a","b","c","d"};
            ps.AddProtocols(p1, protos);
            var output = ps.GetProtocols(p1);
            Assert.That(output, Is.EqualTo(protos));

            var sorted = protos.ToList();
            sorted.Sort();
            Assert.That(sorted.SequenceEqual(output), Is.True);

            var supported = ps.SupportsProtocols(p1, "q", "w", "a", "y", "b");
            Assert.That(supported.Length, Is.EqualTo(2));
            Assert.That(supported[0], Is.EqualTo("a"));
            Assert.That(supported[1], Is.EqualTo("b"));
        }

        //TODO: Test "streaming"-alike functionality
    }
}
