using Android.Runtime;
using NLog;
using NLog.Config;
using NLog.Targets;
using SiamCross.Services.Logging;
using System;
using System.IO;

namespace SiamCross.Droid.Services
{
    [Preserve(AllMembers = true)]
    public class NLogManagerAndroid : ILogManager
    {
        static NLogManagerAndroid()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            ConsoleTarget consoleTarget = new ConsoleTarget();
            config.AddTarget("console", consoleTarget);

            LoggingRule consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(consoleRule);

            FileTarget fileTarget = new FileTarget();

            const string file_name = "SiamServiceLog.txt";
            const Environment.SpecialFolder dir_uid = Environment.SpecialFolder.Personal;
            fileTarget.FileName = Path.Combine(Environment.GetFolderPath(dir_uid), file_name);

            fileTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            LoggingRule fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(fileRule);

            LogManager.Configuration = config;

        }

        public Logger GetLog([System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "")
        {
            string fileName = callerFilePath;

            if (fileName.Contains("/"))
            {
                fileName = fileName.Substring(fileName.LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1);
            }

            Logger logger = LogManager.GetLogger(fileName);
            //return new NLogLoggerAndroid(logger);
            return logger;
        }
    }
}