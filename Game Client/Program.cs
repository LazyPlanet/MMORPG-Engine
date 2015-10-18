using Game_Client.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Client {
    class Program {

        static void Main(string[] args) {

            Console.WriteLine("Press Enter to connect.");
            Console.ReadLine();

            var client = NetClient.Instance();
            client.Hostname     = Properties.Settings.Default["Hostname"] as String;
            client.Port         = (Int32)Properties.Settings.Default["Port"];
            client.MessageHandler = new Action<object>((o) => { Console.WriteLine("data received."); });

            if (client.Connect()) {
                Console.WriteLine("Connected!\nEnter your username.");
                var user = Console.ReadLine();
                Console.WriteLine("Enter your password.");
                var pass = Console.ReadLine();
                Console.WriteLine("Sending Login Request.");
                Send.SendLoginRequest(user, pass);
                Console.ReadLine();
            }

            client.Close();

        }
    }
}
