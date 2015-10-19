using System;
using Lidgren.Network;
using System.Collections.Generic;

namespace Game_Client.Networking {
    public static class Handlers {

        private static Dictionary<Packets.Server, Action<NetIncomingMessage>> Handler = new Dictionary<Packets.Server, Action<NetIncomingMessage>>() {
            { Packets.Server.AuthSuccess,   HandleAuthSuccess },
            { Packets.Server.AuthFailed,    HandleAuthFailed },
        };

        public static void HandleNetMessage(object state) {
            var peer = state as NetPeer;
            var msg = peer.ReadMessage();

            switch (msg.MessageType) {

                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    Console.WriteLine(msg.ReadString());
                    break;

                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus)msg.ReadByte();
                    var reason = msg.ReadString();
                    Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);
                    // Do stuff here when we are connected/disconnected.
                    break;

                case NetIncomingMessageType.Data:
                    // Retrieve our data and pass it on to the designated handler.
                    Action<NetIncomingMessage> handler;
                    if (Handler.TryGetValue((Packets.Server)msg.ReadInt32(), out handler)) handler(msg);
                    break;

                default:
                    Console.WriteLine("Unhandled Message: " + msg.MessageType + " " + msg.LengthBytes + " bytes " + msg.DeliveryMethod + "|" + msg.SequenceChannel);
                    break;
            }

            // Recycle the message.
            peer.Recycle(msg);

        }

        private static void HandleAuthFailed(NetIncomingMessage msg) {
            Console.WriteLine("Incorrect Username/Password.");
        }

        private static void HandleAuthSuccess(NetIncomingMessage msg) {
            Console.WriteLine("Authenticated!");
        }
    }
}
