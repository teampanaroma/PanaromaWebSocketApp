using Newtonsoft.Json;
using PCPOSOKC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Panaroma.Communication.Application
{
    public class OKCProcesses
    {
        public static readonly string FolderPath =
            Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Panaroma Logs");

        public static readonly string DllFolderPath =
            Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Logs");

        public static readonly string AppSettingsFolderPath =
            Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "SettingLogs");

        public string OKCSerialNumber { get; set; }

        public string Method { get; set; }

        public string TypeCode { get; set; }

        public string OkcModel { get; set; }

        public ProcessTime ProcessDateTime { get; set; }

        public ProcessResult ProcessResult { get; set; }

        public ProcessRequest ProcessRequest { get; set; }

        public ProcessResponse ProcessResponse { get; set; }

        public static void CreateIfNotExistsPanaromaLogFolderAndFile()
        {
            Helpers.FolderHelper.CreateFolderIfNotExists(FolderPath);
            Helpers.FileHelper.CreateFileIfNotExists(Path.Combine(FolderPath, GetFileName()));
        }

        private static List<OKCProcesses> LoadOkcProcess()
        {
            if(!File.Exists(Path.Combine(FolderPath, GetFileName())))
            {
                return new List<OKCProcesses>();
            }

            string str = File.ReadAllText(Path.Combine(FolderPath, GetFileName()));
            if(string.IsNullOrEmpty(str))
            {
                return new List<OKCProcesses>();
            }

            return JsonConvert.DeserializeObject<List<OKCProcesses>>(str);
        }

        private static string GetFileName()
        {
            int day = DateTime.Now.Day;
            string str = day.ToString().PadLeft(2, '0');
            day = DateTime.Now.Month;
            string str1 = day.ToString().PadLeft(2, '0');
            day = DateTime.Now.Year;
            return string.Concat(str, str1, day.ToString(), ".log");
        }

        public static void Load(out List<OKCProcesses> okcProcesseses)
        {
            okcProcesseses = LoadOkcProcess();
        }

        public static void Start(ref List<OKCProcesses> okcProcesseses, TcpCommand tcpCommand,
            OKCParameters okcParameters)
        {
            okcProcesseses.Add(new OKCProcesses()
            {
                Method = tcpCommand.Method + " - " + tcpCommand.Header,
                OkcModel = tcpCommand.OKCModel,
                ProcessDateTime = new ProcessTime()
                {
                    StartDateTime = DateTime.Now
                },
                TypeCode = okcParameters.Type,
                ProcessResult = new ProcessResult(),
                ProcessRequest = new ProcessRequest()
                {
                    RawRequest = okcParameters.Content
                },
                ProcessResponse = new ProcessResponse()
            });
        }

        public static void End(IEnumerable<OKCProcesses> okcProcesseses, Members requestMembers,
            Members responseMembers, string message)
        {
            OKCProcesses okcProcesses = okcProcesseses.LastOrDefault();
            if(okcProcesses == null)
                return;
            okcProcesses.ProcessDateTime.EndDateTime = DateTime.Now;
            okcProcesses.ProcessDateTime.TotalProcessTime =
                okcProcesses.ProcessDateTime.EndDateTime - okcProcesses.ProcessDateTime.StartDateTime;
            okcProcesses.ProcessResult.Code = responseMembers.InternalErrNum;
            okcProcesses.ProcessResult.Description = message;
            okcProcesses.OKCSerialNumber = responseMembers.FiscalId;
            okcProcesses.ProcessRequest.OkcRequest =
                responseMembers.InternalErrNum == null ? null : (object)requestMembers;
            okcProcesses.ProcessResponse.RawResponse = message;
            okcProcesses.ProcessResponse.OkcResponse =
                responseMembers.InternalErrNum == null ? null : (object)responseMembers;
            File.WriteAllText(Path.Combine(FolderPath, GetFileName()), JsonConvert.SerializeObject(okcProcesseses,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
        }
    }
}