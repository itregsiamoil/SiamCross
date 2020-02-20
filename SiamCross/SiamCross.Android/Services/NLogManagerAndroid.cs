using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SiamCross.Services.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.IO;
using ILogger = SiamCross.Services.Logging.ILogger;

namespace SiamCross.Droid.Services
{
    public class NLogManagerAndroid : ILogManager
    {
        public NLogManagerAndroid()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(consoleRule);

            var fileTarget = new FileTarget();


            string directory = Directory.CreateDirectory(
                @"/storage/emulated/0/" + (Path.DirectorySeparatorChar + "SiamService2Log")).FullName;

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

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
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

            var logger = LogManager.GetLogger(fileName);
            //return new NLogLoggerAndroid(logger);
            return logger;
        }
    }
}