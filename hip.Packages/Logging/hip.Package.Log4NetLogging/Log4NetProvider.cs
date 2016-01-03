using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Web;

using log4net;
using hip.Infrastructure.Interfaces;

namespace hip.Package.Log4NetLogging
{
    public class Log4NetProvider : ILoggingProvider
    {
        private readonly string DEFAULT_LOGGER = "SYS";
        private ILog _logger;

        public Log4NetProvider()
        {
            GlobalContext.Properties["appname"] = "TutorGroup.Api";

            try
            {
                var codeBaseUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var fileInfo = new System.IO.FileInfo(codeBaseUri.AbsolutePath);
                if (fileInfo.DirectoryName != null)
                {
                    var newConfigFilePath = System.IO.Path.Combine(fileInfo.DirectoryName, "ConfigurationFiles", "Logging/log4net/log4net.config");

                    if (System.IO.File.Exists(newConfigFilePath))
                    {
                        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(newConfigFilePath));
                    }
                    else
                    {
                        throw new Exception("Log4net config file missing Error:");
                    }
                }
                else
                {
                    throw new Exception("Log4net config file missing Error:");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Log4net Initialize Error:" + ex.Message, ex);
            }

            GetLogger(DEFAULT_LOGGER);
        }

        public Log4NetProvider(string logpath, string loggerName = "")
        {
            GlobalContext.Properties["appname"] = "TutorGroup.Api";

            if (logpath != string.Empty)
            {
                ThreadContext.Properties["logpath"] = logpath;
            }
            else
            {
                ThreadContext.Properties["logpath"] = "TutorGroup.Api\\";
            }

            if (string.IsNullOrEmpty(loggerName))
                loggerName = DEFAULT_LOGGER;

            try
            {
                var codeBaseUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var fileInfo = new System.IO.FileInfo(codeBaseUri.AbsolutePath);
                if (fileInfo.DirectoryName != null)
                {
                    var newConfigFilePath = System.IO.Path.Combine(fileInfo.DirectoryName, "ConfigurationFiles", "Logging/log4net/log4net.config");

                    if (System.IO.File.Exists(newConfigFilePath))
                    {
                        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(newConfigFilePath));
                    }
                    else
                    {
                        throw new Exception("Log4net config file missing Error:");
                    }
                }
                else
                {
                    throw new Exception("Log4net config file missing Error:");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Log4net Initialize Error:" + ex.Message, ex);
            }

            GetLogger(loggerName);
            //GetLogger(HttpContext.Current.ApplicationInstance.GetType());
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            _logger.Debug(message, exception);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            _logger.Fatal(message, exception);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            _logger.Info(message, exception);
        }

        public void Trace(string message)
        {
            _logger.Info("Trace:" + message);
        }

        public void Trace(string message, Exception exception)
        {
            _logger.Info("Trace:" + message, exception);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Warn(string message, Exception exception)
        {
            _logger.Warn(message, exception);
        }

        private void GetLogger(string loggerName)
        {
            ThreadContext.Properties["loggerName"] = loggerName;
            _logger = LogManager.GetLogger(loggerName);
        }
    }
}
