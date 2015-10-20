// Original class Courtesy of Joyce Cobb.
// https://github.com/Azurebeats/EclipseSharp/blob/master/Server/Logic/Input.cs
// *****************************
using System;
using System.Linq;
using Lidgren.Network;
using System.Collections.Generic;
using Authentication_Server.Logging;
using Authentication_Server.Database;

namespace Server.Logic {
    public static class Input {

        private static Dictionary<String, Action<Object[]>> commands = new Dictionary<String, Action<Object[]>>() {
            { "close",  Shutdown}, { "exit",  Shutdown}, { "shutdown",  Shutdown},
            { "help", Help },
            { "list", List },
            { "update", Update }
        };

        private static Dictionary<String, String> helplist = new Dictionary<String, String>() {
            { "close", "Shuts down the server and saves all currently loaded information to disk." },
            { "exit", "Shuts down the server and saves all currently loaded information to disk." },
            { "shutdown", "Shuts down the server and saves all currently loaded information to disk." },
            { "help", "Provides help for every command available in this program.\n- Use 'help' to get a list of available commands.\n- Use 'help command' to get more detailed information about a command." },
            { "list", "Lists all currently available entries loaded into the server for the specified item.\n- Use list 'type' to get a list of all available items of that type.\n- Available types include: guids, connections" },
            { "update", "Allows the updating of internal data taken from the database for specific items.\n- Use update 'type' to retrieve new data from the database.\n- Available types include: realms" }
        };

        public static void Process(String input) {
            // Parse our input command and pass it on to the appropriate method.
            var data = input.Split(' ');
            Object[] arguments;
            if (data.Length > 1) {
                arguments = new Object[data.Length - 1];
                for (var i = 0; i < data.Length - 1; i++) {
                    arguments[i] = data[i + 1];
                }
            } else {
                arguments = new Object[0];
            }
            Action<Object[]> cmd;
            if (commands.TryGetValue(data[0], out cmd)) {
                cmd(arguments);
            } else {
                var logger = Logger.Instance();
                Console.WriteLine(String.Format("Unknown Command: {0}", data[0]));
            }
        }

        private static void Shutdown(Object[] args) {
            var logger = Logger.Instance();
            logger.Write("Received Shutdown Command.", LogLevels.Normal);
            Data.Running = false;
        }

        private static void List(Object[] args) {
            var logger = Logger.Instance();
            if (args.Length > 0) {
                switch (((String)args[0]).ToLower()) {
                    case "guids":
                        foreach (var item in GUIDStore.Instance().GetList()) {
                            Console.WriteLine(item);
                        }
                    break;
                    case "connections":
                        foreach (var conn in Authentication_Server.Networking.NetServer.Instance().GetPeer().Connections) {
                            Console.WriteLine(String.Format("ID: {0} Remote: {1}", NetUtility.ToHexString(conn.RemoteUniqueIdentifier), String.Format("{0}:{1}", conn.RemoteEndPoint.Address, conn.RemoteEndPoint.Port)));
                        }
                    break;
                }
            } else {
                Console.WriteLine("Unknown list.");
            }
        }

        private static void Help(Object[] args) {
            // Show a generic help when there's no arguments provided.
            // Otherwise show the command's help where available.
            if (args.Length > 0) {
                String help;
                if (!helplist.TryGetValue((String)args[0], out help)) help = "Unknown Command";
                Console.WriteLine(help);
            } else {
                Console.WriteLine(String.Format("All available commands follow. Please use 'help command' to get a more detailed overview.\n{0}", (from c in commands orderby c.Key select c.Key).ToArray().Aggregate((i, j) => i + ", " + j)));
            }
        }

        private static void Update(Object[] args) {
            if (args.Length > 0) {
                switch (((String)args[0]).ToLower()) {

                    case "realms":
                        Console.WriteLine("Updating Realm List.");
                        Data.UpdateRealmList();
                    break;

                }
            } else {
                Console.WriteLine("Unknown request.");
            }
        }
    }
}