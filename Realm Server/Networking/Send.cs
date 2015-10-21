using System;
using Lidgren.Network;
using Realm_Server.Logging;

namespace Realm_Server.Networking {
    public static class Send {

        private static void ClientSendData(NetBuffer data) {
            var client = NetClient.Instance();
            client.Send(data);
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

    }
}
