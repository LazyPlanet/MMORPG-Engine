using Lidgren.Network;
using Realm_Server.Logging;
using System;
using System.Collections.Generic;

namespace Realm_Server.Networking {
    public static class ServerHandlers {

        private static Dictionary<Packets.Client, Action<NetIncomingMessage>> Handler = new Dictionary<Packets.Client, Action<NetIncomingMessage>>() {
            
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
    }
}
