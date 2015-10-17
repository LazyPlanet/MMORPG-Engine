using System;

namespace Authentication_Server {


    public class Realm {

        private String      rname = String.Empty;
        private String      rhost = String.Empty;
        private Int32       rport = 0;
        private DateTime    rused = DateTime.MinValue;

        public String Name {
            get { return rname; }
            set { rname = value; }
        }

        public String Hostname {
            get { return rhost; }
            set { rhost = value; }
        }

        public Int32 Port {
            get { return rport; }
            set { rport = value; }
        }

        public DateTime LastUsed {
            get { return rused; }
            set { rused = value; }
        }

    }
}
