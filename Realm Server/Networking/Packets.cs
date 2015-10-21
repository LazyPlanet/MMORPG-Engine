namespace Realm_Server.Networking {
    public static class Packets {

        public enum Client {
            LoginRequest,
            ActivePing,
            AuthenticateClient
        }

        public enum Server {
            AuthFailed,
            AuthSuccess
        }

    }
}
