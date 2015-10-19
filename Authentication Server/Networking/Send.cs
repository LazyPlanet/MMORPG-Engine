using System;
using Lidgren.Network;
using Authentication_Server.Database;

namespace Authentication_Server.Networking {
    public static class Send {

        private static void SendDataTo(NetConnection conn, NetBuffer data) {
            var server = NetServer.Instance();
            server.Send(conn, data);
        }

        public static void AuthFailed(NetConnection conn) {
            var data = new NetBuffer();
            data.Write((Int32)Packets.Server.AuthFailed);
            SendDataTo(conn, data);
        }

        public static void AuthSuccess(NetConnection conn, Guid guid) {
            var data = new NetBuffer();
            var list = RealmList.Instance().GetRealms();
            data.Write((Int32)Packets.Server.AuthSuccess);
            data.Write(guid.ToString());
            data.Write(list.Count);
            foreach (var item in list) {
                data.Write(item.Name);
                data.Write(item.Hostname);
                data.Write(item.Port);
                data.Write(item.LastUsed.ToString());
            }
            SendDataTo(conn, data);
        }

    }
}
