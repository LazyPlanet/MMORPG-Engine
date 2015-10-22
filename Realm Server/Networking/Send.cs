using System;
using Lidgren.Network;
using Realm_Server.Logging;

namespace Realm_Server.Networking {
    public static class Send {

        private static void ClientSendData(NetBuffer data) {
            var client = NetClient.Instance();
            var logger = Logger.Instance();
            client.Send(data);
            logger.Write(String.Format("Sending {0} Bytes to Auth Server", data.LengthBytes), LogLevels.Debug);
        }

        private static void SendDataTo(NetConnection conn, NetBuffer data) {
            var server = NetServer.Instance();
            var logger = Logger.Instance();
            server.Send(conn, data);
            logger.Write(String.Format("Sending {0} Bytes to {1}", data.LengthBytes, NetUtility.ToHexString(conn.RemoteUniqueIdentifier)), LogLevels.Debug);
        }

        public static void ActivePing(Int32 id) {
            var logger = Logger.Instance();
            var data = new NetBuffer();
            data.Write((Int32)Packets.Client.ActivePing);
            data.Write(id);
            logger.Write("Sending ActivePing to AuthServer", LogLevels.Debug);
            ClientSendData(data);
        }

        public static void ConfirmGuid(Guid guid) {
            var logger = Logger.Instance();
            var data = new NetBuffer();
            data.Write((Int32)Packets.Client.ConfirmGuid);
            data.Write(guid.ToString());
            logger.Write("Sending ConfirmGuid to AuthServer", LogLevels.Debug);
            ClientSendData(data);
        }

        public static void AlertMessage(NetConnection conn, String msg, Packets.AlertMessage type) {
            var logger = Logger.Instance();
            var data = new NetBuffer();
            data.Write((Int32)Packets.Server.AlertMessage);
            data.Write((Int32)type);
            data.Write(msg);
            logger.Write(String.Format("Sending AlertMessage to {0}", NetUtility.ToHexString(conn.RemoteUniqueIdentifier)), LogLevels.Debug);
            SendDataTo(conn, data);
        }
    }
}
