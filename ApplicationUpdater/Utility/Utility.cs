using ApplicationUpdater;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;

namespace Alfa.Windows.ApplicationUpdater
{
    public class Utility
    {
        public static WaitingForm formObj;
        private static Thread formThread;

        /// <summary>
        /// Hex formatındaki string ifadeyi byte dizisine çevirir.
        /// Hata oluşursa oluşan hatayı fırlatır.
        /// </summary>
        /// <param name="hex">Hex string.</param>
        /// <returns>Oluşturulan byte dizisini döndürür.</returns>

        public static byte[] hexToByteArray(string hex)
        {
            try
            {
                return Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string byteArrayToHex(byte[] data)
        {
            try
            {
                string hex = BitConverter.ToString(data);
                return hex.Replace("-", "");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Parametre olarak verilen text dosyasını okur.
        /// Hata oluşursa oluşan hatayı fırlatır.
        /// </summary>
        /// <param name="fileName">İçeriği okunacak dosya.</param>
        /// <returns>Okunan içeriği döndürür.</returns>
        public static string readTextFile(string fileName)
        {
            try
            {
                StreamReader sr = new StreamReader(fileName);
                string line = sr.ReadToEnd();
                sr.Close();
                return line;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Parametre olarak verilen ifadeyi SecureString nesnesine dönüştürür.
        /// Hata oluşursa oluşan hatayı fırlatır.
        /// </summary>
        /// <param name="password">SecureString nesnesine dönüştürülecek string.</param>
        /// <returns>Dönüştürülen nesneyi geri döndürür.</returns>
        public static SecureString convertToSecureStr(string password)
        {
            try
            {
                var secureStr = new SecureString();
                if(password.Length > 0)
                {
                    foreach(var c in password.ToCharArray()) secureStr.AppendChar(c);
                }

                return secureStr;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Veritabanına ulaşılamıyorsa büyük ihtimalle bitlocker'lı bölüm açılmadan mssql çalışmıştır. Bu durumda gerekli sql sorgusu çalıştırılır.
        /// Bu metot gerekli sql komutunu çalıştırmak için kullanılır.
        /// </summary>
        /// <param name="connStr">Sql Connection string'i.</param>
        /// <param name="query">Sql sorgusu.</param>
        /// <returns>Sorgu çalıştırılmış ise true, çalıştırılmamış ise false döndürülür.</returns>
        public static bool execSqlCommand(string connStr, string query)
        {
            try
            {
                using(SqlConnection sqlCon = new SqlConnection(connStr))
                {
                    using(SqlCommand cmd = new SqlCommand(query, sqlCon))
                    {
                        sqlCon.Open();
                        cmd.ExecuteNonQuery();
                    }

                    sqlCon.Close();
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Uygulamanın açıldığı kullanıcının admin olup olmadığını kontrol eder.
        /// Hata oluşursa oluşan hatayı fırlatır.
        /// </summary>
        /// <returns>Uygulamanın açıldığı kullanıcı admin ise true, değilse false döndürür.</returns>
        public static bool checkCurrentUserIsAdmin()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                string role = "BUILTIN\\Administrators";
                return principal.IsInRole(role);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Parametre olarak verilen uygulamayı Yönetici olarak çalıştırır.
        /// Hata oluşursa oluşan hatayı fırlatır.
        /// </summary>
        /// <param name="exeFile">Çalıştırılacak uygulama.</param>
        /// <param name="args">Parametreler. Parametre yok ise null girilir.</param>
        /// <param name="userName">Yönetici kullanıcı adı.</param>
        /// <param name="password">Yönetici parolası.</param>
        public static Process runAppAsAdmin(string exeFile, string args, string userName, string password,
            string workingDirectory)
        {
            try
            {
                SecureString pwString = convertToSecureStr(password);
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = exeFile;
                info.UserName = userName;
                info.Domain = "";
                info.Password = pwString;
                info.UseShellExecute = false;
                if(workingDirectory != null)
                {
                    info.WorkingDirectory = workingDirectory;
                }

                //info.RedirectStandardOutput = true;
                //info.RedirectStandardError = true;
                if(args != null)
                {
                    info.Arguments = args;
                }

                return Process.Start(info);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Runs the given app
        /// </summary>
        /// <param name="exeFile"></param>
        /// <param name="args"></param>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        public static Process runApp(string exeFile, string args, string workingDirectory)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = exeFile;
                info.Domain = "";
                info.UseShellExecute = false;
                if(workingDirectory != null)
                {
                    info.WorkingDirectory = workingDirectory;
                }

                //info.RedirectStandardOutput = true;
                //info.RedirectStandardError = true;
                if(args != null)
                {
                    info.Arguments = args;
                }

                return Process.Start(info);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets Terminal Serial Num
        /// </summary>
        /// <returns></returns>
        public static string GetTSNoFromFile()
        {
            return File.ReadAllText(@"C:\terminalconfig.txt").Trim();
        }

        /// <summary>
        /// Open Waiting Thread
        /// </summary>
        /// <param name="content"></param>
        public static void CallWaitingForm(string content)
        {
            formObj = new WaitingForm(content);
            formThread = new Thread(() => formObj.ShowDialog());
            formThread.Start();
        }

        /// <summary>
        /// Close waiting Thread
        /// </summary>
        public static void CloseWaitingForm()
        {
            if(formThread.IsAlive)
            {
                formThread.Abort();
            }
        }
    }
}