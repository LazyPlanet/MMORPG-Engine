using Lidgren.Network;
using Realm_Server.Database;
using Realm_Server.Logging;
using System;
using System.Collections.Generic;

namespace Realm_Server.Networking {
    public static class ClientHandlers {

        private static Dictionary<Packets.Server, Action<NetIncomingMessage>> handler = new Dictionary<Packets.Server, Action<NetIncomingMessage>>() {
            { Packets.Server.GuidOK,    HandleGuidOk },
            { Packets.Server.GuidError, HandleGuidError },
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
            if (handler.TryGetValue((Packets.Server)msg.ReadInt32(), out exec)) exec(msg);
        }

        private static void HandleStatusChange(NetIncomingMessage msg) {
            var logger = Logger.Instance();
            var status = (NetConnectionStatus)msg.ReadByte();
            var reason = msg.ReadString();
            logger.Write(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason, LogLevels.Debug);
            // Do stuff here when we are connected/disconnected.
            if (status == NetConnectionStatus.Connected) {
                logger.Write("Contacting Authentication Server.", LogLevels.Normal);
                Send.ActivePing((Int32)Properties.Settings.Default["RealmId"]);
            }
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

        private static void HandleGuidError(NetIncomingMessage msg) {
            var logger  = Logger.Instance();
            var store   = PlayerStore.Instance();
            var guid    = Guid.Parse(msg.ReadString());
            var server  = NetServer.Instance();
            logger.Write(String.Format("Received HandleGuidError From Authentication Server for GUID: {0}", guid), LogLevels.Informational);

            // Get our network peer ID and the accompanying connection.
            var netid = store.GetIdentifierFromGuid(guid);
            var conn = server.GetConnectionFromId(netid);
        }

        private static void HandleGuidOk(NetIncomingMessage msg) {
            var logger  = Logger.Instance();
            var store   = PlayerStore.Instance();
            var guid    = Guid.Parse(msg.ReadString());
            var id      = msg.ReadInt32();
            var server  = NetServer.Instance();
            logger.Write(String.Format("Received HandleGuidOK From Authentication Server for GUID: {0}", guid), LogLevels.Informational);

            // Get our network peer ID and the accompanying connection.
            var netid   = store.GetIdentifierFromGuid(guid);
            var conn    = server.GetConnectionFromId(netid);

            // Check if user is already logged on, and if so kick them.
            var activeuser = store.GetIdentifierFromDatabaseId(id) ?? String.Empty;
            if (!activeuser.Equals(String.Empty)) {
                // TODO: Save Player Data
                // Kick Player from the server with a message.
                var oldconn = server.GetConnectionFromId(activeuser);
                Send.AlertMessage(oldconn, "Another user has logged into your account\nYou will be disconnected.", Packets.AlertMessage.Fatal);
            }

            // Set the user's database ID for future reference.
            store.SetDatabaseId(netid, id);
        }
    }
}
