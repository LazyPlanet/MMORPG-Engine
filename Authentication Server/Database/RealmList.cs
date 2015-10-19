using System;
using System.Collections.Generic;
using System.Threading;

namespace Authentication_Server.Database {
    public class RealmList {

        private         List<Realm> storage         = new List<Realm>();
        private         Boolean     storagemutex    = true;

        private static  RealmList   rlstore;

        public List<Realm> GetRealms() {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result =  storage;

            // Release our mutex.
            storagemutex = true;

            return result;
        }

        private void Clear() {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage.Clear();

            // Release our mutex.
            storagemutex = true;
        }

        public void Fill(List<Realm> data) {
            Clear();

            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage = data;

            // Release our mutex.
            storagemutex = true;
        }

        public void GetMutex() {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;
        }

        public void ReleaseMutex() {
            storagemutex = true;
        }

        public static RealmList Instance() {
            if (rlstore == null) rlstore = new RealmList();
            return rlstore;
        }

    }

    public class Realm {

        private String rname = String.Empty;
        private String rhost = String.Empty;
        private Int32 rport = 0;
        private DateTime rused = DateTime.MinValue;

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
