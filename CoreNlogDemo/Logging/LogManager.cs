using CoreNlogDemo;
using NLog;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Logging
{
    public static class LogManager
    {
        private static ILogger logger = NLog.LogManager.GetCurrentClassLogger();
        public static IDictionary<string, NLog.Logger> loggers = new Dictionary<string, NLog.Logger>();
        private static readonly object logEntryQueueLock = new object();

        static LogManager()
        {
            NLog.Config.ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("request", typeof(RequestDetailsLayoutRenderer));

        }

        public static void Write(string message)
        {
            string reqID = string.Empty;
            string userID = string.Empty;
            string partnerCode = string.Empty;
            try
            {
                WriteToLogger("I", "logs", message);

            }
            catch (Exception ex)
            {
                // do nothing
                try
                {
                    WriteToEventLog("Unable to write Log File : Message : " + message + " : Exception : " + ConvertExceptionToMessage(ex));
                }
                catch
                {

                }
            }
            finally
            {
                reqID = null;
                userID = null;
            }
        }

        public static void WriteToSpecificLog(string logName, string message)
        {
            WriteToLogger("I", logName, message);
        }
        public static void WriteExceptionToSpecificLog(string logName, string message)
        {            
            WriteToLogger("E", logName, message);
        }

        public static void WriteExceptionToSpecificLog(string logName, Exception ex)
        {
            try
            {
                WriteToLogger("E", logName, ConvertExceptionToMessage(ex).ToString());
            }
            catch (Exception subex)
            {
                // do nothing
                try
                {
                    WriteToEventLog("Unable to write Log File : Message : " + ConvertExceptionToMessage(ex).ToString() + " : Exception : " + ConvertExceptionToMessage(subex));
                }
                catch
                {

                }
            }
            finally
            {
            }
        }

        public static void WriteToLogger(string type, string code, string message)
        {
            switch (type)
            {
                case "D":
                    GetLogger(code).Debug(message);
                    break;
                case "E":
                    GetLogger(code).Error(message);
                    break;
                case "F":
                    GetLogger(code).Fatal(message);
                    break;
                case "I":
                    GetLogger(code).Info(message);
                    break;
                case "T":
                    GetLogger(code).Trace(message);
                    break;
                case "W":
                    GetLogger(code).Warn(message);
                    break;
            }

        }

        public static StringBuilder ConvertExceptionToMessage(Exception ex)
        {
            StringBuilder message = new StringBuilder();
            //message=message.Append("Layer: User Interface ");
            //message = message.Append("=============================================================");
            message.Append("ErrorNumber:   ");
            string errorNumber = string.Empty;
            try
            {
                if (
                   ex.GetType().IsAssignableFrom(typeof(System.InvalidCastException)) == false &&
                   ex.GetType().IsAssignableFrom(typeof(System.NullReferenceException)) == false &&
                   ex.GetType().IsAssignableFrom(typeof(System.FormatException)) == false
                   )
                {
                    errorNumber = "1000001";
                }
                else
                {
                    errorNumber = "-1";
                }
            }
            catch (Exception exp)
            {
                // LogManager.Write("Exception occurred in converting error number. returning with default value. Exception ::"+ exp.StackTrace);
                errorNumber = "-1";
            }


            message.Append(errorNumber);

            message = message.Append("Message:  ");
            message = message.AppendLine(ex.Message);
            message = message.Append("StackTrace:  ");
            message = message.Append(ex.StackTrace);
            if (ex.InnerException != null)
            {
                message = message.Append("InnerException:  ");
                message = message.Append(ex.InnerException.Message);
                message = message.Append(ex.InnerException.StackTrace);
                if (ex.InnerException.InnerException != null)
                {
                    message = message.Append("2nd Level InnerException:  ");
                    message = message.Append(ex.InnerException.InnerException.Message);
                    message = message.Append(ex.InnerException.InnerException.StackTrace);
                    if (ex.InnerException.InnerException.InnerException != null)
                    {
                        message = message.Append("3rd Level InnerException:  ");
                        message = message.Append(ex.InnerException.InnerException.InnerException.Message);
                        message = message.Append(ex.InnerException.InnerException.InnerException.StackTrace);
                    }
                }
            }
            message = message.AppendLine(ex.ToString());
            return message;
        }

        public static void WriteToEventLog(string message)
        {
            //if (!EventLog.SourceExists(Startup.StaticConfig.GetSection("ApplicationKey").Value))
            //    EventLog.CreateEventSource(Startup.StaticConfig.GetSection("ApplicationKey").Value, "Application");

            //EventLog.WriteEntry(Startup.StaticConfig.GetSection("ApplicationKey").Value, message);
        }

        private static NLog.Logger GetLogger(string code)
        {
            if (loggers.ContainsKey(code) == false)
            {
                lock (logEntryQueueLock)
                {
                    if (loggers.ContainsKey(code) == false)
                    {
                        loggers.Add(code, NLog.LogManager.GetLogger(code));
                    }
                }
            }
            return loggers[code];
        }
    }

}
