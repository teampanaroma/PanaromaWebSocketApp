using System;
using System.Drawing;
using System.IO;

namespace Panaroma.Communication.Application
{
    public class PrinterWorker : Worker, IWorker
    {
        public PrinterWorker(TcpCommand tcpCommand) : base(tcpCommand)
        {
        }

        public void DoWork()
        {
            if (string.IsNullOrEmpty(TcpCommand.PrinterName))
            {
                InternalCommunication.GetInternalCommunication().HasError = true;
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = "Yazdırma",
                    Description = "Yazdırma başarısız. Yazıcı boş olamaz.",
                    NotificationType = NotificationType.Warning,
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
            else if (string.IsNullOrEmpty(TcpCommand.Content))
            {
                InternalCommunication.GetInternalCommunication().HasError = true;
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = "Yazdırma",
                    Description = TcpCommand.Description + Environment.NewLine +
                                  "Yazdırmak için herhangi bir döküman bulunmadı.",
                    NotificationType = NotificationType.Warning,
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
            else
            {
                string type = TcpCommand.Type;
                if (!(type == "PrintToImage"))
                {
                    if (!(type == "PrintToDos"))
                    {
                        if (!(type == "PrintToHtml"))
                            return;
                        setPrintResult(Helpers.PrinterHelper.PrintToHtml(TcpCommand.PrinterName, TcpCommand.Content));
                    }
                    else
                        setPrintResult(Helpers.PrinterHelper.PrintToDOS(TcpCommand.PrinterName, TcpCommand.Content));
                }
                else
                {
                    Image image;
                    using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(TcpCommand.Content)))
                        image = Image.FromStream(memoryStream, true);
                    setPrintResult(Helpers.PrinterHelper.PrintToImage(TcpCommand.PrinterName, image));
                }
            }
        }

        private void setPrintResult(bool printResult)
        {
            if (printResult)
            {
                InternalCommunication.GetInternalCommunication().IsSuccess = true;
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = TcpCommand.Header,
                    Description = TcpCommand.Description,
                    NotificationType = NotificationType.Success,
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
            else
            {
                InternalCommunication.GetInternalCommunication().HasError = true;
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = TcpCommand.Header,
                    Description = TcpCommand.Description + Environment.NewLine +
                                  "Yazdırma başarısız. Yazıcının bağlantılarının tam ve açık olduğundan emin olunuz.",
                    NotificationType = NotificationType.Error,
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
        }
    }
}