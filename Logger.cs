using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    class Logger
    {
        public enum LogLevel { VERBOSE = 2, DEBUG = 3, INFO = 4, WARN = 5, ERROR = 6, ASSERT = 7 };

        private static Logger instance;
        private static readonly object padlock = new object();
        StreamWriter logFile;

        public LogLevel logLevel = LogLevel.ERROR;

        public static Logger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        public Logger()
        {
            DateTime test = DateTime.Now;
            string logFileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".log";
            logFile = new StreamWriter(logFileName);
            logFile.AutoFlush = true;
            logFile.WriteLine("Log file Opened");
        }

        public void WriteLine(LogLevel level, string logStr)
        {
            if ((logFile != null) && (level >= logLevel))
            {
                logFile.WriteLine(logStr);
            }
        }

        public void Dispose()
        {
            if (logFile != null)
            {
                logFile.WriteLine("closing log.");
                logFile.Flush();
                logFile.Dispose();
                logFile = null;
            }
        }

        public void flush()
        {
            if (logFile != null)
            {
                logFile.Flush();
            }
        }
    }
}

