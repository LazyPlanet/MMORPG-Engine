using System;
using System.IO;
using System.Net;
using Realm_Server.Logging;
using Realm_Server.Database;
using Realm_Server.Networking;
using Realm_Server.Logic;

namespace Realm_Server {
    class Program {
        static void Main(string[] args) {

            // Initialize our logging.
            var logger = Logger.Instance();
            logger.Level    = (LogLevels)Properties.Settings.Default["LogLevel"];
            logger.File     = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("logfile-{0}", String.Format("{0}.{1}", DateTime.Now.ToString("yyyy-MM-dd"), "log")));
            logger.Write(String.Format("Log File: {0}", logger.File), LogLevels.Debug);
            logger.Write(String.Format("Current log level: {0}", logger.Level.ToString()), LogLevels.Informational);
            logger.Write("Initialized Logging Engine.", LogLevels.Normal);


            // Configure our networking process.
            var server = NetServer.Instance();
            server.BindAddress = IPAddress.Any;
            server.BindPort = (Int32)Properties.Settings.Default["HostPort"];
            server.MaxConnections = 100;
            server.MessageHandler = ServerHandlers.HandleNetMessage;
            logger.Write(String.Format("Server will bind to: {0}:{1} for a maximum of {2} connections.", server.BindAddress, server.BindPort, server.MaxConnections), LogLevels.Informational);
            logger.Write("Initialized Networking Component.", LogLevels.Normal);

            // And the portion that connects to the Auth server.
            var client = NetClient.Instance();
            client.Hostname = Properties.Settings.Default["AuthHostname"] as String;
            client.Port = (Int32)Properties.Settings.Default["AuthPort"];
            client.MessageHandler = ClientHandlers.HandleNetMessage;

            // Load our game data.

            // Start the server! We're done loading.
            server.Open();
            logger.Write("Opened Server.", LogLevels.Normal);

            // Connect to the Authentication Server and tell them we're running!
            client.Connect();

            while (Data.Running) {
                Input.Process(Console.ReadLine());
            }

            // We're shutting down!
            server.Close();
            client.Close();
            logger.Write("Networking Component Shut Down.", LogLevels.Normal);
        }
    }
}
