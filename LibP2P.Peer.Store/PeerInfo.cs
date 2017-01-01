using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiformats.Address;
using Multiformats.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibP2P.Peer.Store
{
    public class PeerInfo
    {
        public PeerId Id { get; }
        public Multiaddress[] Addresses { get; }

        public PeerInfo(PeerId id, Multiaddress[] addresses)
        {
            Id = id;
            Addresses = addresses;
        }

        public byte[] MarshalJson()
        {
            var output = new Dictionary<string, object>
            {
                ["ID"] = Id.ToString(Multibase.Base58),
                ["Addrs"] = Addresses.Select(a => a.ToString()).ToArray()
            };

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));
        }

        public static PeerInfo UnmarshalJson(byte[] b)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(b));
            var pid = new PeerId(Multibase.DecodeRaw(Multibase.Base58, (string) data["ID"]));
            var addrs = ((JArray)data["Addrs"]).Select(t => Multiaddress.Decode((string)t));

            return new PeerInfo(pid, addrs.ToArray());
        }
    }
}