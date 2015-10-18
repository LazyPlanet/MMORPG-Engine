using System;
using System.Net;
using System.Threading;
using Authentication_Server.Database;
using Authentication_Server.Networking;

namespace Authentication_Server {
    class Program {

        private static NetServer            NetServer;
        private static Timer                RealmListTimer;

        static void Main(string[] args) {

            // Configure our networking process.
            NetServer                   = NetServer.Instance();
            NetServer.BindAddress       = IPAddress.Any;
            NetServer.BindPort          = (Int32)Properties.Settings.Default["HostPort"];
            NetServer.MaxConnections    = 100;
            NetServer.MessageHandler    = Handlers.HandleNetMessage;

            // Set up our timers that will periodically perform some background work.
            RealmListTimer = new Timer(new TimerCallback(Data.UpdateRealmList), null, 0, 60000);

            // Start the server! We're done loading.
            NetServer.Open();

            Console.ReadLine();

        }
    }
}
