using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Realm_Server.Database {
    public class PlayerStore {

        private Dictionary<String, PlayerData> storage = new Dictionary<String, PlayerData>();
        private Boolean storagemutex = true;

        private static PlayerStore instance;

        public Boolean Contains(String netid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = storage.ContainsKey(netid);

            // Release our mutex.
            storagemutex = true;

            return result;
        }
        public void AddPlayer(String netid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage.Add(netid, new PlayerData());

            // Release our mutex.
            storagemutex = true;
        }
        public void RemovePlayer(String netid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage.Remove(netid);

            // Release our mutex.
            storagemutex = true;
        }
        public String GetIdentifierFromGuid(Guid guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = (
                from item in storage
                where item.Value.AuthorizationId.Equals(guid)
                select item.Key
           ).Single().ToString();

            // Release our mutex.
            storagemutex = true;

            return result;
        }

        public void SetDatabaseId(String netid, Int32 id) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage[netid].DatabaseId = id;

            // Release our mutex.
            storagemutex = true;
        }
        public Int32 GetDatabaseId(String netid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = storage[netid].DatabaseId;

            // Release our mutex.
            storagemutex = true;

            return result;
        }
        public void SetAuthorizationId(String netid, Guid guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage[netid].AuthorizationId = guid;

            // Release our mutex.
            storagemutex = true;
        }
        public Guid GetAuthorizationId(String netid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = storage[netid].AuthorizationId;

            // Release our mutex.
            storagemutex = true;

            return result;
        }

        public static PlayerStore Instance() {
            if (instance == null) instance = new PlayerStore();
            return instance;
        }

        private class PlayerData {

            private Int32 dbid;
            private Guid authid;

            public Int32 DatabaseId {
                get { return dbid; }
                set { dbid = value; }
            }

            public Guid AuthorizationId {
                get { return authid; }
                set { authid = value; }
            }

        }
    }    
}
