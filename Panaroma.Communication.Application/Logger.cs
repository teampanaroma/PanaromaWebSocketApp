using System;
using System.Configuration;
using System.IO;

namespace Panaroma.Communication.Application
{
    public class Logger
    {
        private static string LogFileName = "AppSettingsLog";

        public enum Level
        {
            Fatal = 1,
            Error = 2,
            Info = 3
        }

        private static void Write(string msg, Exception exception, Level lvl)
        {
            try
            {
                int logLevel;
                if(!int.TryParse(ConfigurationManager.AppSettings.Get("AppSettingsLogLevel"), out logLevel))
                {
                    return;
                }

                if((int)lvl > logLevel)
                {
                    return;
                }

                string dllPath = OKCProcesses.AppSettingsFolderPath;
                var currentLogFileName =
                    string.Format("{0}{1}{2}", dllPath, LogFileName, DateTime.Today.ToString("ddMMyy"));
                if(File.Exists(currentLogFileName))
                {
                    var fileInfo = new FileInfo(currentLogFileName);
                    if(fileInfo.Length > 1024 * 1024 * 2)
                    {
                        File.Move(currentLogFileName, currentLogFileName + DateTime.Now.ToShortTimeString());
                    }
                }

                if(!File.Exists(currentLogFileName))
                {
                    File.Create(currentLogFileName);
                }

                File.AppendAllText(currentLogFileName,
                    string.Format("[{0}]-[{1}] {2} {3} {4}", DateTime.Now.ToLongTimeString(), lvl, msg,
                        exception != null ? exception.ToString() : string.Empty, Environment.NewLine));
            }
            catch(Exception ex)
            {
                Info(ex.Message);
            }
        }

        public static void Fatal(string msg)
        {
            Write(msg, null, Level.Fatal);
        }

        public static void Fatal(string msg, Exception ex)
        {
            Write(msg, ex, Level.Fatal);
        }

        public static void Error(string msg)
        {
            Write(msg, null, Level.Error);
        }

        public static void Error(string msg, Exception ex)
        {
            Write(msg, ex, Level.Error);
        }

        public static void Info(string msg)
        {
            Write(msg, null, Level.Info);
        }
    }
}