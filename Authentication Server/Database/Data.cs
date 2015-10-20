using System;
using System.Collections.Generic;
using Authentication_Server.Logging;

namespace Authentication_Server.Database {
    public static class Data {

        public static Boolean Running = true;

        private static void SetupDBConnection(DBConnection conn) {
            if (conn.Hostname == String.Empty) {
                conn.Hostname       = Properties.Settings.Default["SqlHost"] as String;
                conn.Port           = (Int32)Properties.Settings.Default["SqlPort"];
                conn.DatabaseName   = Properties.Settings.Default["SqlDatabase"] as String;
                conn.Username       = Properties.Settings.Default["SqlUser"] as String;
                conn.Password       = Properties.Settings.Default["SqlPassword"] as String;
            }
        }
        internal static void UpdateRealmList(object state) {
            var conn = DBConnection.Instance();

            var list    = RealmList.Instance();
            var realms  = new List<Realm>();
            var logger  = Logger.Instance();

            // Make sure we've got our settings sorted out before moving on.
            SetupDBConnection(conn);

            // Connect to the database and retrieve our data.
            if (conn.Connect()) {
                var reader = conn.ExecuteSqlReader(@"SELECT * from RealmList");
                while (reader.Read()) {
                    realms.Add(new Realm() {
                        Name        = Convert.ToString(reader["name"]),
                        Hostname    = Convert.ToString(reader["address"]),
                        Port        = Convert.ToInt32(reader["port"]),
                        LastUsed    = reader["lastactive"] == DBNull.Value 
                                        ? DateTime.MinValue 
                                        : Convert.ToDateTime(reader["lastactive"])
                    });

                }
                conn.Close();
                list.Fill(realms);
                logger.Write(String.Format("Successfully found {0} Realms.", realms.Count), LogLevels.Informational);
            } else {
                logger.Write("Database Connection failed!", LogLevels.Normal);
            }
        }

        internal static void PurgeOldGuids(object state) {
            var list    = GUIDStore.Instance();
            var logger  = Logger.Instance();
            var count   = list.RemoveOld();
            logger.Write(String.Format("Removed {0} expired Guids.", count), LogLevels.Informational);
        }

        public static Int32[] AuthenticateUser(String user, String pass) {
            var conn    = DBConnection.Instance();
            var logger  = Logger.Instance();
            Int32[] result = new Int32[2];

            // Make sure we've got our settings sorted out before moving on.
            SetupDBConnection(conn);

            if (conn.Connect()) {
                var reader = conn.ExecuteSqlReader(String.Format("SELECT AuthenticateUser('{0}', '{1}') AS success, GetUserId('{0}') AS id", user, pass));
                while (reader.Read()) {
                    result[0] = Convert.ToByte(reader["success"]);
                    result[1] = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]);
                }
                conn.Close();
            } else {
                result[0] = 1;
                result[1] = 0;
                logger.Write("Database Connection failed!", LogLevels.Normal);
            }
            return result;
        }

    }
}
