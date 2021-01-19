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

            string directory = @"/storage/emulated/0/" + (Path.DirectorySeparatorChar + "SiamService2Log");
            try
            {
                directory = Directory.CreateDirectory(directory).FullName;
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }


            if (Directory.Exists(directory))
            {
                fileTarget.FileName = Path.Combine(directory, "Log.txt");
            }
            else
            {
                fileTarget.FileName = Path.Combine(Directory.CreateDirectory(directory).FullName, "Log.txt");
            }

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