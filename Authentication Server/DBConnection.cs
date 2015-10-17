using System;
using MySql.Data.MySqlClient;


namespace Authentication_Server {
    public class DBConnection {

        private         String          dbname      = String.Empty;
        private         String          dbhost      = String.Empty;
        private         Int32           dbport      = 3306;
        private         String          dbuser      = String.Empty;
        private         String          dbpass      = String.Empty;
        private         MySqlConnection dbconn      = null;
        private static  DBConnection    dbinstance  = null;

        public String DatabaseName {
            get { return dbname; }
            set { dbname = value; }
        }

        public String Hostname {
            get { return dbhost; }
            set { dbhost = value; }
        }

        public Int32 Port {
            get { return dbport; }
            set { dbport = value; }
        }

        public String Username {
            get { return dbuser; }
            set { dbuser = value; }
        }

        public String Password {
            get { return dbpass; }
            set { dbpass = value; }
        }

        public static DBConnection Instance() {
            if (dbinstance == null) dbinstance = new DBConnection();
            return dbinstance;
        }

        public bool Connect() {
            var result = true;
            if (dbconn == null || dbconn.State != System.Data.ConnectionState.Open) {
                if (dbhost == String.Empty || dbname == string.Empty) {
                    result = false;
                } else {
                    // Username and password are optional, so we have to take that into account here.
                    var connstr = String.Format("Server={0}; Port={1}; Database={2}; {3} {4}",
                         dbhost,
                         dbport,
                         dbname,
                         dbuser == string.Empty ? "" : String.Format("UID={0};", dbuser),
                         dbpass == string.Empty ? "" : String.Format("Password={0};", dbpass)
                        );
                    dbconn = new MySqlConnection(connstr);
                    try {
                        dbconn.Open();
                        result = true;
                    } catch (MySqlException e) {
                        Console.WriteLine(e.Message);
                        result = false;
                    }
                }
            }
            return result;
        }

        public MySqlConnection GetConnection() {
            return dbconn;
        }

        public void Close() {
            dbconn.Close();
        }

        private MySqlCommand GetCommand(String query) {
            return new MySqlCommand(query, dbconn);
        }

        public MySqlDataReader ExecuteSqlReader(String query) {
            var cmd = GetCommand(query);
            return cmd.ExecuteReader();
        }

        public Int32 ExecuteSqlQuery(String query) {
            var cmd = GetCommand(query);
            return cmd.ExecuteNonQuery();
        }

    }
}
