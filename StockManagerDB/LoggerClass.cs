﻿using ESNLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagerDB
{
    internal class LoggerClass
    {
        public static Logger logger = new Logger(Logger.GetDefaultLogPath("ESN", "StockManagerDB", "log"))
        {
            WriteMode = Logger.WriteModes.Write,
            FilenameMode = Logger.FilenamesModes.FileName_LastPrevious,
            PrefixMode = Logger.PrefixModes.RunTime,
        };

        public static void Init()
        {
            if (!logger.Enable())
                throw new InvalidOperationException("Unable to start logger");
        }

        public static bool Write(string data)
        {
            return Write(data, Logger.LogLevels.Debug);
        }

        public static bool Write(string data, Logger.LogLevels logLevel)
        {
            return logger.Write(data, logLevel);
        }

        public static bool Write(string data, string logLevelName)
        {
            return logger.Write(data, logLevelName);
        }
    }
}
