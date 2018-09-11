using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Panaroma.Communication.Application
{
    public class ProcessWorker : Worker
    {
        public ProcessWorker(TcpCommand tcpCommand) : base(tcpCommand)
        {
        }

        public void DoWork()
        {
            string type = TcpCommand.Type;
            if (type == "PrintToImage" || type == "PrintToDos")
            {
                (new PrinterWorker(TcpCommand)).DoWork();
                return;
            }

            if (type == "LocalPrinters")
            {
                IEnumerable<Printer> printers = Helpers.PrinterHelper.GetPrinters();
                InternalCommunication.GetInternalCommunication().IsSuccess = true;
                InternalCommunication.GetInternalCommunication().Results = JsonConvert.SerializeObject(printers);
                return;
            }

            if (type == "Sales")
            {
                (new OKCWorker(TcpCommand)).DoWork();
                return;
            }

            if (type == "LocalCashId")
            {
                string cashId = Helpers.RegistryHelper.GetCashId();
                InternalCommunication.GetInternalCommunication().IsSuccess = true;
                InternalCommunication.GetInternalCommunication().Results = JsonConvert.SerializeObject(cashId);
                return;
            }

            if (type == "Restart")
            {
                App.AllowMultipleApplication(true);
                return;
            }

            if (!(type == "SaveFile") && !(type == "GetFile"))
            {
                throw new NotSupportedException();
            }

            (new FileWorker(TcpCommand)).DoWork();
        }
    }
}