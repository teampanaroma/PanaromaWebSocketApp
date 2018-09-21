using System.IO;
using System.Reflection;

namespace Alfa.Windows.ApplicationUpdater
{
    internal class Settings
    {
        public static bool APPUPDATER_TEST_MODE = false;
        public static int UPDATE_CHECK_INTERVAL = 1000;
        public static string TERMINAL_SERIAL_NUM = null;

        public static string APP_SEARCH_PATH = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
            "PanaromaWebSocketApp");

        public static string APP_SEARCH_CONDITION = APP_SEARCH_PATH + "App";
        public static string APPLICATIONMANAGER_FOLDER_PATH = @"C:\ApplicationManager";
        public static string APPLICATIONMANAGER_TYPE_NAME = "ApplicationManager";
        public static string STANDART_APP_NAME = "AppUpdater.exe";
        public static string CONTENT_FILE_NAME = "content.xml";
        public static string SIGNATURE_FILE_NAME = "signature.txt";
        public static string DISABLED_APP_EXTENSION = "disabled";
        public static string ALLOWED_IP_SEPERATOR = ",";
        public static string UPDATER_APP_TYPE = "App0";
        public static string UPDATE_TMP_FOLER_PATH = APP_SEARCH_PATH + UPDATER_APP_TYPE + "\\updates";
        public static string UPDATE_ZIP_FILE_NAME = "update.zip";
        public static string UPDATE_CONTENT_FILE_NAME = "updateconfig.xml";
        public static string UPDATER_WORKING_DIRECTORY = APP_SEARCH_PATH + UPDATER_APP_TYPE;
        public static string UPDATEHELPER_EXE_PATH = UPDATER_WORKING_DIRECTORY + "\\UpdateHelper.exe";
        public static string UPDATE_WEB_API_URL = "http://95.173.163.226:8094/api/AppUpdate";
        public static string UPDATE_WEB_API_IP = "95.173.163.226";
        public static int UPDATE_WEB_API_PORT = 9098;
        public static string UPDATE_DOWNLOAD_TMP_PATH = APP_SEARCH_PATH + UPDATER_APP_TYPE + "\\downloadtmp.zip";
        public static string GET_FTP_INFO = "http://95.173.163.226:8094/api/FtpInfoes";
    }
}