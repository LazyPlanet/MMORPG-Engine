﻿namespace Realm_Server.Networking {
    public static class Packets {

        public enum Client {
            LoginRequest,
            ActivePing,
            AuthenticateClient,
            ConfirmGuid,
        }

        public enum Server {
            AuthFailed,
            AuthSuccess,
            GuidOK,
            GuidError,
        }

    }
}
