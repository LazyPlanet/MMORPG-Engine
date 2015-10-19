using Lidgren.Network;
using System;
using System.Net;
using System.Threading;

namespace Game_Client.Networking {
    class NetClient {

        private String                      netaddress;
        private Int32                       netport;
        private Action<Object>              nethandler  = null;
        private NetPeerConfiguration        netconfig   = null;
        private Lidgren.Network.NetClient   netconn     = null;
        private static NetClient            netinstance = null;

        public String Hostname {
            get { return netaddress; }
            set { netaddress = value; }
        }

        public Int32 Port {
            get { return netport; }
            set { netport = value; }
        }

        public Action<Object> MessageHandler {
            get { return nethandler; }
            set {
                if (netconn == null) {
                    nethandler = value;
                } else {
                    throw new Exception("Unable to set MessageHandler after NetServer has been started.");
                }
            }
        }

        public static NetClient Instance() {
            if (netinstance == null) netinstance = new NetClient();
            return netinstance;
        }

        public Boolean Connect() {
            var result = true;

            if (nethandler == null || netaddress == null || netport == 0 ) {
                result = false;
            } else {
                if (netconn == null) {
                    netconfig = new NetPeerConfiguration("AuthServer");
                    netconn = new Lidgren.Network.NetClient(netconfig);
                    netconn.RegisterReceivedCallback(new SendOrPostCallback(nethandler), new SynchronizationContext());
                }
                try {
                    netconn.Start();
                    var hail = netconn.CreateMessage("Coming in hot!");
                    var conn = netconn.Connect(netaddress, netport, hail);
                    result = conn.Status == NetConnectionStatus.Connected ? true : false;
                } catch {
                    result = false;
                }
            }
            return result;
        }

        public void Send(NetBuffer data) {
            var msg = netconn.CreateMessage();
            msg.Write(data);
            netconn.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public Lidgren.Network.NetClient GetClient() {
            return netconn;
        }

        public void Close() {
            if (netconn == null) return;
            netconn.Disconnect("Halting Client.");
        }

    }
}
