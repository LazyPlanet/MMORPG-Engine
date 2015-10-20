using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Authentication_Server.Database {
    public class GUIDStore {

        private List<GUIDData>      storage         = new List<GUIDData>();
        private Boolean             storagemutex    = true;

        private static GUIDStore    gstore;

        public void AddGUID(Int32 id, Guid data) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var item = new GUIDData() { ID = id, GUID = data, TimeStamp = DateTime.UtcNow };
            storage.Add(item);

            // Release our mutex.
            storagemutex = true;
        }

        public Boolean Contains(Guid data) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = false;
            foreach (var item in storage) {
                if (item.GUID == data) result = true;
            }

            // Release our mutex.
            storagemutex = true;

            return result;
        }

        public Int32 GetID(Guid data) {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = 0;
            foreach (var item in storage) {
                if (item.GUID == data) result = item.ID;
            }

            // Release our mutex.
            storagemutex = true;

            return result;
        }

        public String[] GetList() {
            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            var result = (from i in storage
                          let guid = i.GUID
                          let age = DateTime.UtcNow.Subtract(i.TimeStamp).TotalHours
                          select String.Format("GUID: {0} Age: {1}", guid, age)
           ).ToArray();
           
            // Release our mutex.
            storagemutex = true;

            return result;
        }

        public Int32 RemoveOld() {
            var now = DateTime.UtcNow;

            // wait for our mutex to clear before we continue.
            while (!storagemutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            storagemutex = false;

            List<GUIDData> old = new List<GUIDData>();

            foreach (var item in storage) {
                if (now.Subtract(item.TimeStamp).TotalHours > 8) old.Add(item);
            }

            foreach (var item in old) {
                storage.Remove(item);
            }

            // Release our mutex.
            storagemutex = true;

            return old.Count;
        }

        public static GUIDStore Instance() {
            if (gstore == null) gstore = new GUIDStore();
            return gstore;
        }

        private class GUIDData {

            private Guid        dguid;
            private Int32       did;
            private DateTime    dtimestamp;

            public Guid GUID {
                get { return dguid; }
                set { dguid = value; }
            }

            public Int32 ID {
                get { return did; }
                set { did = value; }
            }

            public DateTime TimeStamp {
                get { return dtimestamp; }
                set { dtimestamp = value; }
            }
        }

    }

}
