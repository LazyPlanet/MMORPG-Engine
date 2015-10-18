using System;
using Lidgren.Network;

namespace Authentication_Server.Networking {
    public static class Send {

        private static void SendDataTo(NetConnection conn, NetBuffer data) {
            var server = NetServer.Instance();
            server.Send(conn, data);
        }

        public static void SendAuthFailed(NetConnection conn) {
            var data = new NetBuffer();
            data.Write((Int32)Packets.Server.AuthFailed);
            SendDataTo(conn, data);
        }

    }
}
