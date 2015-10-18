using System;
using System.Collections.Generic;
using System.Threading;

namespace Authentication_Server.Database {
    public static class Data {

        private static List<Realm> RealmList = new List<Realm>();
        private static Boolean RealmListMutex = true;

        private static void SetupDBConnection(DBConnection conn) {
            if (conn.Hostname == String.Empty) {
                conn.Hostname       = Properties.Settings.Default["SqlHost"] as String;
                conn.Port           = (Int32)Properties.Settings.Default["SqlPort"];
                conn.DatabaseName   = Properties.Settings.Default["SqlDatabase"] as String;
                conn.Username       = Properties.Settings.Default["SqlUser"] as String;
                conn.Password       = Properties.Settings.Default["SqlPassword"] as String;
            }
        }
        public static void UpdateRealmList(object state) {
            var conn = DBConnection.Instance();

            // Make sure the realmlist is not currently used by any other process.
            // We can't change the collection while it is being used after all.
            while (!RealmListMutex) {
                Thread.Sleep(1);
            }

            // Claim the realm list.
            RealmListMutex = false;

            // Make sure we've got our settings sorted out before moving on.
            SetupDBConnection(conn);

            // Connect to the database and retrieve our data.
            if (conn.Connect()) {
                var reader = conn.ExecuteSqlReader(@"SELECT * from RealmList");
                RealmList.Clear();
                while (reader.Read()) {
                    RealmList.Add(new Realm() {
                        Name        = Convert.ToString(reader["name"]),
                        Hostname    = Convert.ToString(reader["address"]),
                        Port        = Convert.ToInt32(reader["port"]),
                        LastUsed    = reader["lastactive"] == DBNull.Value 
                                        ? DateTime.MinValue 
                                        : Convert.ToDateTime(reader["lastactive"])
                    });

                }
                conn.Close();
                Console.WriteLine(String.Format("Successfully found {0} Realms.", RealmList.Count));
            } else {
                Console.WriteLine("Database Connection failed!");
            }

            // Release the Realm List.
            RealmListMutex = true;
        }

        public static Int32[] AuthenticateUser(String user, String pass) {
            var conn = DBConnection.Instance();
            Int32[] result = new Int32[2];

            // Make sure we've got our settings sorted out before moving on.
            SetupDBConnection(conn);

            if (conn.Connect()) {
                var reader = conn.ExecuteSqlReader(String.Format("SELECT AuthenticateUser('{0}', '{1}') AS success, GetUserId('{0}') AS id", user, pass));
                while (reader.Read()) {
                    result[0] = Convert.ToByte(reader["success"]);
                    result[1] = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]);
                }
            } else {
                result[0] = 1;
                result[1] = 0;
                Console.WriteLine("Database Connection failed!");
            }
            return result;
        }

    }
}
