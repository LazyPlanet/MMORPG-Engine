using System;
using System.IO;
using System.Threading;

namespace Authentication_Server.Logging {
    public class Logger {

        private         String          lfile;
        private         LogLevels       llevel = LogLevels.Debug;
        private         Boolean         lmutex = true;
        private         StreamWriter    file;

        private static  Logger  instance;

        public String File {
            get { return lfile; }
            set { lfile = value; }
        }

        public LogLevels Level {
            get { return llevel; }
            set { llevel = value; }
        }

        public void Write(String input, LogLevels level) {
            // Check if we even need to handle this level.
            if (level < llevel) return;

            if (file == null) file = System.IO.File.AppendText(lfile);

            // wait for our mutex to clear before we continue.
            while (!lmutex) {
                Thread.Sleep(1);
            }
            // Claim our mutex.
            lmutex = false;

            var msg = String.Format("[{0}][{1}] {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level.ToString(), input);
            Console.WriteLine(msg);
            file.WriteLine(msg);

            // Release our mutex.
            lmutex = true;
        }

        public static Logger Instance() {
            if (instance == null) instance = new Logger();
            return instance;
        }

    }

    public enum LogLevels {
        Debug,
        Informational,
        Normal,
        None
    }
}
