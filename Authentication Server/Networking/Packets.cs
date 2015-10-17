using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication_Server.Networking {
    public static class Packets {

        private enum NetClientPackets {
            LoginRequest,
            ActivePing
        };
        private static Dictionary<NetClientPackets, Action<NetPeer, NetIncomingMessage>> PacketHandler = new Dictionary<NetClientPackets, Action<NetPeer, NetIncomingMessage>>() {
            { NetClientPackets.LoginRequest,    HandleLoginRequest },
            { NetClientPackets.ActivePing,      HandleActivePing },
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
                if (status == NetConnectionStatus.Connected) Console.WriteLine("Remote hail: " + msg.SenderConnection.RemoteHailMessage.ReadString());
                break;

                case NetIncomingMessageType.Data:
                // Retrieve our data and pass it on to the designated handler.
                Action<NetPeer, NetIncomingMessage> handler;
                if (PacketHandler.TryGetValue((NetClientPackets)msg.ReadInt32(), out handler)) handler(peer, msg);
                break;

                default:
                Console.WriteLine("Unhandled Message: " + msg.MessageType + " " + msg.LengthBytes + " bytes " + msg.DeliveryMethod + "|" + msg.SequenceChannel);
                break;
            }

            // Recycle the message.
            peer.Recycle(msg);

        }

        private static void HandleActivePing(NetPeer peer, NetIncomingMessage msg) {
            throw new NotImplementedException();
        }
        private static void HandleLoginRequest(NetPeer peer, NetIncomingMessage msg) {
            throw new NotImplementedException();
        }
    }
}
