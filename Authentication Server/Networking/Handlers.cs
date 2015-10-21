using System;
using Lidgren.Network;
using System.Collections.Generic;
using Authentication_Server.Logging;
using Authentication_Server.Database;

namespace Authentication_Server.Networking {
    public static class Handlers {

        private static Dictionary<Packets.Client, Action<NetIncomingMessage>> Handler = new Dictionary<Packets.Client, Action<NetIncomingMessage>>() {
            { Packets.Client.LoginRequest,    HandleLoginRequest },
            { Packets.Client.ActivePing,      HandleActivePing },
            { Packets.Client.ConfirmGuid,     HandleConfirmGuid },
        };

        public static void HandleNetMessage(object state) {
            var logger = Logger.Instance();
            var peer = state as NetPeer;
            var msg = peer.ReadMessage();

            if (msg.SenderConnection != null) {
                logger.Write(String.Format("Received {0} Bytes from {1}", msg.LengthBytes, NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Debug);
            } else {
                logger.Write("Handling local message.", LogLevels.Debug);
            }
            switch (msg.MessageType) {

                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                logger.Write(msg.ReadString(), LogLevels.Debug);
                break;

                case NetIncomingMessageType.StatusChanged:
                var status = (NetConnectionStatus)msg.ReadByte();
                var reason = msg.ReadString();
                logger.Write(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason, LogLevels.Debug);
                if (status == NetConnectionStatus.Connected) logger.Write("Remote hail: " + msg.SenderConnection.RemoteHailMessage.ReadString(), LogLevels.Informational);
                break;

                case NetIncomingMessageType.Data:
                // Retrieve our data and pass it on to the designated handler.
                Action<NetIncomingMessage> handler;
                if (Handler.TryGetValue((Packets.Client)msg.ReadInt32(), out handler)) handler(msg);
                break;

                default:
                logger.Write("Unhandled Message: " + msg.MessageType + " " + msg.LengthBytes + " bytes " + msg.DeliveryMethod + "|" + msg.SequenceChannel, LogLevels.Debug);
                break;
            }

            // Recycle the message.
            peer.Recycle(msg);

        }

        private static void HandleActivePing(NetIncomingMessage msg) {
            var logger  = Logger.Instance();
            var list    = RealmList.Instance();
            logger.Write(String.Format("Received ActivePing from {0}", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Informational);

            // Retrieve our ID.
            var id = msg.ReadInt32();

            // Update our realm, if we have one.
            if (list.UpdateRealmStatus(id, NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier))) {
                logger.Write(String.Format("Realm ID: {0} connected with RemoteIdentifier: {1}", id, NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Debug);
            } else {
                logger.Write(String.Format("Realm ID: {0} failed to provide a valid RealmId.", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Debug);
            }
        }
        private static void HandleLoginRequest(NetIncomingMessage msg) {
            var logger = Logger.Instance();
            logger.Write(String.Format("Received LoginRequest from {0}", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Informational);

            // Retrieve our username and password from the message.
            var user = msg.ReadString();
            var pass = msg.ReadString();

            // Attempt to authenticate the user.
            var result = Data.AuthenticateUser(user, pass);
            if (result[0] == 0) {
                // Login OK.
                // Generate a brand new GUID and add our user to the internal storage for later use.
                var id = result[1];
                var guid = Guid.NewGuid();
                var storage = GUIDStore.Instance();
                storage.AddGUID(id, guid);

                logger.Write(String.Format("Authenticated user at Peer: {0}", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Debug);
                logger.Write(String.Format("Added GUID: {0} to storage.", guid), LogLevels.Debug);

                // Send our user the OK and our realmlist.
                Send.AuthSuccess(msg.SenderConnection, guid);
            } else {
                // Login Failed.

                logger.Write(String.Format("Authentication failed for user at Peer: {0}", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Debug);

                // Tell our user they entered incorrect information.
                Send.AuthFailed(msg.SenderConnection);
            }
        }

        private static void HandleConfirmGuid(NetIncomingMessage msg) {
            var logger = Logger.Instance();
            logger.Write(String.Format("Received ConfirmGuid from {0}", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)), LogLevels.Informational);
            var store = GUIDStore.Instance();

            // See if our storage contains this Guid.
            var guid = Guid.Parse(msg.ReadString());
            if (store.Contains(guid)) {
                // Yes it does, we can accept this client.
                logger.Write(String.Format("GUID: {0} Exists, allowing user on.", guid), LogLevels.Debug);
                Send.GuidOK(msg.SenderConnection, guid);
            } else {
                // No it doesn't, where did they come from?
                logger.Write(String.Format("GUID: {0} Does NotExists, notify user.", guid), LogLevels.Debug);
                Send.GuidError(msg.SenderConnection, guid);
            }
            
        }

    }
}
