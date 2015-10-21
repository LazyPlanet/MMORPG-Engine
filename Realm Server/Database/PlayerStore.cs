using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Realm_Server.Database {
    public class PlayerStore {

        private Dictionary<String, PlayerData> storage = new Dictionary<String, PlayerData>();
        private Boolean storagemutex = true;

        private static PlayerStore instance;

        public Boolean Contains(String guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = storage.ContainsKey(guid);

            // Release our mutex.
            storagemutex = true;

            return result;
        }
        public void AddPlayer(String guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage.Add(guid, new PlayerData());

            // Release our mutex.
            storagemutex = true;
        }
        public void RemovePlayer(String guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage.Remove(guid);

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

        public void SetDatabaseId(String guid, Int32 id) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage[guid].DatabaseId = id;

            // Release our mutex.
            storagemutex = true;
        }
        public Int32 GetDatabaseId(String guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = storage[guid].DatabaseId;

            // Release our mutex.
            storagemutex = true;

            return result;
        }
        public void SetAuthorizationId(String guid, Guid id) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            storage[guid].AuthorizationId = id;

            // Release our mutex.
            storagemutex = true;
        }
        public Guid GetAuthorizationId(String guid) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = storage[guid].AuthorizationId;

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
