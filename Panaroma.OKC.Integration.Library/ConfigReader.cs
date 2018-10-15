using System;
using System.Configuration;

namespace Panaroma.OKC.Integration.Library
{
    public class ConfigReader
    {
        public static string GetAppSettingString(string key,string defaultVal)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultVal;
        }
        public static int GetAppSettinInt(string key,int defaultVal)
        {
            return int.Parse(GetAppSettingString(key, defaultVal.ToString()));
        }
        public static bool GetAppSettingBoolean(string key,bool defaultVal)
        {
            return GetAppSettingString(key, defaultVal.ToString()).ToLower() == "true";
        }

    }
}
