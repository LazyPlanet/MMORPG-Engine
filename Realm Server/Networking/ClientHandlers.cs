using Lidgren.Network;
using Realm_Server.Logging;
using System;
using System.Collections.Generic;

namespace Realm_Server.Networking {
    public static class ClientHandlers {

        private static Dictionary<Packets.Server, Action<NetIncomingMessage>> Handler = new Dictionary<Packets.Server, Action<NetIncomingMessage>>() {
            
        };

        public static void HandleNetMessage(object state) {
            var logger  = Logger.Instance();
            var peer    = state as NetPeer;
            var msg     = peer.ReadMessage();

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
                // Do stuff here when we are connected/disconnected.
                if (status == NetConnectionStatus.Connected) {
                    logger.Write("Contacting Authentication Server.", LogLevels.Normal);
                    Send.ActivePing((Int32)Properties.Settings.Default["RealmId"]);
                }
                break;

                case NetIncomingMessageType.Data:
                // Retrieve our data and pass it on to the designated handler.
                Action<NetIncomingMessage> handler;
                if (Handler.TryGetValue((Packets.Server)msg.ReadInt32(), out handler)) handler(msg);
                break;

                default:
                logger.Write("Unhandled Message: " + msg.MessageType + " " + msg.LengthBytes + " bytes " + msg.DeliveryMethod + "|" + msg.SequenceChannel, LogLevels.Debug);
                break;
            }

            // Recycle the message.
            peer.Recycle(msg);

        }
    }
}
