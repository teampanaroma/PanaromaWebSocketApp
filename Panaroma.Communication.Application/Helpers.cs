using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Panaroma.Communication.Application
{
    public static class Helpers
    {
        public static readonly string ExecutingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        private static readonly bool UserIdAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private const string CommunicationRegistryPath = "SOFTWARE\\Panaroma\\Communication";

        public static class OKCModels
        {
            public const string Verifone = "Verifone";
        }

        public static class DateTimeHelper
        {
            public static string GetDateTime()
            {
                DateTime now = DateTime.Now;
                string shortDateString = now.ToShortDateString();
                string str = " ";
                now = DateTime.Now;
                string shortTimeString = now.ToShortTimeString();
                return shortDateString + str + shortTimeString;
            }
        }

        public class ResourceHelper
        {
            public static BitmapImage GetBitmapImage(string imageName)
            {
                using(MemoryStream memoryStream = new MemoryStream())
                {
                    (new ResourceManager(typeof(Properties.Resources)).GetObject(imageName) as Bitmap)?.Save(
                        memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0L;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
        }

        private static class RawPrinterHelper
        {
            private static string _fileName;

            [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", CharSet = CharSet.Ansi,
                CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter,
                out IntPtr hPrinter, IntPtr pd);

            [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool ClosePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", CharSet = CharSet.Ansi,
                CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool StartDocPrinter(IntPtr hPrinter, int level,
                [MarshalAs(UnmanagedType.LPStruct), In]
                DOCINFOA di);

            [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool EndDocPrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool StartPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool EndPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

            private static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount)
            {
                DOCINFOA di = new DOCINFOA();
                bool flag = false;
                di.pDocName = string.IsNullOrEmpty(_fileName) ? "Panaroma (Text)" : Path.GetFileName(_fileName);
                di.pDataType = "RAW";
                try
                {
                    IntPtr hPrinter;
                    if(OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
                    {
                        if(StartDocPrinter(hPrinter, 1, di))
                        {
                            if(StartPagePrinter(hPrinter))
                            {
                                int dwWritten;
                                flag = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                                EndPagePrinter(hPrinter);
                            }

                            EndDocPrinter(hPrinter);
                        }

                        ClosePrinter(hPrinter);
                    }
                }
                catch
                {
                    flag = false;
                }

                return flag;
            }

            public static bool SendFileToPrinter(string szPrinterName, string szFileName)
            {
                try
                {
                    _fileName = szFileName;
                    FileStream fileStream = new FileStream(szFileName, FileMode.Open);
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    int int32 = Convert.ToInt32(fileStream.Length);
                    byte[] source = binaryReader.ReadBytes(int32);
                    IntPtr num1 = Marshal.AllocCoTaskMem(int32);
                    int startIndex = 0;
                    IntPtr destination = num1;
                    int length = int32;
                    Marshal.Copy(source, startIndex, destination, length);
                    int num2 = SendBytesToPrinter(szPrinterName, num1, int32) ? 1 : 0;
                    Marshal.FreeCoTaskMem(num1);
                    return num2 != 0;
                }
                catch
                {
                    return false;
                }
            }

            public static bool SendStringToPrinter(string szPrinterName, string szString)
            {
                try
                {
                    int length = szString.Length;
                    IntPtr coTaskMemAnsi = Marshal.StringToCoTaskMemAnsi(szString);
                    SendBytesToPrinter(szPrinterName, coTaskMemAnsi, length);
                    Marshal.FreeCoTaskMem(coTaskMemAnsi);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private class DOCINFOA
            {
                [MarshalAs(UnmanagedType.LPStr)]
                public string pDocName;

                [MarshalAs(UnmanagedType.LPStr)]
                public string pOutputFile;

                [MarshalAs(UnmanagedType.LPStr)]
                public string pDataType;
            }
        }

        private static class DocumentHelper
        {
            public static PrintDocument BuildDefaultPrintDocument(Image image, string printerName)
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.DocumentName = "Panaroma (Image)";
                printDocument.DefaultPageSettings.PaperSize = new PaperSize("receipt", image.Width, image.Height);
                printDocument.PrintPage += (s, e) =>
                {
                    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    e.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                    e.Graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                        new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                };
                return printDocument;
            }
        }

        public static class PrinterHelper
        {
            internal static IEnumerable<Printer> GetPrinters()
            {
                return new ManagementObjectSearcher("SELECT * from Win32_Printer").Get().Cast<ManagementBaseObject>()
                    .Select(printer => new Printer()
                    {
                        Name = (string)printer.GetPropertyValue("Name"),
                        Status = (string)printer.GetPropertyValue("Status"),
                        IsDefault = (bool)printer.GetPropertyValue("Default"),
                        IsNetworkPrinter = (bool)printer.GetPropertyValue("Network")
                    }).ToList();
            }

            [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetDefaultPrinter(string name);

            public static bool PrintToHtml(string printerName, string html)
            {
                try
                {
                    using(WebBrowser webBrowser = new WebBrowser())
                    {
                        webBrowser.Navigate("about:blank");
                        HtmlDocument document = webBrowser.Document;
                        if((object)document != null)
                            document.Write(html);
                        webBrowser.Refresh();
                        SetDefaultPrinter(printerName);
                        webBrowser.Print();
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }

            public static bool PrintToImage(string printerName, Image image)
            {
                try
                {
                    PrintDocument printDocument = DocumentHelper.BuildDefaultPrintDocument(image, printerName);
                    printDocument.EndPrint += new PrintEventHandler(PrintDocument_EndPrint);
                    printDocument.Print();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    image?.Dispose();
                }
            }

            private static void PrintDocument_EndPrint(object sender, PrintEventArgs e)
            {
                ((Component)sender)?.Dispose();
            }

            public static bool PrintToDOS(string printerName, string text)
            {
                return RawPrinterHelper.SendStringToPrinter(printerName, text);
            }
        }

        public static class FolderHelper
        {
            public static void IfNotExistsCreatePanaromaSideFolder()
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if(!Directory.Exists(folderPath) || Directory.Exists(Path.Combine(folderPath, "Panaroma")))
                    return;
                Directory.CreateDirectory(Path.Combine(folderPath, "Panaroma", "Communication"));
            }

            public static void CreateFolderIfNotExists(string path)
            {
                if(Directory.Exists(path))
                    return;
                Directory.CreateDirectory(path);
            }

            public static void dosyaYaz(string object1, string object2)
            {
                string directory = Directory.GetCurrentDirectory();
                string dosya_yolu = directory + @"\PanaromaPayment.txt";
                if(!File.Exists(dosya_yolu))
                {
                    File.Create(dosya_yolu);
                }

                StreamWriter sw = File.AppendText(dosya_yolu);
                sw.WriteLine();
                sw.WriteLine("*----------------------------------*");
                sw.WriteLine("İşlem Tarihi: " + DateTime.Now);
                sw.WriteLine(object1);
                sw.WriteLine(object2);
                sw.WriteLine("*----------------------------------*");
                sw.Flush();
                sw.Close();
            }
        }

        public static class FileHelper
        {
            public static void CreateFileIfNotExists(string path)
            {
                if(File.Exists(path))
                    return;
                using(File.Create(path))
                {
                }

                ;
            }

            public static void RemoveOldLogFiles(int day = 30)
            {
                try
                {
                    if(!Directory.Exists(OKCProcesses.FolderPath))
                        return;
                    ((IEnumerable<string>)Directory.GetFiles(OKCProcesses.FolderPath, "*.log",
                            SearchOption.TopDirectoryOnly))
                        .Where(e => DateTime.Compare(File.GetCreationTime(e), DateTime.Now.AddDays(-day)) < 0).ToList()
                        .ForEach(new Action<string>(File.Delete));
                }
                catch
                {
                }
            }

            public static void RemoveOldLogFilesDll(int day = 30)
            {
                try
                {
                    if(!Directory.Exists(OKCProcesses.DllFolderPath))

                        return;
                    ((IEnumerable<string>)Directory.GetFiles(OKCProcesses.DllFolderPath, "*.dat",
                            SearchOption.TopDirectoryOnly))
                        .Where(e => DateTime.Compare(File.GetCreationTime(e), DateTime.Now.AddDays(-day)) < 0).ToList()
                        .ForEach(new Action<string>(File.Delete));
                }
                catch
                {
                }
            }
        }

        public static class RegistryHelper
        {
            private const string cash =
                "Kasa numarası oluşturulmadı. Lütfen uygulamanın kısayoluna sağ tıklayarak Yönetici olarak çalıştır (Run as Administrator) deyiniz.";

            public static void AddApplicationStartupRegistry()
            {
                using(RegistryKey registryKey =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if(registryKey?.GetValue(ExecutingAssemblyName) != null)
                        registryKey.DeleteValue(ExecutingAssemblyName);
                    registryKey?.SetValue(ExecutingAssemblyName, System.Windows.Forms.Application.ExecutablePath);
                }
            }

            public static void IfNotExistsCashIdThenAdd(string cashId, out bool isNeedAdminMode)
            {
                isNeedAdminMode = false;
                using(RegistryKey registryKey =
                    Registry.LocalMachine.OpenSubKey(Path.Combine("SOFTWARE\\Panaroma\\Communication", "Cash"),
                        UserIdAdmin))
                {
                    if(registryKey == null)
                    {
                        if(UserIdAdmin)
                        {
                            Registry.LocalMachine
                                .CreateSubKey(Path.Combine("SOFTWARE\\Panaroma\\Communication", "Cash"), UserIdAdmin)
                                .SetValue("CashId", cashId);
                        }
                        else
                        {
                            isNeedAdminMode = true;
                            MessageBox.Show(
                                "Kasa numarası oluşturulmadı. Lütfen uygulamanın kısayoluna sağ tıklayarak Yönetici olarak çalıştır (Run as Administrator) deyiniz.",
                                "Kasa numarası", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                }
            }

            public static void UpdateCashId(string cashId)
            {
                using(RegistryKey registryKey =
                    RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                {
                    using(registryKey.OpenSubKey("Software\\Classes\\CLSID\\", false))
                    {
                    }

                    ;
                }

                using(RegistryKey registryKey =
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\Panaroma\\Communication", true))
                {
                    if(registryKey == null)
                    {
                        bool isNeedAdminMode;
                        IfNotExistsCashIdThenAdd(cashId, out isNeedAdminMode);
                    }
                    else if(!UserIdAdmin)
                    {
                        InternalCommunication.GetInternalCommunication().HasError = true;
                        InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(
                            new NotificationWindows()
                            {
                                Header = "Kasa Numarası",
                                Description =
                                    "Kasa numarası oluşturulmadı. Lütfen uygulamanın kısayoluna sağ tıklayarak Yönetici olarak çalıştır (Run as Administrator) deyiniz.",
                                NotificationType = NotificationType.Warning,
                                Time = DateTimeHelper.GetDateTime()
                            });
                    }
                    else
                        Registry.LocalMachine
                            .OpenSubKey(Path.Combine("SOFTWARE\\Panaroma\\Communication", "Cash"), true)
                            ?.SetValue("CashId", cashId);
                }
            }

            public static string GetCashId()
            {
                using(RegistryKey registryKey =
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\Panaroma\\Communication", false))
                {
                    if(registryKey == null)
                        return string.Empty;
                    return Registry.LocalMachine
                        .OpenSubKey(Path.Combine("SOFTWARE\\Panaroma\\Communication", "Cash"), false)
                        ?.GetValue("CashId").ToString();
                }
            }
        }
    }
}