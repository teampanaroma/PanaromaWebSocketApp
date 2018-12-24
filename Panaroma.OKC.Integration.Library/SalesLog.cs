using System;
using System.IO;

namespace Panaroma.OKC.Integration.Library
{
    public class SalesLog
    {
        private static string logPath = "SaleLogs";
        public static bool LogEnable = false;
        private static StreamWriter sw;
        private static DateTime lastLogTime;
        private static DateTime now;

        public static string GetLogPath()
        {
            return logPath;
        }
        public static void SetLogPath(string strLogPath="SaleLogs")
        {
            logPath = strLogPath;
        }
        public static void OpenLogFile()
        {
            if(!LogEnable)
            {
                return;
            }
            if(!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            if(sw==null)
            {
                string str = logPath;
                DateTime now = DateTime.Now;
                sw = File.AppendText(string.Concat(str, "\\", now.ToString("yyyyMMdd"), ".dat"));
            }
            lastLogTime = DateTime.Now;
        }
        public static void CloseLogFile()
        {
            try
            {
                if(sw!=null)
                {
                    sw.Close();
                    sw = null;
                }
            }
            catch(Exception)
            {
            }
        }
        public static void Write(string format,params object[]args)
        {
            if(!LogEnable)
            {
                return;
            }
            if(sw == null)
            {
                OpenLogFile();
                if(sw == null)
                {
                    return;
                }
            }
            //lock(sw)
            //{
                now = DateTime.Now;
                if(now.DayOfYear != lastLogTime.DayOfYear)
                {
                    CloseLogFile();
                    OpenLogFile();
                }
                sw.Write("{0}: ", now.ToString());
                sw.WriteLine(format, args);
                sw.Flush();
                lastLogTime = now;
            //}
        }
    }
}