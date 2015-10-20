using System;
using Lidgren.Network;

namespace Realm_Server.Networking {
    public static class Send {

        private static void SendData(NetBuffer data) {
            var client = NetClient.Instance();
            client.Send(data);
        }

        public static void ActivePing(Int32 id) {
            var data = new NetBuffer();
            data.Write((Int32)Packets.Client.ActivePing);
            data.Write(id);
            SendData(data);
        }

    }
}
