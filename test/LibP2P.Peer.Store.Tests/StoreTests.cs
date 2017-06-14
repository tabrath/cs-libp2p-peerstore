using System.Collections.Generic;
using System.Linq;
using Multiformats.Address;
using Xunit;

namespace LibP2P.Peer.Store.Tests
{
    public class StoreTests
    {
        private static Multiaddress[] MakeAddresses(int count) => Enumerable.Range(0, count)
            .Select(i => Multiaddress.Decode($"/ip4/127.0.0.1/tcp/{i}")).ToArray();

/*        [Test]
        public void TestAddrStream()
        {
            var addrs = MakeAddresses(100);
            var pid = new PeerId("testpeer");
            var ps = new PeerStore();
            ps.AddAddresses(pid, addrs.Take(10), TimeSpan.FromHours(1));
            var cts = new CancellationTokenSource();
            var addrch = ps.AddressStream(pid, cts.Token);
            Multiaddress addr;

            for (var i = 10; i < 20; i++)
            {
                ps.AddAddress(pid, addrs[i], TimeSpan.FromHours(1));
            }

            for (var i = 0; i < 20; i++)
            {
                Assert.True(addrch.TryTake(out addr, 10000, cts.Token));
            }

            var done = new ManualResetEvent(false);
            Task.Factory.StartNew(() =>
            {
                for (var i = 20; i <= 80; i++)
                {
                    ps.AddAddress(pid, addrs[i], TimeSpan.FromHours(1));
                }
            }).ContinueWith(_ => done.Set());

            for (var i = 0; i < 20; i++)
            {
                Assert.True(addrch.TryTake(out addr, 10000, cts.Token));
            }

            Assert.True(done.WaitOne(100));

            for (var i = 0; i < 20; i++)
            {
                Assert.True(addrch.TryTake(out addr, 10000, cts.Token));
            }

            cts.Cancel();

            foreach (var a in addrs.Skip(80))
            {
                ps.AddAddress(pid, a, TimeSpan.FromHours(1));
            }
        }*/

        /*[Test]
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
        }*/

        [Fact]
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

            Assert.Equal(ps.Peers.Length, 10);

            var pinfo = ps.PeerInfo(pids[0]);
            Assert.Equal(pinfo.Addresses[0], addrs[0]);
        }

        [Fact]
        public void TestStoreProtoStore()
        {
            var ps = new PeerStore();
            var p1 = (PeerId) "ef2ea34a";
            var protos = new[] {"a","b","c","d"};
            ps.AddProtocols(p1, protos);
            var output = ps.GetProtocols(p1);
            Assert.Equal(output, protos);

            var sorted = protos.ToList();
            sorted.Sort();
            Assert.True(sorted.SequenceEqual(output));

            var supported = ps.SupportsProtocols(p1, "q", "w", "a", "y", "b");
            Assert.Equal(supported.Length, 2);
            Assert.Equal(supported[0], "a");
            Assert.Equal(supported[1], "b");
        }

        //TODO: Test "streaming"-alike functionality
    }
}
