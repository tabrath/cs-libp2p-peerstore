using Multiformats.Address;
using Xunit;

namespace LibP2P.Peer.Store.Tests
{
    public class PeerInfoTests
    {
        private static Multiaddress MustAddr(string s) => Multiaddress.Decode(s);

        [Fact]
        public void TestPeerInfoMarshal()
        {
            var a = MustAddr("/ip4/1.2.3.4/tcp/4536");
            var b = MustAddr("/ip4/1.2.3.8/udp/7777");
            var id = PeerId.Decode("QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ");
            var pi = new PeerInfo(id, new[] {a,b});
            var data = pi.MarshalJson();
            var pi2 = PeerInfo.UnmarshalJson(data);

            Assert.Equal(pi2.Id, pi.Id);
            Assert.Equal(pi2.Addresses[0], pi.Addresses[0]);
            Assert.Equal(pi2.Addresses[1], pi.Addresses[1]);
            Assert.Equal(pi2.Id.ToString(), id.ToString());
        }
    }
}
