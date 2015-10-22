using System;
using System.IO;
using System.Net;
using System.Threading;
using Authentication_Server.Logging;
using Authentication_Server.Database;
using Authentication_Server.Networking;
using Authentication_Server.Logic;

namespace Authentication_Server {
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
            var server              = NetServer.Instance();
            server.BindAddress      = IPAddress.Any;
            server.BindPort         = (Int32)Properties.Settings.Default["HostPort"];
            server.MaxConnections   = 100;
            server.MessageHandler   = Handlers.HandleNetMessage;
            logger.Write(String.Format("Server will bind to: {0}:{1} for a maximum of {2} connections.", server.BindAddress, server.BindPort, server.MaxConnections), LogLevels.Informational);
            logger.Write("Initialized Networking Component.", LogLevels.Normal);

            // Retrieve our realm list.
            Data.UpdateRealmList();

            // Set up our timers that will periodically perform some background work.
            var GuidRemovalTimer    = new Timer(new TimerCallback(Data.PurgeOldGuids), null, 0, 900000);        // Every 15 minutes.
            logger.Write("Initialized Timed Logic Components.", LogLevels.Normal);

            // Start the server! We're done loading.
            server.Open();
            logger.Write("Opened Server.", LogLevels.Normal);

            while (Data.Running) {
                Input.Process(Console.ReadLine());
            }

            // We're shutting down!
            server.Close();
            logger.Write("Networking Component Shut Down.", LogLevels.Normal);
        }
    }
}
