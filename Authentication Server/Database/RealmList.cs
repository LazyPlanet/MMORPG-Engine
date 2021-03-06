﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Authentication_Server.Database {
    public class RealmList {

        private         Dictionary<Int32, Realm>    storage         = new Dictionary<Int32, Realm>();
        private         Boolean                     storagemutex    = true;

        private static  RealmList   rlstore;

        public Dictionary<Int32, Realm> GetRealms() {
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

        public void Fill(Dictionary<Int32, Realm> data) {
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

        public Boolean UpdateRealmStatus(Int32 id, String remoteidentifier) {
            if (!storage.ContainsKey(id)) return false;

            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage[id].RemoteIdentifier = remoteidentifier;

            // Release our mutex.
            storagemutex = true;

            return true;

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

        private String rname        = String.Empty;
        private String rhost        = String.Empty;
        private Int32 rport         = 0;
        private String ridentifier  = String.Empty;

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

        public String RemoteIdentifier {
            get { return ridentifier; }
            set { ridentifier = value; }
        }

    }
}
