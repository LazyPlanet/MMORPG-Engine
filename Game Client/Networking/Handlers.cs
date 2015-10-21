using System;
using Lidgren.Network;
using System.Collections.Generic;
using Game_Client.Database;
using System.Linq;

namespace Game_Client.Networking {
    public static class Handlers {

        private static Dictionary<Packets.Server, Action<NetIncomingMessage>> Handler = new Dictionary<Packets.Server, Action<NetIncomingMessage>>() {
            { Packets.Server.AuthSuccess,   HandleAuthSuccess },
            { Packets.Server.AuthFailed,    HandleAuthFailed },
        };

        public static void HandleNetMessage(object state) {
            var peer    = state as NetPeer;
            var msg     = peer.ReadMessage();

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
                    if (status == NetConnectionStatus.Connected) {
                    // If we're connecting to anything but the auth server, we'll need to send them a hello to authenticate ourselves with.
                    var authhost = String.Format("{0}:{1}", Properties.Settings.Default["Hostname"] as String, (Int32)Properties.Settings.Default["Port"]);
                    var curhost = String.Format("{0}:{1}", NetClient.Instance().Hostname, NetClient.Instance().Port);
                    if (!authhost.Equals(curhost)) {
                            Send.AuthenticateClient(Data.MyGUID);
                        }
                    }
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
            MainWindow.Instance().Dispatcher.Invoke(()=> {
                MainWindow.Instance().ShowLoginWarning("Incorrect Username/Password!");
            });
        }

        private static void HandleAuthSuccess(NetIncomingMessage msg) {
            Data.MyGUID = Guid.Parse(msg.ReadString());
            var count   = msg.ReadInt32();
            Data.RealmList.Clear();
            Data.RealmList.AddRange(
                from i in Enumerable.Range(0, count)
                let name        = msg.ReadString()
                let hostname    = msg.ReadString()
                let port        = msg.ReadInt32()
                let online      = msg.ReadBoolean()
                select new Realm() { Name = name, Hostname = hostname, Port = port, Online = online }
            );
            MainWindow.Instance().Dispatcher.Invoke(()=> {
                MainWindow.Instance().ShowRealmList();
            });
        }
    }
}
