using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Panaroma.Update.Helper
{
    public class ApplicationUpdate
    {
        /// <summary>
        /// Starts Unrar Process for given downloaded zip file
        /// </summary>
        /// <param name="rarFileName"></param>
        /// <param name="password"></param>
        /// <param name="startupPath"></param>
        /// <returns></returns>
        public static bool StartUnrarProcess(string rarFileName, string password, string startupPath)
        {
            try
            {
                if (password == null)
                {
                    password = "";
                }

                var fileExt = Path.GetExtension(rarFileName);
                if (fileExt == ".zip")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(rarFileName))
                    {
                        foreach (ZipArchiveEntry file in archive.Entries)
                        {
                            string completeFileName = Path.Combine(startupPath, file.FullName);
                            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                            if (Path.GetExtension(completeFileName).Length > 0)
                                file.ExtractToFile(completeFileName, true);
                        }
                    }

                    return true;
                }
                else
                {
                    throw new NotImplementedException("Dosya türü desteklenmiyor");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unrar işleminde hata oluştu:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Starts the application
        /// </summary>
        /// <param name="startupPath"></param>
        /// <param name="exeName"></param>
        /// <returns></returns>
        public static bool StartApp(string startupPath, string exeName)
        {
            try
            {
                var str2 = Path.Combine(startupPath, exeName);
                if (File.Exists(str2))
                {
                    Process.Start(str2);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Start App İşleminde Hata Oluştu :" + ex.Message);
                return false;
            }
        }
    }
}