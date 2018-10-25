using Newtonsoft.Json;
using Panaroma.OKC.Integration.Library;
using PCPOSOKC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Panaroma.Communication.Application
{
    public class OKCVerifone : Worker, IWorker
    {
        private static readonly Cashier Cashier = new Cashier()
        {
            Id = "00",
            Password = "1234"
        };

        private static readonly OKCConfiguration OkcConfiguration = new OKCConfiguration();
        private static List<OKCProcesses> _okcProcesseses = new List<OKCProcesses>();
        private static OKC.Integration.Library.OKC _okc;
        private Members _requestMembers;

        public OKCVerifone(TcpCommand tcpCommand)
            : base(tcpCommand)
        {
            if(_okc != null)
            {
                _okc.SetDefaultProcessInformationAndMembers();
            }
            else
            {
                switch(OkcConfiguration.OKCConnectionType)
                {
                    case 1:
                        _okc = new OKC.Integration.Library.OKC(OkcConfiguration.ComConfiguration);
                        break;

                    case 2:
                        _okc = new OKC.Integration.Library.OKC(OkcConfiguration.EthernetConfiguration);
                        break;
                }
            }

            if(!OkcConfiguration.OKCLog)
                return;
            _okc?.TrySetLogStat("Logs");
        }

        public void DoWork()
        {
            WorkerExceptionHandle workerExceptionHandle = null;
            try
            {
                SetLogger();
                OKCParameters oKCParameter = JsonConvert.DeserializeObject<OKCParameters>(TcpCommand.Content);
                OKCProcesses.Start(ref _okcProcesseses, TcpCommand, oKCParameter);
                string type = oKCParameter.Type;
                switch(type)
                {
                    case "1":
                        {
                            TryReceiptBegin(oKCParameter);
                            break;
                        }
                    case "2":
                        {
                            TryDoTransaction(oKCParameter);
                            break;
                        }
                    case "3":
                        {
                            TryDoBatchTransaction(oKCParameter);
                            break;
                        }
                    case "4":
                        {
                            TryDoPayment(oKCParameter);
                            break;
                        }
                    case "5":
                        {
                            TryReceiptEnd();
                            break;
                        }
                    case "6":
                        {
                            TryFreePrint(oKCParameter);
                            break;
                        }
                    case "7":
                        {
                            TryGetReceiptTotal();
                            break;
                        }
                    case "8":
                        {
                            break;
                        }
                    case "9":
                        {
                            TryPrintZReport();
                            break;
                        }
                    case "10":
                        {
                            TryPrintXReport();
                            break;
                        }
                    case "11":
                        {
                            TryGetOKCStatus();
                            break;
                        }
                    case "12":
                        {
                            TryGMP3Pair();
                            break;
                        }
                    case "13":
                        {
                            TryPrintXPLUSaleReport(oKCParameter);
                            break;
                        }
                    case "14":
                        {
                            TryPrintXPLUProgram(oKCParameter);
                            break;
                        }
                    case "15":
                        {
                            TryPrintEkuDetailReport();
                            break;
                        }
                    case "16":
                        {
                            TryPrintEkuZDetailReport();
                            break;
                        }
                    case "17":
                        {
                            TryPrintEkuReceiptDetailReportWithDatetime(oKCParameter);
                            break;
                        }
                    case "18":
                        {
                            TryPrintLastSaleReceiptCopy();
                            break;
                        }
                    case "19":
                        {
                            TryPrintSalesReportWihtZNo(oKCParameter);
                            break;
                        }
                    case "20":
                        {
                            TryPrintBankEOD();
                            break;
                        }
                    case "21":
                        {
                            TryOpenDrawer();
                            break;
                        }
                    case "22":
                        {
                            TryRestartApp();
                            break;
                        }
                    case "23":
                        {
                            TryPowerOFF();
                            break;
                        }
                    case "24":
                        {
                            TrySetEcrConfig(oKCParameter);
                            break;
                        }
                    case "25":
                        {
                            TryPrintFinancalZDetailReportWithDateTime(oKCParameter);
                            break;
                        }
                    case "26":
                        {
                            TryPrintFinancalZDetailReportWithZNo(oKCParameter);
                            break;
                        }
                    case "27":
                        {
                            TryPrintFinancalZReportWithDateTime(oKCParameter);
                            break;
                        }
                    case "28":
                        {
                            TryPrintFinancalZReportWithZNo(oKCParameter);
                            break;
                        }
                    case "29":
                        {
                            TrySetGroup(oKCParameter);
                            break;
                        }
                    case "30":
                        {
                            TryPrintLastZReportCopy();
                            break;
                        }
                    case "31":
                        {
                            TryPrintBankSlipCopy(oKCParameter);
                            break;
                        }
                    case "32":
                        {
                            TryPing();
                            break;
                        }
                    case "33":
                        {
                            TryGetLastZReportSoftCopy();
                            break;
                        }
                    case "34":
                        {
                            TryFreePrintList(oKCParameter);
                            break;
                        }
                }
            }
            catch(Exception ex)
            {
                workerExceptionHandle = new WorkerExceptionHandle(ex);
            }
            finally
            {
                try
                {
                    if(_okc.ProcessInformation.HasError || workerExceptionHandle != null)
                        OKCResult.SetToCommunicationResult(true,
                            workerExceptionHandle != null
                                ? workerExceptionHandle.Exception.Message
                                : _okc.ProcessInformation.InformationMessages.Message, TcpCommand,
                            workerExceptionHandle != null
                                ? workerExceptionHandle.Exception
                                : _okc.ProcessInformation.InformationMessages.Exception, false);
                    else
                        OKCResult.SetToCommunicationResult(false, _okc.ProcessInformation.InformationMessages.Message,
                            TcpCommand, null, false);
                    OKCProcesses.End(_okcProcesseses, _okc.Request, _okc.Result,
                        JsonConvert.SerializeObject(_okc.ProcessInformation.InformationMessages.Message));
                    _okcProcesseses.Clear();
                }
                catch
                {
                    _okcProcesseses.Clear();
                }
            }
        }

        public void TryReceiptBegin(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            SetEcrConfig();
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryReceiptBegin(_requestMembers);
        }

        public void TryDoTransaction(OKCParameters okcparameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcparameters.Content);
            _okc.TryDoTransaction(_requestMembers);
        }

        public void TryDoBatchTransaction(OKCParameters okcParameters)
        {
            int i;
            BatchTranItem batchTranItem;
            Members[] membersArray = JsonConvert.DeserializeObject<Members[]>(okcParameters.Content);
            if(!membersArray.Any())
            {
                throw new ArgumentNullException("Fişe yazılacak ürün bulunamadı.");
            }

            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
            {
                return;
            }

            SetEcrConfig();
            _requestMembers = new Members();
            List<BatchTranItem> batchTranItems = new List<BatchTranItem>();
            Action action = () =>
            {
                _requestMembers.BatchTranItems = batchTranItems.ToArray();
                int count = batchTranItems.Count;
                _requestMembers.BatchItemCnt = count.ToString();
                _okc.TryDoBatchTransaction(_requestMembers);
                batchTranItems.Clear();
            };
            Members[] membersArray1 = membersArray;
            for(i = 0; i < membersArray1.Length; i++)
            {
                Members member = membersArray1[i];
                if(!member.ProcessType.Equals("AA"))
                {
                    List<BatchTranItem> batchTranItems1 = batchTranItems;
                    batchTranItem = new BatchTranItem()
                    {
                        ItemName =
                            IfValueIsNullOrEmptyThenReturnDefaultValue(member.ItemName, string.Empty),
                        Amount = IfValueIsNullOrEmptyThenReturnDefaultValue(member.Amount, string.Empty),
                        Barcode = IfValueIsNullOrEmptyThenReturnDefaultValue(member.Barcode, string.Empty),
                        CollId = IfValueIsNullOrEmptyThenReturnDefaultValue(member.CollectionId, "00"),
                        DepartId = IfValueIsNullOrEmptyThenReturnDefaultValue(member.DepartmentId,
                            string.Empty),
                        ProcessType = member.ProcessType,
                        Quantity =
                            IfValueIsNullOrEmptyThenReturnDefaultValue(member.Quantity, string.Empty),
                        Rate = IfValueIsNullOrEmptyThenReturnDefaultValue(member.Rate, string.Empty),
                        TranId = IfValueIsNullOrEmptyThenReturnDefaultValue(member.TranId, string.Empty),
                        UnitCode =
                            IfValueIsNullOrEmptyThenReturnDefaultValue(member.UnitCode, string.Empty),
                        TaxRate = IfValueIsNullOrEmptyThenReturnDefaultValue(member.TaxRate, string.Empty),
                        UnitPrice = IfValueIsNullOrEmptyThenReturnDefaultValue(member.UnitPrice,
                            string.Empty),
                        FreeText = string.Empty
                    };
                    batchTranItems1.Add(batchTranItem);
                }

                if(!string.IsNullOrEmpty(member.FreeText))
                {
                    List<BatchTranItem> batchTranItems2 = batchTranItems;
                    batchTranItem = new BatchTranItem()
                    {
                        ProcessType = "AA",
                        FreeText = (string.IsNullOrEmpty(member.FreeText)
                            ? ""
                            : (new FreeText(ConfigurationManager.AppSettings["Format"], ConfigurationManager.AppSettings["Position"],
                                (member.FreeText.Length > 42 ? member.FreeText.Substring(0, 42) : member.FreeText)))
                            .GetFreeText()),
                        CollId = IfValueIsNullOrEmptyThenReturnDefaultValue(member.CollectionId, "00"),
                        ItemName =
                            IfValueIsNullOrEmptyThenReturnDefaultValue(member.ItemName, string.Empty),
                        Barcode = IfValueIsNullOrEmptyThenReturnDefaultValue(member.Barcode, string.Empty),
                        Amount = IfValueIsNullOrEmptyThenReturnDefaultValue(member.Amount, string.Empty),
                        TranId = IfValueIsNullOrEmptyThenReturnDefaultValue(member.TranId, string.Empty),
                        Quantity =
                            IfValueIsNullOrEmptyThenReturnDefaultValue(member.Quantity, string.Empty),
                        DepartId = IfValueIsNullOrEmptyThenReturnDefaultValue(member.DepartmentId,
                            string.Empty),
                        Rate = IfValueIsNullOrEmptyThenReturnDefaultValue(member.Rate, string.Empty),
                        UnitCode =
                            IfValueIsNullOrEmptyThenReturnDefaultValue(member.UnitCode, string.Empty),
                        TaxRate = IfValueIsNullOrEmptyThenReturnDefaultValue(member.TaxRate, string.Empty),
                        UnitPrice = IfValueIsNullOrEmptyThenReturnDefaultValue(member.UnitPrice,
                            string.Empty)
                    };
                    batchTranItems2.Add(batchTranItem);
                }

                if(batchTranItems.Count > 39)
                {
                    action();
                }
            }

            if(!batchTranItems.Any())
            {
                return;
            }

            action();
            _requestMembers.BatchTranItems = batchTranItems.ToArray();
            i = batchTranItems.Count;
            _requestMembers.BatchItemCnt = i.ToString();
        }

        public void TryDoPayment(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryDoPayment(_requestMembers);
        }
        public void TryGetReceiptTotal()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _okc.TryGetReceiptTotal();
        }
        public void TryReceiptEnd()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _okc.TryReceiptEnd(new Members());
        }

        public void TryFreePrint(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _requestMembers.FreeText =
                new FreeText(_requestMembers.CurrIndex, _requestMembers.AcquirerId, _requestMembers.FreeText)
                    .GetFreeText();
            _okc.TryFreePrint(_requestMembers);
        }

        public void TryFreePrintList(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _requestMembers = new Members();
            _requestMembers.BatchTranItems = JsonConvert.DeserializeObject<List<FreeTextItems>>(okcParameters.Content)
                .Select(item => new BatchTranItem()
                {
                    ProcessType = "AA",
                    FreeText = string.IsNullOrEmpty(item.Text)
                        ? ""
                        : new FreeText(item.Format, item.Position,
                            item.Text.Length > 42 ? item.Text.Substring(0, 42) : item.Text).GetFreeText(),
                    CollId = "00",
                    ItemName = "",
                    Barcode = "",
                    Amount = "",
                    TranId = "",
                    Quantity = "",
                    DepartId = "",
                    Rate = "",
                    UnitCode = "",
                    TaxRate = "",
                    UnitPrice = ""
                }).ToArray();
            _requestMembers.BatchItemCnt = _requestMembers.BatchTranItems.Length.ToString();
            _okc.TryDoBatchTransaction(_requestMembers);
        }

        public void TryPrintZReport()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintZReport();
        }

        public void TryGetLastZReportSoftCopy()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryGetLastZReportSoftCopy();
        }

        public void TryPrintLastZReportCopy()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintLastZReportCopy();
        }

        public void TryPrintXReport()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintXReport();
        }

        public void TryGetOKCStatus()
        {
            _okc.TryGetOKCStatus();
        }

        public void TryGMP3Pair()
        {
            if(_okc.CheckGmp3PairStatus())
                return;
            _okc.TryConnectToEthernet();
            if(_okc.ProcessInformation.HasError)
                return;
            _okc.TryGMP3Pair();
        }

        public void TryPing()
        {
            _okc.TryPing();
        }

        public void TryPrintXPLUSaleReport(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintXPLUSaleReport(_requestMembers);
        }

        public void TryPrintXPLUProgram(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintXPLUProgram(_requestMembers);
        }

        public void TryPrintEkuDetailReport()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintEkuDetailReport();
        }

        public void TryPrintEkuZDetailReport()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintEkuZDetailReport();
        }

        public void TryPrintEkuReceiptDetailReportWithDatetime(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintEkuReceiptDetailReportWithDatetime(_requestMembers);
        }

        public void TryPrintFinancalZDetailReportWithDateTime(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintFinancalZDetailReportWithDateTime(_requestMembers);
        }

        public void TryPrintFinancalZDetailReportWithZNo(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintFinancalZDetailReportWithZNo(_requestMembers);
        }

        public void TryPrintFinancalZReportWithDateTime(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintFinancalZReportWithDateTime(_requestMembers);
        }

        public void TryPrintFinancalZReportWithZNo(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintFinancalZReportWithZNo(_requestMembers);
        }

        public void TryPrintLastSaleReceiptCopy()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintLastSaleReceiptCopy();
        }

        public void TryPrintSalesReportWihtZNo(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintSalesReportWihtZNo(_requestMembers);
        }

        public void TryPrintBankEOD()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _okc.TryPrintBankEOD();
        }

        public void TryPrintBankSlipCopy(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TryPrintBankSlipCopy(_requestMembers);
        }

        public void TryOpenDrawer()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _okc.TryOpenDrawer();
        }

        public void TryRestartApp()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _okc.TryRestartApp();
        }

        public void TryPowerOFF()
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            _okc.TryPowerOFF();
        }

        public void TrySetEcrConfig(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.SALE))
                return;
            HybridMembers hybridMembers = JsonConvert.DeserializeObject<HybridMembers>(okcParameters.Content);
            _okc.TrySetEcrConfig(hybridMembers);
        }

        public void TrySetGroup(OKCParameters okcParameters)
        {
            if(!PrepareSaleOrAdmin(EcrModeType.ADMIN))
                return;
            _requestMembers = JsonConvert.DeserializeObject<Members>(okcParameters.Content);
            _okc.TrySetGroup(_requestMembers);
        }

        private static bool PrepareSaleOrAdmin(EcrModeType ecrModelType)
        {
            _okc.TryCashierLogin(Cashier);
            _okc.TryChangeEcrMode(ecrModelType);
            return !_okc.ProcessInformation.HasError;
        }

        private static string IfValueIsNullOrEmptyThenReturnDefaultValue(string value, string defaultValue)
        {
            if(!string.IsNullOrEmpty(value))
                return value;
            return defaultValue;
        }

        private static void SetEcrConfig()
        {
            _okc.TrySetEcrConfig(new HybridMembers()
            {
                EcrConfig = new EcrConfig()
                {
                    IsDisableCashControl = true,
                    IsDisableCardInfoRequest = true,
                    IsDisableExitDeviceInSetMenu = true,
                    IsDisableExitFromSaleWithKeyX = true,
                    IsDisableDepartmentLimitMessages = true,
                    IsDisableSalesScreenTimeout = true
                }
            });
        }

        private static void SetLogger()
        {
            OKCProcesses.CreateIfNotExistsPanaromaLogFolderAndFile();
            OKCProcesses.Load(out _okcProcesseses);
        }

        private static string TryGetFromResultCodeToDescription(int okcresult)
        {
            return _okc.TryGetFromResultCodeToDescription(okcresult);
        }
    }
}