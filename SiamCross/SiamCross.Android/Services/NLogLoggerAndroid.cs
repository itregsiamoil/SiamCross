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
using NLog;
using SiamCross.Services.Logging;

namespace SiamCross.Droid.Services
{
    public class NLogLoggerAndroid : SiamCross.Services.Logging.ILogger
    {
        private Logger _logger;
        public NLogLoggerAndroid(Logger logger)
        {
            _logger = logger;
        }

        public void Debug(string text, params object[] args)
        {
            _logger.Debug(text, args);
        }

        public void Error(string text, params object[] args)
        {
            _logger.Error(text, args);
        }

        public void Fatal(string text, params object[] args)
        {
            _logger.Fatal(text, args);
        }

        public void Info(string text, params object[] args)
        {
            _logger.Info(text, args);
        }

        public void Trace(string text, params object[] args)
        {
            _logger.Trace(text, args);
        }

        public void Warn(string text, params object[] args)
        {
            _logger.Warn(text, args);
        }
    }
}