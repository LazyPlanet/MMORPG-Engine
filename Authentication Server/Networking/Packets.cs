namespace Authentication_Server.Networking {
    public static class Packets {

        public enum Client {
            LoginRequest,
            ActivePing
        }

        public enum Server {
            AuthFailed,
            AuthSuccess
        }

    }
}
