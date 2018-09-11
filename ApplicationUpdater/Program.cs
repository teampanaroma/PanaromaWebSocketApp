using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Alfa.Windows.ApplicationUpdater
{
    class Program
    {
        static XmlSerializer updateXmlSerializer;
        static XmlSerializer appConfigXmlSerializer;
        static IList<string> updateFolders;
        static IList<string> appFolders;
        static List<Installedsoftware> installedSoftwares = new List<Installedsoftware>();

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

        private static int ShowConsoleMinimized = 0;

        /// <summary>
        /// Main Method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {


            FileStream ostrm = null;
            StreamWriter writer = null;
            TextWriter originalOut = Console.Out;
            string logFileName;


            try
            {
                ///LOGGER
                ///TODO: initLogger isimli metod oluştur veya ayrı sınıfa çek. Böyle olmaz
                try
                {
                    string logFileDir =
                        Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
                            "ApplicationUpdater");
                    Directory.CreateDirectory(logFileDir);
                    logFileName = String.Format(logFileDir + "\\APPLICATIONUPDATER" + "{0:dd_MM_yyyy_HH_mm_ss}",
                                      DateTime.Now) + "_LOG.txt";
                    ostrm = new FileStream(logFileName, FileMode.Append, FileAccess.Write);
                    writer = new StreamWriter(ostrm);
                    Console.SetOut(writer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Log dosyası hazırlanamadı, çıktılar konsola yapılacak. Hata: " + ex.Message);
                }
                ///END LOGGER

                Console.WriteLine("########## AppUpdaterDemo Başlatıldı: " + DateTime.Now.ToString() +
                                  "    ############\n");


                updateXmlSerializer = new XmlSerializer(typeof(UpdateConfig));
                appConfigXmlSerializer = new XmlSerializer(typeof(AppConfig));

                SetTerminalSerialNum();
                IsDownloadedAppExist();



                Console.WriteLine("Klasörler Taranıyor\n");



                try
                {
                    Console.WriteLine("###########################\n");
                    Console.WriteLine("Update Check Interval: " + Settings.UPDATE_CHECK_INTERVAL + "\n");

                    //KASA HER AÇILIŞTA KONTROL EDİCEK INTERVAL AÇILIŞTAN SONRA 15 SANİYE OLACAK

                    Thread.Sleep(Settings.UPDATE_CHECK_INTERVAL);

                    appFolders = new DirectoryInfo(Settings.APP_SEARCH_PATH).GetDirectories()
                        .Where(f => !f.Attributes.HasFlag(FileAttributes.System) &&
                                    f.FullName.Contains(Settings.APP_SEARCH_CONDITION))
                        .Select(f => f.FullName)
                        .ToList();
                    Console.WriteLine(appFolders.Count + " app found\n");
                    appFolders.Add(Settings.APPLICATIONMANAGER_FOLDER_PATH);
                    Console.WriteLine("Added to appFolders: " + Settings.APPLICATIONMANAGER_FOLDER_PATH + "\n");

                    CheckSofwares();


                    Console.WriteLine("Create and fill UpdateRequestDTO\n");
                    UpdateRequestDTO updateRequestDTO = new UpdateRequestDTO();
                    updateRequestDTO.TerminalSerialNum = Settings.TERMINAL_SERIAL_NUM;
                    updateRequestDTO.InstalledSoftwares = installedSoftwares.ToArray();
                    Console.WriteLine("Check updates from web api\n");
                    Utility.CallWaitingForm("Güncellemeler Denetleniyor Lütfen Bekleyiniz.");

                    UpdateResponseDTO updateResponse =
                        UpdateCheckClient.checkForUpdates(updateRequestDTO, Settings.UPDATE_WEB_API_URL);

                    Utility.CloseWaitingForm();
                    if (updateResponse.Result && updateResponse.UpdatePackages.Count() > 0)
                    {
                        Utility.CallWaitingForm("Güncellemer İndiriliyor.. Lütfen Cihazınızı Kapatmayınız..");
                        Console.WriteLine("Result true\n");
                        IList<FtpInfo> userInformation = UpdateCheckClient.GetFtpUserInformation(Settings.GET_FTP_INFO);
                        foreach (Updatepackage updatePackage in updateResponse.UpdatePackages)
                        {
                            Console.WriteLine("//////////////////////////////////////////\n");
                            Console.WriteLine("download new update\n ");
                            //byte[] newZipFileArr = UpdateCheckClient.downloadUpdate(updatePackage.AppUrl, Settings.UPDATE_DOWNLOAD_TMP_PATH);
                            byte[] newZipFileArr = UpdateCheckClient.DownloadFtpFile(updatePackage.AppUrl,
                                Settings.UPDATE_DOWNLOAD_TMP_PATH, userInformation.First().UserName,
                                userInformation.First().Password);
                            Console.WriteLine("new update downloaded\n");
                            if (newZipFileArr == null || updatePackage.AppType == null)
                            {
                                Console.WriteLine("new zip file arr or apptype null\n");
                                continue;
                            }

                            string updateTmpPath = Settings.UPDATE_TMP_FOLER_PATH + "\\" + updatePackage.AppType;
                            Console.WriteLine("update tmp path: " + updateTmpPath + "\n");
                            string updatedZipFileName = updateTmpPath + "\\" + Settings.UPDATE_ZIP_FILE_NAME;
                            Console.WriteLine("updated zip file name: " + updatedZipFileName + "\n");
                            if (Directory.Exists(updateTmpPath))
                            {
                                Directory.Delete(updateTmpPath, true);
                            }

                            Directory.CreateDirectory(updateTmpPath);
                            Console.WriteLine("Directory deleted and created: " + updateTmpPath + "\n");
                            File.WriteAllBytes(updatedZipFileName, newZipFileArr);
                            Console.WriteLine("start update config \n");
                            UpdateConfig updateConfig = new UpdateConfig();
                            updateConfig.Description = updatePackage.Description;
                            updateConfig.Enabled = updatePackage.Enabled;
                            updateConfig.Name = updatePackage.AppName;
                            updateConfig.Type = updatePackage.AppType;
                            updateConfig.Version = updatePackage.Version;
                            Console.WriteLine("End Update Config\n");
                            FileStream updateConfigFile =
                                File.Create(updateTmpPath + "\\" + Settings.UPDATE_CONTENT_FILE_NAME);

                            updateXmlSerializer.Serialize(updateConfigFile, updateConfig);
                            updateConfigFile.Close();
                            Console.WriteLine("update config has been written to: " + updateTmpPath + "\\" +
                                              Settings.UPDATE_CONTENT_FILE_NAME + "\n");
                            Console.WriteLine("//////////////////////////////////////////\n");

                        }


                    }

                    Utility.CloseWaitingForm();
                    RunInitalApplication();
                }
                catch (Exception ex)
                {
                    Utility.CloseWaitingForm();
                    RunInitalApplication();
                    Console.WriteLine("Update Check Loop Error: " + ex.Message + "\n");
                }

            }
            catch (Exception ex)
            {
                Utility.CloseWaitingForm();
                RunInitalApplication();
                Console.WriteLine("Hata oluştu: " + ex.Message + "\n");
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
            }

            Application.Exit();
        }

        /// <summary>
        /// Terminal Serial Number Set et.
        /// </summary>
        private static void SetTerminalSerialNum()
        {
            try
            {
                if (!File.Exists(@"C:\terminalconfig.txt"))
                {
                    string result = Microsoft.VisualBasic.Interaction.InputBox(
                        "Lütfen Bir defaya mahsus olmak üzere Cihazınız Üzerinde bulunan Mali Sicil numarasını dikkatlice giriniz",
                        "");
                    File.Create(@"C:\terminalconfig.txt").Dispose();
                    TextWriter sw = new StreamWriter(@"C:\terminalconfig.txt", true);
                    sw.WriteLine(result);
                    Settings.TERMINAL_SERIAL_NUM = result;
                    sw.Close();
                }
                else
                {
                    Settings.TERMINAL_SERIAL_NUM = Utility.GetTSNoFromFile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TerminalSerial Number Okunamadı: " + ex.Message);
                RunInitalApplication();

            }

        }

        /// <summary>
        /// İndirilmiş bir güncelleme varmı
        /// </summary>
        private static void IsDownloadedAppExist()
        {
            try
            {
                updateFolders = new DirectoryInfo(Settings.UPDATE_TMP_FOLER_PATH).GetDirectories()
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.System))
                    .Select(f => f.FullName)
                    .ToList();
                Console.WriteLine("Bulunan Klasör sayısı : " + updateFolders.Count + "\n");
                if (updateFolders.Count > 0)
                {
                    RunUpdater();
                }

            }
            catch (Exception ex)
            {
                Utility.CloseWaitingForm();
                RunInitalApplication();
                Console.WriteLine("Update Error: " + ex.Message + "\n");
            }
        }

        /// <summary>
        /// İndirilen Güncelleştirmeleri Açılışta yükleyen fonksiyondur
        /// </summary>
        private static void RunUpdater()
        {

            foreach (string updateDir in updateFolders)
            {

                try
                {
                    Console.WriteLine("#####################\n");
                    Console.WriteLine("Klasör: " + updateDir + "\n");

                    Console.WriteLine("Config Dosyası : " + updateDir + "\\" + Settings.UPDATE_CONTENT_FILE_NAME +
                                      "\n");
                    StreamReader updateConfigFile =
                        new StreamReader(updateDir + "\\" + Settings.UPDATE_CONTENT_FILE_NAME);
                    UpdateConfig updateConfig = (UpdateConfig) updateXmlSerializer.Deserialize(updateConfigFile);
                    updateConfigFile.Close();
                    Console.WriteLine("Config Dosyası Okundu\n");
                    string newAppPath = Settings.APP_SEARCH_PATH + updateConfig.Type;
                    Console.WriteLine("Yeni Uygulama Dizini: " + newAppPath + "\n");

                    if (updateConfig.Type == Settings.APPLICATIONMANAGER_TYPE_NAME)
                    {
                        newAppPath = Settings.APPLICATIONMANAGER_FOLDER_PATH;
                        Console.WriteLine("Yeni Uygulama dizini değişti: " + newAppPath + "\n");
                    }

                    string newAppContentFilePath = newAppPath + "\\" + Settings.CONTENT_FILE_NAME;
                    Console.WriteLine("Yeni uygulamanın content file dizini: " + newAppContentFilePath + "\n");
                    if (!Directory.Exists(newAppPath))
                    {
                        Console.WriteLine("Yeni uygulamaya ait dizin bulunamadı\n");
                        Directory.CreateDirectory(newAppPath);

                        Console.WriteLine("Uygulama Config Başlatıldı\n");
                        AppConfig appConfig = new AppConfig();
                        appConfig.Description = updateConfig.Description;
                        appConfig.Enabled = updateConfig.Enabled;
                        appConfig.Name = updateConfig.Name;
                        appConfig.Version = updateConfig.Version;
                        FileStream newContentFS = File.Create(newAppContentFilePath);
                        appConfigXmlSerializer.Serialize(newContentFS, appConfig);
                        newContentFS.Close();
                        Console.WriteLine("Uygulama config bitirildi\n");

                    }
                    else
                    {
                        Console.WriteLine("Uygulama Dizini mevcut\n");
                        AppConfig appConfig = null;
                        if (!File.Exists(newAppContentFilePath))
                        {
                            appConfig = new AppConfig();
                            Console.WriteLine("App Config bulunamadı, oluşturuldu\n");
                        }
                        else
                        {
                            Console.WriteLine("App Config bulundu\n");
                            StreamReader contentFile = new StreamReader(newAppContentFilePath);
                            appConfig = (AppConfig) appConfigXmlSerializer.Deserialize(contentFile);
                            contentFile.Close();

                            Console.WriteLine("App Config Okundu\n");
                            if (File.Exists(newAppContentFilePath))
                            {
                                File.Delete(newAppContentFilePath);

                                Console.WriteLine(" App Config Silindi\n");

                            }

                            if (appConfig.Name != updateConfig.Name)
                            {
                                Console.WriteLine("app config name and update config name not same\n");
                                if (Directory.Exists(newAppPath))
                                {
                                    Directory.Delete(newAppPath, true);
                                }

                                Directory.CreateDirectory(newAppPath);
                                appConfig = new AppConfig();
                                Console.WriteLine("deleted all old sw directory. Created new appconfig\n");

                            }
                        }

                        Console.WriteLine("Start appConfig parameters\n");
                        appConfig.Description = updateConfig.Description;
                        appConfig.Enabled = updateConfig.Enabled;
                        appConfig.Name = updateConfig.Name;
                        appConfig.Version = updateConfig.Version;

                        Console.WriteLine("Start appconfig write\n");
                        FileStream newContentFile = File.Create(newAppContentFilePath);
                        appConfigXmlSerializer.Serialize(newContentFile, appConfig);
                        newContentFile.Close();
                        Console.WriteLine("new appconfig file has been written\n");
                    }


                    //_RarFileName = args[0];                     //Download edilen dosyanın pathi
                    //_StartupPath = args[1];                     //yeni dosyaların kopyalanacağı dizin
                    //_ExeName = args[2];                         //yeni dosyaların kopyalandığı dizinde çalıştırılacak uygulama
                    //_RunNewExe = Convert.ToBoolean(args[3]);    //yeni kopyalanan uygulama çalıştırılsınmı? True veya False
                    string updateHelperArgs = "\"" + updateDir + "\\" + Settings.UPDATE_ZIP_FILE_NAME + "\" " + "\"" +
                                              newAppPath + "\" " + "\"" + Settings.STANDART_APP_NAME + "\" " + "\"" +
                                              Boolean.FalseString + "\"";

                    Console.WriteLine("UpdateHelper.exe args: " + updateHelperArgs + "\n");
                    if (updateConfig.Type == Settings.UPDATER_APP_TYPE)
                    {
                        #region app updater güncelleme bug ı çözülene kadar kapalı

                        //OMG IT'S ME ! 
                        updateHelperArgs = "\"" + updateDir + "\\" + Settings.UPDATE_ZIP_FILE_NAME + "\" " + "\"" +
                                           newAppPath + "\" " + "\"" + Settings.STANDART_APP_NAME + "\" " + "\"" +
                                           Boolean.TrueString + "\"";

                        Console.WriteLine("Updating appupdater. New Args: " + updateHelperArgs + "\n");
                        Utility.runApp(Settings.UPDATEHELPER_EXE_PATH, updateHelperArgs,
                            Settings.UPDATER_WORKING_DIRECTORY);
                        Environment.Exit(0);

                        #endregion
                    }
                    else
                    {
                        Console.WriteLine("Starting UpdateHelper\n");
                        Process helperProcess = Utility.runApp(Settings.UPDATEHELPER_EXE_PATH, updateHelperArgs,
                            Settings.UPDATER_WORKING_DIRECTORY);
                        helperProcess.WaitForExit();
                        Console.WriteLine("UpdateHelper worked\n");
                        Console.WriteLine("###########################\n");
                    }
                }

                catch (Exception ex)
                {
                    Utility.CloseWaitingForm();
                    Console.WriteLine("Update Loop Error: " + ex.Message + "\n");
                }
            }

        }

        /// <summary>
        /// Yüklü olan yazılımları her açılışta kontrol eder, Güvenlik ayarlarını belirler
        /// </summary>
        private static void CheckSofwares()
        {
            for (int i = 0; i < appFolders.Count - 1; i++)
            {
                Console.WriteLine("*************************");
                try
                {
                    string contentFilePath = appFolders[i] + "\\" + Settings.CONTENT_FILE_NAME;

                    Console.WriteLine("Check xml file\n");
                    StreamReader contentFile = new StreamReader(contentFilePath);
                    AppConfig appConfig = (AppConfig) appConfigXmlSerializer.Deserialize(contentFile);
                    contentFile.Close();
                    Console.WriteLine("Content file has been read\n");

                    Console.WriteLine("appfolder index: " + i + "\n");
                    string[] appFolderPath = appFolders[i].Split('\\');
                    string appType = appFolderPath[appFolderPath.Length - 1];
                    string appFilePath = appFolders[i] + "\\" + appConfig.MainAppName;
                    Console.WriteLine("App File Path: " + appFilePath + "\n");
                    //string contentFilePath = appFolders[i] + "\\" + Settings.CONTENT_FILE_NAME;
                    string signatureFilePath = appFolders[i] + "\\" + Settings.SIGNATURE_FILE_NAME;
                    Console.WriteLine("Signature File Path: " + signatureFilePath + "\n");
                    Console.WriteLine("Content File Path: " + contentFilePath + "\n");
                    if (!File.Exists(contentFilePath))
                    {
                        Console.WriteLine("content dosyası bulunamadı\n");
                        continue;
                    }

                    if (!File.Exists(appFilePath) && !File.Exists(appFilePath + Settings.DISABLED_APP_EXTENSION))
                    {
                        Console.WriteLine(appFilePath + " bulunamadı");
                        continue;
                    }

                    if (!File.Exists(signatureFilePath))
                    {
                        Console.WriteLine("signature dosyası bulunamadı\n");
                        continue;
                    }

                    Console.WriteLine("Get hash\n");
                    SHA256Managed sha = new SHA256Managed();
                    byte[] hash = null;
                    if (File.Exists(appFilePath))
                    {
                        Console.WriteLine("appfile exists\n");
                        hash = sha.ComputeHash(File.ReadAllBytes(appFilePath));
                    }
                    else
                    {
                        Console.WriteLine("appfile not exists, getting disabled app's hash\n");
                        hash = sha.ComputeHash(File.ReadAllBytes(appFilePath + Settings.DISABLED_APP_EXTENSION));
                    }

                    Console.WriteLine("calculated hash: " + Utility.byteArrayToHex(hash) + "\n");
                    string signature = Utility.readTextFile(signatureFilePath);
                    Console.WriteLine("signaturefrom file: " + signature + "\n");
                    string hashStr = Utility.byteArrayToHex(hash);
                    string allowedIpAddresses = "";
                    Console.WriteLine("Start Firewall settings\n");
                    for (int j = 0; j < appConfig.FwRules.Length; j++)
                    {
                        AppConfigFwRule fwRule = appConfig.FwRules[j];
                        allowedIpAddresses += fwRule.Ip + Settings.ALLOWED_IP_SEPERATOR;
                    }

                    Console.WriteLine("allowed addresses: " + allowedIpAddresses + "\n");
                    Installedsoftware installedSw = new Installedsoftware();
                    installedSw.AllowedIpAddresses = allowedIpAddresses;
                    installedSw.AppHash = hashStr;
                    installedSw.AppName = appConfig.Name;
                    installedSw.AppSignature = signature;
                    installedSw.AppType = appType;
                    installedSw.AppVersion = appConfig.Version;
                    installedSw.Description = appConfig.Description;
                    installedSw.Enabled = appConfig.Enabled;
                    installedSoftwares.Add(installedSw);
                    Console.WriteLine("installed software added to list\n");
                }
                catch (Exception ex)
                {
                    Utility.CloseWaitingForm();
                    RunInitalApplication();
                    Console.WriteLine("Check content file exception. Index num: " + i + "\n" + ex.Message);
                }

                Console.WriteLine("*************************\n");
            }
        }

        /// <summary>
        /// Kasanın Hangi uygulamayı çalıştıracağını belirler
        /// </summary>
        private static void RunInitalApplication()
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;

            //Ön ofis yüklü ise aç yok ise servis uygulmasını çalıştır.
            if (Directory.Exists("D:\\App3"))
            {
                XmlSerializer xmlserializer = new XmlSerializer(typeof(AppConfig));
                StreamReader reader = new StreamReader("D:\\App3\\content.xml");
                AppConfig appConfig = (AppConfig) xmlserializer.Deserialize(reader);
                reader.Close();

                p.StartInfo.FileName = "D:\\App3\\" + appConfig.MainAppName;
                p.Start();

            }
            else
            {
                p.StartInfo.FileName = "D:\\App2\\ServiceProject.exe";
                p.Start();
            }
        }

    }
}