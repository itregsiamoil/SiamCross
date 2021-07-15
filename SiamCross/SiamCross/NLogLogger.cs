using NLog;

namespace SiamCross
{
    public class NLogLogger : SiamCross.Services.Logging.ILogger
    {
        private readonly Logger _logger;
        public NLogLogger(Logger logger)
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