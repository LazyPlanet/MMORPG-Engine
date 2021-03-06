﻿using System;

namespace Game_Client.Database {
    public class Realm {

        private String rname        = String.Empty;
        private String rhost        = String.Empty;
        private Int32 rport         = 0;
        private Boolean ronline     = false;

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

        public Boolean Online {
            get { return ronline; }
            set { ronline = value; }
        }

    }
}
