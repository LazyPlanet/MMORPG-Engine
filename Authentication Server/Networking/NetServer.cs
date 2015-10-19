using System;
using System.Net;
using System.Threading;
using Lidgren.Network;
using System.Collections.Generic;

namespace Authentication_Server.Networking {
    public class NetServer {

        private         IPAddress                   netaddress;
        private         Int32                       netport;
        private         Int32                       netmax;
        private         Action<Object>              nethandler  = null;
        private         NetPeerConfiguration        netconfig   = null;
        private         Lidgren.Network.NetServer   netconn     = null;
        private static  NetServer                   netinstance = null;
        
        public IPAddress BindAddress {
            get { return netaddress; }
            set { netaddress = value; }
        }

        public Int32 BindPort {
            get { return netport; }
            set { netport = value; }
        }

        public Int32 MaxConnections {
            get { return netmax; }
            set { netmax = value; }
        }

        public Action<Object> MessageHandler {
            get { return nethandler; }
            set {
                if (netconn == null) 
                    { nethandler = value;
                } else {
                    throw new Exception("Unable to set MessageHandler after NetServer has been started.");
                }
            }
        }

        public static NetServer Instance() {
            if (netinstance == null) netinstance = new NetServer();
            return netinstance;
        }

        public Boolean Open() {
            var result = true;

            if (nethandler == null || netaddress == null || netport == 0 || netmax == 0) {
                result = false;
            } else {
                if (netconn == null) {
                    netconfig = new NetPeerConfiguration("AuthServer");
                    netconfig.Port = netport;
                    netconfig.MaximumConnections = netmax;
                    netconn = new Lidgren.Network.NetServer(netconfig);
                    netconn.RegisterReceivedCallback(new SendOrPostCallback(nethandler), new SynchronizationContext());
                }
                try {
                    netconn.Start();
                    result = true;
                } catch {
                    result = false;
                }
            }
            return result;
        }

        public void Send(NetConnection conn, NetBuffer data) {
            if (netconn == null) return;
            var msg = netconn.CreateMessage();
            msg.Write(data);
            netconn.SendMessage(msg, conn, NetDeliveryMethod.ReliableOrdered);
        }

        public Lidgren.Network.NetServer GetPeer() {
            return netconn;
        }

        public List<NetConnection> Connections() {
            return netconn == null ? new List<NetConnection>() : netconn.Connections;
        }

        public void Close() {
            if (netconn == null) return;
            netconn.Shutdown("Halting Server.");
        }
    }
}
