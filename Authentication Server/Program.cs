using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Threading;

namespace Authentication_Server {
    class Program {

        private static NetPeerConfiguration NetConfig = new NetPeerConfiguration("AuthServer");
        private static NetPeer              NetServer;

        private enum NetClientPackets {
            LoginRequest,
            ActivePing
        };
        private static Dictionary<NetClientPackets, Action<NetPeer, NetIncomingMessage>> PacketHandler = new Dictionary<NetClientPackets, Action<NetPeer, NetIncomingMessage>>() {
            { NetClientPackets.LoginRequest,    HandleLoginRequest },
            { NetClientPackets.ActivePing,      HandleActivePing },
        };

        private static List<Realm>          RealmList       = new List<Realm>();
        private static Boolean              RealmListMutex  = true;
        private static Timer                RealmListTimer;

        static void Main(string[] args) {

            // Configure our networking process.
            NetConfig.Port                  = (Int32)Properties.Settings.Default["HostPort"];
            NetConfig.MaximumConnections    = 100;
            NetServer                       = new NetPeer(NetConfig);
            NetServer.RegisterReceivedCallback(new SendOrPostCallback(HandleNetMessage), new SynchronizationContext());

            // Set up our timers that will periodically perform some background work.
            RealmListTimer = new Timer(new TimerCallback(UpdateRealmList), null, 0, 60000);

            // Start the server! We're done loading.
            NetServer.Start();

            Console.ReadLine();

        }

        private static void SetupDBConnection(DBConnection conn) {
            if (conn.Hostname == String.Empty) {
                conn.Hostname       = Properties.Settings.Default["SqlHost"] as String;
                conn.Port           = (Int32)Properties.Settings.Default["SqlPort"];
                conn.DatabaseName   = Properties.Settings.Default["SqlDatabase"] as String;
                conn.Username       = Properties.Settings.Default["SqlUser"] as String;
                conn.Password       = Properties.Settings.Default["SqlPassword"] as String;
            }
        }
        private static void UpdateRealmList(object state) {
            var conn = DBConnection.Instance();

            // Make sure the realmlist is not currently used by any other process.
            // We can't change the collection while it is being used after all.
            while (!RealmListMutex) {
                Thread.Sleep(1);   
            }

            // Claim the realm list.
            RealmListMutex = false;

            // Make sure we've got our settings sorted out before moving on.
            SetupDBConnection(conn);

            // Connect to the database and retrieve our data.
            if (conn.Connect()) {
                var reader = conn.ExecuteSqlReader(@"SELECT * from RealmList");
                RealmList.Clear();
                while (reader.Read()) {
                    RealmList.Add(new Realm() {
                        Name        = reader["name"] as String,
                        Hostname    = reader["address"] as String,
                        Port        = (Int32)reader["port"],
                        LastUsed    = reader["lastactive"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["lastactive"]
                    });

                }
                conn.Close();
                Console.WriteLine(String.Format("Successfully found {0} Realms.", RealmList.Count));
            } else {
                Console.WriteLine("Database Connection failed!");
            }

            // Release the Realm List.
            RealmListMutex = true;
        }
        private static void HandleNetMessage(object state) {
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
