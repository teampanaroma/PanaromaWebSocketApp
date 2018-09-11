using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Panaroma.Update.Helper
{
    internal class Program
    {
        /// <summary>
        /// Açılışta test moduna göre pencere gizleme veya gösterme için gerekli metod.
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Açılışta test moduna göre pencere gizleme veya gösterme için gerekli metod.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static string _StartupPath;
        private static string _ExeName;
        private static string _RarFileName;
        private static bool _RunNewExe;

        public static readonly string AppSettingsFolderPath =
            Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "UpdateHelper");

        private static void Main(string[] args)
        {
            FileStream ostrm = null;
            StreamWriter writer = null;
            TextWriter originalOut = Console.Out;
            string logFileName;
            int exitCode = 0;
            try
            {
                ///LOGGER
                try
                {
                    //string logFileDir = "D:\\POS\\Logs\\UpdateHelper";
                    Directory.CreateDirectory(AppSettingsFolderPath);
                    logFileName =
                        string.Format(AppSettingsFolderPath + "\\UPDATEHELPER" + "{0:dd_MM_yyyy_HH_mm_ss}",
                            DateTime.Now) +
                        "_LOG.txt";
                    ostrm = new FileStream(logFileName, FileMode.Append, FileAccess.Write);
                    writer = new StreamWriter(ostrm);
                    Console.SetOut(writer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Log dosyası hazırlanamadı, çıktılar konsola yapılacak. Hata: " + ex.Message);
                }
                ///END LOGGER

                Console.WriteLine("############# APPSTARTER LOG " + DateTime.Now.ToString() + " #############");
                Console.WriteLine("Uygulama başlıyor");

                IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, 0);
                }

                updatePrc(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata oluştu: " + ex.Message);
                exitCode = -1;
            }
            finally
            {
                Console.WriteLine("#############  " + DateTime.Now.ToString() + " #############");
                Console.SetOut(originalOut);
                try
                {
                    writer.Close();
                    ostrm.Close();
                }
                catch
                {
                }

                Environment.Exit(exitCode);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        private static void updatePrc(string[] args)
        {
            if (args == null || args.Length != 4)
            {
                throw new Exception("Update Argümanları Hatalı");
            }

            Thread.Sleep(1000);
            _RarFileName = args[0]; //Download edilen dosyanın pathi
            _StartupPath = args[1]; //yeni dosyaların kopyalanacağı dizin
            _ExeName = args[2]; //yeni dosyaların kopyalandığı dizinde çalıştırılacak uygulama
            _RunNewExe = Convert.ToBoolean(args[3]); //yeni kopyalanan uygulama çalıştırılsınmı? True veya False

            Console.WriteLine("Zip File: " + _RarFileName);
            Console.WriteLine("StartUp Path: " + _StartupPath);
            Console.WriteLine("Exe Name: " + _ExeName);
            Console.WriteLine("Run New Exe: " + args[3]);

            Console.WriteLine("Start Unzip");
            var result = ApplicationUpdate.StartUnrarProcess(_RarFileName, null, _StartupPath);

            if (!result)
                throw new Exception("Unzip Başarısız");

            string[] tmpPathArr = _RarFileName.Split('\\');
            string tmpPath = tmpPathArr[0];
            for (int i = 1; i < tmpPathArr.Length - 1; i++)
            {
                tmpPath += "\\" + tmpPathArr[i];
            }

            Console.WriteLine("remove directory: " + tmpPath);
            if (Directory.Exists(tmpPath))
            {
                Console.WriteLine("Directory exists");
                Directory.Delete(tmpPath, true);
            }

            if (_RunNewExe)
            {
                Console.WriteLine("Start Exe File");
                while (!ApplicationUpdate.StartApp(_StartupPath, _ExeName)) ;
            }
        }
    }
}