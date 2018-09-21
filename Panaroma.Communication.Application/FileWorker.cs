using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Panaroma.Communication.Application
{
    internal class FileWorker : Worker, IWorker
    {
        private static readonly string _folderpath =
            Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "files");

        public FileWorker(TcpCommand tcpCommand)
            : base(tcpCommand)
        {
            if(Directory.Exists(_folderpath))
                return;
            Directory.CreateDirectory(_folderpath);
        }

        public void DoWork()
        {
            FileWorkerParams fileWorkerParams = JsonConvert.DeserializeObject<FileWorkerParams>(TcpCommand.Content);
            if(fileWorkerParams.FileName.Length.Equals(0))
            {
                this.setFileResult(false, "Dosya adı boş olamaz.", null);
            }
            else
            {
                string path = Path.Combine(_folderpath, fileWorkerParams.FileName);
                string header = TcpCommand.Header;
                if(!(header == "SaveFile"))
                {
                    if(!(header == "GetFile"))
                        return;
                    if(!File.Exists(path))
                    {
                        this.setFileResult(false,
                            string.Format("{0} isimli dosya bulunamadı.", fileWorkerParams.FileName), null);
                    }
                    else
                    {
                        FileWorkerParams fileparams;
                        setFileResult(tryGetFile(fileWorkerParams.FileName, out fileparams), null, fileparams);
                    }
                }
                else
                {
                    trySaveFile(fileWorkerParams.FileName, fileWorkerParams.FileContent);
                    setFileResult(true, null, null);
                }
            }
        }

        private static void trySaveFile(string fullfilepath, string content)
        {
            if(File.Exists(fullfilepath))
                File.Delete(fullfilepath);
            File.WriteAllText(fullfilepath, content);
        }

        private static bool tryGetFile(string fullfilepath, out FileWorkerParams fileparams)
        {
            fileparams = new FileWorkerParams()
            {
                FileName = Path.GetFileName(fullfilepath),
                FileContent = File.ReadAllText(fullfilepath)
            };
            return true;
        }

        private void setFileResult(bool result, string message = null, FileWorkerParams fileparams = null)
        {
            if(result)
            {
                InternalCommunication.GetInternalCommunication().IsSuccess = true;
                InternalCommunication.GetInternalCommunication().Results = fileparams;
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = TcpCommand.Header,
                    Description = message ?? TcpCommand.Description,
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
                    Description = TcpCommand.Description + Environment.NewLine + message,
                    NotificationType = NotificationType.Error,
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
        }
    }
}