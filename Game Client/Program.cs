using Game_Client.Networking;
using System;
using System.Windows;

namespace Game_Client {
    class Program {

        [STAThread]
        static void Main(string[] args) {

            // Instantiate our network client and attempt to connect in the background.
            var client = NetClient.Instance();
            client.Hostname         = Properties.Settings.Default["Hostname"] as String;
            client.Port             = (Int32)Properties.Settings.Default["Port"];
            client.MessageHandler   = Handlers.HandleNetMessage;
            client.Connect();

            // Start our form and handle everything from there.
            var app = new Application();
            app.Run(MainWindow.Instance());

            // We've reached the end of our application's life.
            // Au Revoir my friend.
            // All jokes aside, let's unload everything we've managed to load during our run.
            client.Close();

        }
    }
}
