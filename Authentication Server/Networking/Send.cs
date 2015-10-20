using System;
using System.Linq;
using Lidgren.Network;
using Authentication_Server.Logging;
using Authentication_Server.Database;

namespace Authentication_Server.Networking {
    public static class Send {

        private static void SendDataTo(NetConnection conn, NetBuffer data) {
            var server = NetServer.Instance();
            var logger = Logger.Instance();
            server.Send(conn, data);
            logger.Write(String.Format("Sending {0} Bytes to {1}", data.LengthBytes, NetUtility.ToHexString(conn.RemoteUniqueIdentifier)), LogLevels.Debug);
        }

        public static void AuthFailed(NetConnection conn) {
            var data    = new NetBuffer();
            var logger  = Logger.Instance();
            data.Write((Int32)Packets.Server.AuthFailed);
            logger.Write(String.Format("Sending AuthFailed to {0}", NetUtility.ToHexString(conn.RemoteUniqueIdentifier)), LogLevels.Debug);
            SendDataTo(conn, data);
        }

        public static void AuthSuccess(NetConnection conn, Guid guid) {
            var data    = new NetBuffer();
            var list    = RealmList.Instance().GetRealms();
            var logger  = Logger.Instance();
            data.Write((Int32)Packets.Server.AuthSuccess);
            data.Write(guid.ToString());
            data.Write(list.Count);
            foreach (var item in list) {
                data.Write(item.Value.Name);
                data.Write(item.Value.Hostname);
                data.Write(item.Value.Port);
                data.Write(
                    (from c in NetServer.Instance().Connections()
                     select NetUtility.ToHexString(c.RemoteUniqueIdentifier))
                     .Contains(item.Value.RemoteIdentifier) ? true : false
                );
            }
            logger.Write(String.Format("Sending AuthSuccess to {0}", NetUtility.ToHexString(conn.RemoteUniqueIdentifier)), LogLevels.Debug);
            SendDataTo(conn, data);
        }

    }
}
