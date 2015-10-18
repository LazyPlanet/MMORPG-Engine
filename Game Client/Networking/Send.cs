﻿using System;
using Lidgren.Network;

namespace Game_Client.Networking {
    public static class Send {

        private static void SendData(NetBuffer data) {
            var client = NetClient.Instance();
            client.Send(data);
        }

        public static void SendLoginRequest(String user, String pass) {
            var data = new NetBuffer();
            data.Write((Int32)Packets.Client.LoginRequest);
            data.Write(user);
            data.Write(pass);
            SendData(data);
        }

    }
}
