using Lidgren.Network;
using Realm_Server.Database;
using Realm_Server.Logging;
using System;
using System.Collections.Generic;

namespace Realm_Server.Networking {
    public static class ServerHandlers {

        private static Dictionary<Packets.Client, Action<NetIncomingMessage>> handler = new Dictionary<Packets.Client, Action<NetIncomingMessage>>() {
            { Packets.Client.AuthenticateClient, HandleAuthenticateClient }
        };

        private static Dictionary<NetIncomingMessageType, Action<NetIncomingMessage>> messagetypes = new Dictionary<NetIncomingMessageType, Action<NetIncomingMessage>>() {
            { NetIncomingMessageType.DebugMessage,          HandleDebugMessage },
            { NetIncomingMessageType.ErrorMessage,          HandleDebugMessage },
            { NetIncomingMessageType.WarningMessage,        HandleDebugMessage },
            { NetIncomingMessageType.VerboseDebugMessage,   HandleDebugMessage },
            { NetIncomingMessageType.StatusChanged,         HandleStatusChange },
            { NetIncomingMessageType.Data,                  HandleData },
        };

        private static void HandleData(NetIncomingMessage msg) {
            // Retrieve our data and pass it on to the designated handler.
            Action<NetIncomingMessage> exec;
            if (handler.TryGetValue((Packets.Client)msg.ReadInt32(), out exec)) exec(msg);
        }

        private static void HandleStatusChange(NetIncomingMessage msg) {
            var logger = Logger.Instance();
            var status = (NetConnectionStatus)msg.ReadByte();
            var reason = msg.ReadString();
            logger.Write(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason, LogLevels.Debug);
            if (status == NetConnectionStatus.Connected) logger.Write("Remote hail: " + msg.SenderConnection.RemoteHailMessage.ReadString(), LogLevels.Informational);
        }

        private static void HandleDebugMessage(NetIncomingMessage msg) {
            var logger = Logger.Instance();
            logger.Write(msg.ReadString(), LogLevels.Debug);
        }

        public static void HandleNetMessage(object state) {
            var logger  = Logger.Instance();
            var peer    = state as NetPeer;
            var msg     = peer.ReadMessage();

            if (msg.SenderConnection != null) {
                logger.Write(String.Format("Received {0} Bytes from {1}", msg.LengthBytes, NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Debug);
            } else {
                logger.Write("Handling local message.", LogLevels.Debug);
            }

            Action<NetIncomingMessage> exec;
            if (!messagetypes.TryGetValue(msg.MessageType, out exec)) exec = (dat) => { var log = Logger.Instance(); log.Write("Unhandled Message: " + dat.MessageType + " " + dat.LengthBytes + " bytes " + dat.DeliveryMethod + "|" + dat.SequenceChannel, LogLevels.Debug); };
            exec(msg);

            // Recycle the message.
            peer.Recycle(msg);
        }

        private static void HandleAuthenticateClient(NetIncomingMessage msg) {
            var logger = Logger.Instance();
            var netid = NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier);
            logger.Write(String.Format("Received AuthenticateClient from {0}", netid), LogLevels.Informational);

            // Get our user.
            var user = NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier);

            // Get our GUID.
            var guid = Guid.Parse(msg.ReadString());

            // Create a new user and add their (currently known) data.
            var store = PlayerStore.Instance();
            store.AddPlayer(netid);
            store.SetAuthorizationId(netid, guid);
            logger.Write(String.Format("Adding new Player with GUID: {0} AuthorizationID: {1}", guid, netid), LogLevels.Debug);

            // Try and confirm this guid with our authentication server.
            Send.ConfirmGuid(guid);
        }
    }
}
