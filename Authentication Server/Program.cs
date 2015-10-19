using System;
using System.Net;
using System.Threading;
using Authentication_Server.Database;
using Authentication_Server.Networking;

namespace Authentication_Server {
    class Program {

        private static Timer RealmListTimer;
        private static Timer GuidRemovalTimer;

        static void Main(string[] args) {

            // Configure our networking process.
            var server              = NetServer.Instance();
            server.BindAddress      = IPAddress.Any;
            server.BindPort         = (Int32)Properties.Settings.Default["HostPort"];
            server.MaxConnections   = 100;
            server.MessageHandler   = Handlers.HandleNetMessage;

            // Set up our timers that will periodically perform some background work.
            RealmListTimer      = new Timer(new TimerCallback(Data.UpdateRealmList), null, 0, 60000);
            GuidRemovalTimer    = new Timer(new TimerCallback(Data.PurgeOldGuids), null, 0, 900000);

            // Start the server! We're done loading.
            server.Open();

            Console.ReadLine();

        }
    }
}
