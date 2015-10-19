using Authentication_Server.Database;
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace Authentication_Server.Networking {
    public static class Handlers {

        private static Dictionary<Packets.Client, Action<NetIncomingMessage>> Handler = new Dictionary<Packets.Client, Action<NetIncomingMessage>>() {
            { Packets.Client.LoginRequest,    HandleLoginRequest },
            { Packets.Client.ActivePing,      HandleActivePing },
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
                Action<NetIncomingMessage> handler;
                if (Handler.TryGetValue((Packets.Client)msg.ReadInt32(), out handler)) handler(msg);
                break;

                default:
                Console.WriteLine("Unhandled Message: " + msg.MessageType + " " + msg.LengthBytes + " bytes " + msg.DeliveryMethod + "|" + msg.SequenceChannel);
                break;
            }

            // Recycle the message.
            peer.Recycle(msg);

        }

        private static void HandleActivePing(NetIncomingMessage msg) {
            throw new NotImplementedException();
        }
        private static void HandleLoginRequest(NetIncomingMessage msg) {
            Console.WriteLine(String.Format("Received LoginRequest from {0}", NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier)));

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

                Console.WriteLine(String.Format("Added new GUID to storage: {0}", guid));

                // Send our user the OK and our realmlist.
                Send.AuthSuccess(msg.SenderConnection, guid);
            } else {
                // Login Failed.
                // Tell our user they entered incorrect information.
                Send.AuthFailed(msg.SenderConnection);
            }
        }

    }
}
