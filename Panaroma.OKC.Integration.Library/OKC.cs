using PCPOSOKC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Panaroma.OKC.Integration.Library
{
    public class OKC : IOKC
    {
        private Cashier _cashier = new Cashier()
        {
            Id = "00",
            Password = "1234"
        };

        private readonly EthernetConfiguration _ethernetConfiguration;
        private readonly COMConfiguration _comConfiguration;
        private ProcessInformation _processInformation;
        private readonly ConnectionType _connectionType;
        private EcrInterface _ecrInterface;
        private Members _result;
        private Members _request;

        public ProcessInformation ProcessInformation
        {
            get { return _processInformation; }
        }

        public Members Result
        {
            get { return _result; }
        }

        public Members Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public OKCConfiguration OKCConfiguration { get; private set; }

        public OKC(EthernetConfiguration ethernetConfiguration)
        {
            if(string.IsNullOrEmpty(ethernetConfiguration?.IpAddress) ||
                string.IsNullOrWhiteSpace(ethernetConfiguration.IpAddress) || ethernetConfiguration.Port < 0)
            {
                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                    "Ethernet ile bağlantıda IpAddress veya Port alanı boş alamaz.");
            }
            else
            {
                _ethernetConfiguration = ethernetConfiguration;
                _connectionType = ConnectionType.TCP_IP;
                ApplicationInit(true);
            }
        }

        public OKC(string ipAddress, int port)
        {
            if(string.IsNullOrEmpty(ipAddress) || string.IsNullOrWhiteSpace(ipAddress) || port < 0)
            {
                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                    "Ethernet ile bağlantıda IpAddress veya Port alanı boş alamaz.");
            }
            else
            {
                _ethernetConfiguration = new EthernetConfiguration(ipAddress, port.Equals(0) ? 41200 : port);
                _connectionType = ConnectionType.TCP_IP;
                ApplicationInit(true);
            }
        }

        public OKC(COMConfiguration comConfiguration)
        {
            if(string.IsNullOrEmpty(comConfiguration?.PortName) ||
                string.IsNullOrWhiteSpace(comConfiguration.PortName))
            {
                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                    "Com port ile bağlantıda port adresi boş alamaz.");
            }
            else
            {
                _comConfiguration = comConfiguration;
                _connectionType = ConnectionType.COM;
                this.ApplicationInit(true);
            }
        }

        public OKC(string comPortName)
        {
            if(string.IsNullOrEmpty(comPortName) || string.IsNullOrWhiteSpace(comPortName))
            {
                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                    "Com port ile bağlantıda port adresi boş alamaz.");
            }
            else
            {
                _comConfiguration = new COMConfiguration(comPortName);
                _connectionType = ConnectionType.COM;
                ApplicationInit(true);
            }
        }

        public OKC TryPing()
        {
            try
            {
                MethodInit(Request, "TryPing");
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_Ping, new Members(), ref _result);
                if(int.Parse(_result.InternalErrNum).Equals(0))
                    return this;
                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                    "Cihaz bilgisi alınamadı. İnternet bağlantısını kontrol ediniz.");
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryConnectToCOM()
        {
            try
            {
                MethodInit(Request, "TryConnectToCOM");
                _ecrInterface.COMM_Close();
                int resultId = _ecrInterface.COMM_Open(COMMTYPE.RS232, new CommMedia()
                {
                    strSerialPort = _comConfiguration.PortName
                });
                _result.InternalErrNum = resultId.ToString();
                SetApplicationResult(resultId, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryConnectToEthernet()
        {
            try
            {
                MethodInit(Request, "TryConnectToEthernet");
                _ecrInterface.COMM_Close();
                int resultId = _ecrInterface.COMM_Open(COMMTYPE.TCPIP, new CommMedia()
                {
                    iServerPort = _ethernetConfiguration.Port,
                    strServerAddr = _ethernetConfiguration.IpAddress
                });
                _result.InternalErrNum = resultId.ToString();
                SetApplicationResult(resultId, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public void SetDefaultProcessInformationAndMembers()
        {
            MethodInit(Request, "SetDefaultProcessInformationAndMembers");
        }

        public OKC TryCashierLogin(string cashierId, string cashierPassword)
        {
            _cashier = new Cashier()
            {
                Id = cashierId,
                Password = cashierPassword
            };
            return TryCashierLogin(_cashier);
        }

        public OKC TryCashierLogin(Cashier cashier)
        {
            try
            {
                MethodInit(Request, "TryCashierLogin");
                if(!CheckOkcConnection())
                    return this;
                _cashier = cashier;
                if(string.IsNullOrEmpty(cashier?.Id) || string.IsNullOrWhiteSpace(cashier.Id) ||
                    (string.IsNullOrEmpty(cashier.Password) || string.IsNullOrWhiteSpace(cashier.Password)) ||
                    (cashier.Id.Length > 4 || cashier.Id.Length < 2 ||
                     (cashier.Password.Length > 4 || cashier.Password.Length < 2)))
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "Kasiyer login işleminde Id veya şifre boş olamaz.");
                if(cashier != null)
                    _ecrInterface.SendCmd2ECR(Tags.msgREQ_CashierLogin, new Members()
                    {
                        CashierId = cashier.Id,
                        CashierPwd = cashier.Password
                    }, ref _result);
                int code = int.Parse(_result.InternalErrNum);
                if(!code.Equals(0))
                {
                    Helpers.Conditional.SetOKCWarningInformation(ref _processInformation, code);
                }
                else
                {
                    Helpers.Conditional.SetSuccessInformation(ref _processInformation);
                }
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGMP3Pair()
        {
            if(!CheckGmp3PairStatus())
                return this;

            try
            {
                MethodInit(Request, "TryGMP3Pair");
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GMP3Pair, new Members(), ref _result);
                SetApplicationResult(-100, new PairResult()
                {
                    Df02TraceNo = _result.groupDF02.TraceNo,
                    Df02TranDate = _result.groupDF02.TranDate,
                    Df02TranTime = _result.groupDF02.TranTime,
                    Df6FErrRespCode = _result.groupDF6F.ErrRespCode,
                    ErrRespCodeResult = _result.groupDF6F.status,
                    Df6FExtDevIndex = _result.groupDF6F.ExtDevIndex,
                    Df5KeyInvalidationCnt = _result.groupDF6F.keyInvalidationCnt,
                    KencKcv = _result.groupDF6F.Kcv
                });
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGMP3Echo()
        {
            try
            {
                MethodInit(Request, "TryGMP3Echo");
                string[] strArray = Assembly.LoadFrom("PCPOSOKC.dll").GetName().Version.ToString().Split('.');
                string str = strArray[0] + strArray[1] + strArray[2] + strArray[3];
                Members reqMembers = new Members();
                reqMembers.groupDF02.TraceNo = string.IsNullOrEmpty(reqMembers.groupDF02.TraceNo)
                    ? _ecrInterface.GMP3Pair.TranSeqNo.ToString().PadLeft(6, '0')
                    : reqMembers.groupDF02.TraceNo.PadLeft(6, '0');
                reqMembers.groupDF02.TranDate = string.IsNullOrEmpty(reqMembers.groupDF02.TranDate)
                    ? DateTime.Now.ToString("yyyyMMddHHmmss").Substring(2, 6)
                    : reqMembers.groupDF02.TranDate;
                reqMembers.groupDF02.TranTime = string.IsNullOrEmpty(reqMembers.groupDF02.TranTime)
                    ? DateTime.Now.ToString("yyyyMMddHHmmss").Substring(8, 6)
                    : reqMembers.groupDF02.TranTime;
                reqMembers.groupDF6F.VersionInfo = str.PadLeft(6, '0');
                reqMembers.groupDF6F.Kcv = _ecrInterface.GMP3Pair.getKCV().ToHexString();
                reqMembers.groupDF6F.status = "00000000";
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GMP3Echo, reqMembers, ref _result);
                SetApplicationResult(-100, (object)null);
            }
            catch(ArgumentOutOfRangeException ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryReceiptBegin(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryReceiptBegin");
                if(!CheckOkcConnection())
                    return this;
                OKC okcStatus = TryGetOKCStatus();
                if(okcStatus.ProcessInformation.HasError)
                {
                    Helpers.Conditional.SetExceptionInformation(ref _processInformation,
                        okcStatus.ProcessInformation.InformationMessages.Exception ??
                        new Exception(okcStatus.ProcessInformation.InformationMessages.Message.ToString()));
                    return this;
                }

                if(((OKCStatus)okcStatus.ProcessInformation.InformationMessages.Message).ReceiptIsOpen)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "Açıkta zaten bir fiş var.");
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.DocumentType) || requestMembers.DocumentType.Length > 2)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("{0} boş olamaz || {1} 2 karaketerden uzun olamaz.", "DocumentType",
                            "DocumentType"));
                    return this;
                }

                switch(requestMembers.DocumentType)
                {
                    case "02":
                    case "03":
                    case "04":
                        if(string.IsNullOrEmpty(requestMembers.Tckn) && string.IsNullOrEmpty(requestMembers.Vkn))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; fatura, e-fatura, e-arsiv fatura seçildiğinde {0} veya {1} alanları aynı anda boş geçilemez.",
                                    "Tckn", "Vkn"));
                            return this;
                        }

                        if(requestMembers.DocumentType.Equals("02") &&
                            (string.IsNullOrEmpty(requestMembers.BillSerialNo) ||
                             requestMembers.BillSerialNo.Length < 7))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format("{0} en az 7 karakter olmalı.", "BillSerialNo"));
                            return this;
                        }

                        break;

                    case "05":
                    case "07":
                        if(string.IsNullOrEmpty(requestMembers.Title))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; yemek ceki / kartı veya Avans seçildiğinde {0} alanı boş geçilemez.",
                                    "Title"));
                            return this;
                        }

                        if(requestMembers.DocumentType.Equals("07"))
                        {
                            if(string.IsNullOrEmpty(requestMembers.Amount))
                            {
                                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                    string.Format("Doküman tipi olarak; Avans seçildiğinde {0} alanı boş geçilemez.",
                                        "Amount"));
                                return this;
                            }

                            if(string.IsNullOrEmpty(requestMembers.Tckn) && string.IsNullOrEmpty(requestMembers.Vkn))
                            {
                                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                    string.Format(
                                        "Doküman tipi olarak; Avans seçildiğinde {0} veya {1} alanları aynı anda boş geçilemez.",
                                        "Tckn", requestMembers.Vkn));
                                return this;
                            }

                            break;
                        }

                        break;

                    case "08":
                        if(string.IsNullOrEmpty(requestMembers.Commision))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Fatura Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                    "Commision"));
                            return this;
                        }

                        if(string.IsNullOrEmpty(requestMembers.SubscriberNo))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Fatura Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                    "SubscriberNo"));
                            return this;
                        }

                        if(string.IsNullOrEmpty(requestMembers.Title))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Fatura Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                    "Title"));
                            return this;
                        }

                        if(string.IsNullOrEmpty(requestMembers.Amount))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Fatura Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                    "Amount"));
                            return this;
                        }

                        if(requestMembers.BillSerialNo.Length < 7)
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format("{0} en az 7 karakter olmalı.", "BillSerialNo"));
                            return this;
                        }

                        break;

                    case "09":
                    case "0B":
                    case "0C":
                    case "0F":
                        if(string.IsNullOrEmpty(requestMembers.BillSerialNo))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Tahsilat Makbuzu,Gider Pusulası,İrsaliye,Cari Hesap Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                    "BillSerialNo"));
                            return this;
                        }

                        if(requestMembers.BillSerialNo.Length < 7)
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format("{0} en az 7 karakter olmalı.", "BillSerialNo"));
                            return this;
                        }

                        if(requestMembers.DocumentType.Equals("0F"))
                        {
                            if(string.IsNullOrEmpty(requestMembers.TranDate))
                            {
                                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                    string.Format(
                                        "Doküman tipi olarak; Cari Hesap Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                        "TranDate"));
                                return this;
                            }

                            if(string.IsNullOrEmpty(requestMembers.TranTime))
                            {
                                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                    string.Format(
                                        "Doküman tipi olarak; Cari Hesap Tahsilatı seçildiğinde {0} alanı boş geçilemez.",
                                        "TranTime"));
                                return this;
                            }

                            break;
                        }

                        break;

                    case "0A":
                        if(string.IsNullOrEmpty(requestMembers.ReceiptNum))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Otopark Çıkış seçildiğinde {0} alanı boş geçilemez.",
                                    "ReceiptNum"));
                            return this;
                        }

                        break;

                    case "0D":
                    case "0E":
                        if(string.IsNullOrEmpty(requestMembers.Amount))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format(
                                    "Doküman tipi olarak; Kasa Giriş, Kasa Çıkış seçildiğinde {0} alanı boş geçilemez.",
                                    "Amount"));
                            return this;
                        }

                        break;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_RcptBegin, requestMembers, ref _result);
                SetApplicationResult(-100, new ReceiptBeginResponse()
                {
                    TranDate = Helpers.ReceiptHelper.MergeDateAndTime(_result.TranDate, _result.TranTime),
                    ZNum = _result.ZNum,
                    ReceiptNumber = _result.ReceiptNum,
                    UniqenNumber = _result.MidleUniqueNumber
                });
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryDoTransaction(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryDoTransaction");
                if(!CheckOkcConnection())
                    return this;
                if(!((OKCStatus)TryGetOKCStatus().ProcessInformation.InformationMessages.Message).ReceiptIsOpen)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "Açıkta fiş yok. Öncelikle yeni bir fiş açın.");
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.ProcessType))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("{0} alanı boş olamaz.", "ProcessType"));
                    return this;
                }

                if(requestMembers.ProcessType.Length > 2)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("{0} alanı 2 karakterden uzun olamaz.", "ProcessType"));
                    return this;
                }

                if(!string.IsNullOrEmpty(requestMembers.DepartmentId) && requestMembers.DepartmentId.Length > 2)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("{0} alanı 2 karakterden uzun olamaz.", "DepartmentId"));
                    return this;
                }

                if(requestMembers.ProcessType.Equals("A3") && string.IsNullOrEmpty(requestMembers.Amount) &&
                    string.IsNullOrEmpty(requestMembers.Rate))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "İndirim işleminde İndirim Oranı ve Tutar alanlarından en az birisi dolu olmalıdır.");
                    return this;
                }

                if(requestMembers.ProcessType.Equals("A4") && string.IsNullOrEmpty(requestMembers.Rate))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "Artırım işleminde artırım oranı dolu olmalıdır.");
                    return this;
                }

                if(requestMembers.ProcessType.Equals("A8"))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "İptal işleminde TranId boş geçilemez.");
                    return this;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_DoTran, requestMembers, ref _result);
                SetApplicationResult(-100, new DoTransactionResponse()
                {
                    ZNum = _result.ZNum,
                    ReceiptNumber = _result.ReceiptNum,
                    UniqenNumber = _result.MidleUniqueNumber,
                    TranId = _result.TranId
                });
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryDoBatchTransaction(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryDoBatchTransaction");
                if(!CheckOkcConnection())
                    return this;
                if(requestMembers.BatchTranItems == null ||
                    !((IEnumerable<BatchTranItem>)requestMembers.BatchTranItems).Any())
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("{0} boş olamaz.", "BatchTranItems"));
                    return this;
                }

                for(int index = 0; index < requestMembers.BatchTranItems.Length; ++index)
                {
                    if(string.IsNullOrEmpty(requestMembers.BatchTranItems[index].ProcessType))
                    {
                        Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                            "BatchItem ProcessType alanı boş olamaz.");
                        return this;
                    }

                    if(requestMembers.BatchTranItems[index].ProcessType.Length > 2)
                    {
                        Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                            "BatchItem ProcessType alanı 2 karakterden uzun olamaz.");
                        return this;
                    }
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_BatchTran, requestMembers, ref _result);
                SetApplicationResult(-100, new DoTransactionResponse()
                {
                    ZNum = _result.ZNum,
                    ReceiptNumber = _result.ReceiptNum,
                    UniqenNumber = _result.MidleUniqueNumber,
                    TranId = _result.TranId
                });
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryDoPayment(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryDoPayment");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.PaymentType))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("Ödeme işleminde {0} dolu olmalıdır.", "PaymentType"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.Amount))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("Ödeme işleminde {0} dolu olmalıdır.", "Amount"));
                    return this;
                }

                if(requestMembers.PaymentType.Equals("03"))
                {
                    if(string.IsNullOrEmpty(requestMembers.ExcRate))
                    {
                        Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                            string.Format("Dövizli ödeme işlemlerinde {0} dolu olmalıdır.", "Amount"));
                    }
                }
                if(requestMembers.ProcessType != null)
                {
                    if(requestMembers.ProcessType.Equals("30") || requestMembers.ProcessType.Equals("50"))
                    {
                        if(string.IsNullOrEmpty(requestMembers.AcquirerId))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format("İptal ve iade işleminde {0} dolu olmalıdır.", "AcquirerId"));
                            return this;
                        }

                        if(requestMembers.ProcessType.Equals("30"))
                        {
                            if(string.IsNullOrEmpty(requestMembers.BatchNum))
                            {
                                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                    string.Format("İptal işleminde {0} dolu olmalıdır.", "BatchNum"));
                                return this;
                            }

                            if(string.IsNullOrEmpty(requestMembers.StanNum))
                            {
                                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                    string.Format("İptal işleminde {0} dolu olmalıdır.", "StanNum"));
                                return this;
                            }
                        }

                        if(!string.IsNullOrEmpty(requestMembers.ReferenceNumber) &&
                            requestMembers.ReferenceNumber.Length < 10)
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format("{0} en az 10 karakter olmalıdır.", "ReferenceNumber"));
                            return this;
                        }
                    }

                    if(requestMembers.ProcessType.Equals("02") && string.IsNullOrEmpty(requestMembers.InstallmentCnt))
                    {
                        Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                            string.Format("Taksitli satış işleminde {0} dolu olmalıdır.", "InstallmentCnt"));
                        return this;
                    }
                }
                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_DoPayment, requestMembers, ref _result);
                int code = int.Parse(_result.InternalErrNum);
                if(code.Equals(0))
                {
                    Func<string, bool> func = response =>
                    {
                        if(_result.ResponseCode == null)
                            return false;
                        if(!_result.ResponseCode.Equals("00") && !_result.ResponseCode.Equals("08"))
                            return _result.ResponseCode.Equals("11");
                        return true;
                    };
                    if(requestMembers.PaymentType.Equals("02") && func(_result.ResponseCode))
                    {
                        Helpers.Conditional.SetSuccessInformation(ref _processInformation, new BankResponse()
                        {
                            Amount = _result.Amount,
                            ProcessType = _result.ProcessType,
                            AuthCode = _result.AuthCode,
                            BatchNum = _result.BatchNum,
                            CardNum = _result.CardNum,
                            TranDate = _result.TranDate,
                            AcquirerId = _result.AcquirerId,
                            CardType = _result.CardType,
                            IssuerId = _result.IssuerId,
                            MerchantId = _result.MerchantId,
                            Posem = _result.Posem,
                            ResponseCode = _result.ResponseCode,
                            StanNum = _result.StanNum,
                            TerminalId = _result.TerminalId,
                            ReferenceNumber = _result.ReferenceNumber
                        });
                    }
                    else
                    {
                        if(requestMembers.PaymentType.Equals("02") && !func(_result.ResponseCode))
                        {
                            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                                string.Format("Banka işlemi başarısız. Hata kodu: {0}", _result.ResponseCode));
                            return this;
                        }

                        #region GetReceiptTotal

                        ReceiptTotal rTotal = new ReceiptTotal();
                        TryGetReceiptTotal();
                        Func<string> func2 = () =>
                        {
                            if(string.IsNullOrEmpty(_result.Amount))
                                return null;
                            if(!(int.Parse(_result.Amount).ToString() == "0"))
                                return int.Parse(_result.Amount).ToString();
                            return null;
                        };
                        if(_result.PaymentSummryCnt.Equals(0))
                        {
                            rTotal.ToplamTutar = func2();
                        }
                        else
                        {
                            int num = _result.PaymentSummary.Where(payment => !string.IsNullOrEmpty(payment.Amount))
                                .Sum(payment => int.Parse(payment.Amount));
                            rTotal.ToplamOdenenTutar = num.ToString();
                            rTotal.ToplamTutar = func2();
                            rTotal.KalanTutar = Convert.ToString(Convert.ToInt32(rTotal.ToplamTutar) - Convert.ToInt32(rTotal.ToplamOdenenTutar));
                            rTotal.OdenenTutar = requestMembers.Amount.TrimStart('0');
                            rTotal.Aciklama = "Para üstü yok";
                            if(Convert.ToInt32(rTotal.OdenenTutar) > Convert.ToInt32(rTotal.KalanTutar) && Convert.ToInt32(rTotal.KalanTutar) == 0)
                            {
                                rTotal.Aciklama = "Para Üstü Var";
                            }
                        }

                        #endregion GetReceiptTotal

                        Helpers.Conditional.SetSuccessInformation(ref _processInformation, rTotal);
                    }
                }
                else
                    Helpers.Conditional.SetOKCWarningInformation(ref _processInformation, code);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryReceiptEnd(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryReceiptEnd");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_ReceiptEnd, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryFreePrint(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryFreePrint");
                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_FreePrint, requestMembers, ref _result);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetOKCStatus()
        {
            try
            {
                MethodInit(Request, "TryGetOKCStatus");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_Ping, new Members(), ref _result);
                if(string.IsNullOrEmpty(_result.InternalErrNum) || !int.Parse(_result.InternalErrNum).Equals(0))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "Cihaz bilgisi alınamadı. İnternet bağlantısını kontrol edin.");
                    return this;
                }

                OKCStatus okcStatus = new OKCStatus()
                {
                    IsConnected = true,
                    DocumentType = int.Parse(_result.DocumentType),
                    AppVersion = _result.AppVersion,
                    CashierId = int.Parse(_result.CashierId),
                    EJNUM = _result.EJNum,
                    EcrMode = _result.EcrMode.Equals("02") ? EcrModeType.SALE : EcrModeType.ADMIN,
                    FiscalID = _result.FiscalId,
                    ReceiptNum = _result.ReceiptNum,
                    TranDate = Helpers.ReceiptHelper.MergeDateAndTime(_result.TranDate, _result.TranTime),
                    TranStatus = int.Parse(_result.TranStatus),
                    ZNum = _result.ZNum,
                    PaperInfo = _result.PaperInfo,
                    EjCapacityInfo = _result.EjCapacityInfo,
                    EjRemainigCountOfLines = _result.EjRemainingCountOfLines,
                    TranStatusDescription = Helpers.ReceiptHelper.GetStatusDescription(_result.TranStatus)
                };
                if(_result.TranStatus.Equals("3") || _result.TranStatus.Equals("4"))
                    okcStatus.ReceiptIsOpen = true;
                if(okcStatus.IsConnected && okcStatus.ReceiptIsOpen)
                {
                    if(okcStatus.EcrMode == EcrModeType.ADMIN)
                        _ecrInterface.SendCmd2ECR(Tags.msgREQ_ChangeEcrMode, new Members()
                        {
                            EcrMode = "02",
                            CashierId = _cashier.Id,
                            CashierPwd = _cashier.Password
                        }, ref _result);
                    TryGetReceiptTotal();
                    Func<string> func = () =>
                    {
                        if(string.IsNullOrEmpty(_result.Amount))
                            return null;
                        if(!(int.Parse(_result.Amount).ToString() == "0"))
                            return int.Parse(_result.Amount).ToString();
                        return null;
                    };
                    if(_result.PaymentSummryCnt.Equals(0))
                    {
                        okcStatus.Amount = func();
                    }
                    else
                    {
                        int num = _result.PaymentSummary.Where(payment => !string.IsNullOrEmpty(payment.Amount))
                            .Sum(payment => int.Parse(payment.Amount));
                        okcStatus.PaidAmount = num.ToString();
                        okcStatus.Amount = func();
                    }
                }

                Helpers.Conditional.SetSuccessInformation(ref _processInformation, okcStatus);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryChangeEcrMode(string ecrMode)
        {
            if(string.IsNullOrEmpty(ecrMode) || string.IsNullOrWhiteSpace(ecrMode) || !ecrMode.Length.Equals(2))
            {
                Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                    "EcrMode boş veya iki karakterden kısa olamaz.");
                return this;
            }

            return TryChangeEcrMode((ecrMode == "03" ? EcrModeType.ADMIN : EcrModeType.SALE));
        }

        public OKC TryChangeEcrMode(EcrModeType ecrModeType)
        {
            try
            {
                MethodInit(Request, "TryChangeEcrMode");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(_cashier?.Id) || string.IsNullOrWhiteSpace(_cashier.Id) ||
                    (string.IsNullOrEmpty(_cashier.Password) || string.IsNullOrWhiteSpace(_cashier.Password)))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "ChangeEcrMode işleminde Kasiyer Id ve Parola alanı dolu olmalıdır.");
                    return this;
                }

                Members members = new Members()
                {
                    EcrMode = ecrModeType == EcrModeType.ADMIN ? "03" : "02",
                    CashierId = _cashier.Id,
                    CashierPwd = _cashier.Password
                };
                Helpers.MembersHelper.SetDefaultPadLeft(ref members);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_ChangeEcrMode, members, ref _result);
                int code = int.Parse(_result.InternalErrNum);
                if(code.Equals(0))
                    Helpers.Conditional.SetSuccessInformation(ref _processInformation);
                else
                    Helpers.Conditional.SetOKCWarningInformation(ref _processInformation, code);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetReceiptTotal()
        {
            try
            {
                MethodInit(Request, "TryGetReceiptTotal");
                if(CheckOkcConnection())
                {
                    Members member = new Members()
                    {
                        CreditPaymentResult = new CreditPaymentResultTable[20],
                        VATGrpTotal = new VATGroupTotalTable[8],
                        PaymentSummary = new PaymentSummaryTable[3]
                    };
                    _result = member;
                    EcrInterface ecrInterface = _ecrInterface;
                    member = new Members();
                    ecrInterface.SendCmd2ECR(Tags.msgREQ_GetReceiptTot, member, ref _result);
                    SetApplicationResult(-100, null);
                }
                else
                {
                    return this;
                }
            }
            catch(Exception exception)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, exception);
            }

            return this;
        }

        public OKC TryCardInfo(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryCardInfo");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.CardPrefix))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("CardInfo işleminde {0} alanı dolu olmalıdır.", "CardPrefix"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.AcquirerId))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("CardInfo işleminde {0} alanı dolu olmalıdır.", "AcquirerId"));
                    return this;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_InfoInquiry, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetHeader(IDictionary<byte, string> headers)
        {
            try
            {
                MethodInit(Request, "TrySetHeader");
                if(!CheckOkcConnection())
                    return this;
                if(headers == null || headers.Count > 8 || headers.Keys.Any(e => e > 8))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "Header bilgileri boş alamaz || Başlık sayısı 8 den büyük olamaz || Satır numarası 8 den büyük olamaz.");
                    return this;
                }

                int resultId = 0;
                foreach(KeyValuePair<byte, string> header in headers)
                {
                    _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetHeader, new Members()
                    {
                        HeaderText = header.Value,
                        LineNum = header.Key.ToString()
                    }, ref _result);
                    if((resultId = int.Parse(_result.InternalErrNum)) != 0)
                        break;
                }

                SetApplicationResult(resultId, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetExchange(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryGetExchange");
                if(!CheckOkcConnection())
                    return this;
                _result = new Members()
                {
                    CurrTable = new CurrencyTable[6]
                };
                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetExchange, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetVatRates()
        {
            try
            {
                MethodInit(Request, "TryGetVatRates");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetVatRates, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryReceiptInquiry(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryReceiptInquiry");
                if(!CheckOkcConnection())
                    return this;
                _result = new Members()
                {
                    CreditPaymentResult = new CreditPaymentResultTable[20]
                };
                if(string.IsNullOrEmpty(requestMembers.ZNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("ReceiptInquiry işleminde {0} alanı dolu olmalıdır.", "ZNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.ReceiptNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("ReceiptInquiry işleminde {0} alanı dolu olmalıdır.", "ReceiptNum"));
                    return this;
                }

                requestMembers.ZNum = int.Parse(requestMembers.ZNum).ToString().PadLeft(4, '0');
                requestMembers.ReceiptNum = int.Parse(requestMembers.ReceiptNum).ToString().PadLeft(6, '0');
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_ReceiptInq, requestMembers, ref _result);
                SetApplicationResult(-100, new ReceiptInquiryResponse().SetResponse(_result));
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetEcrConfig(HybridMembers hybridMembers)
        {
            try
            {
                MethodInit(hybridMembers.RequestMembers, "TrySetEcrConfig");
                if(!CheckOkcConnection())
                    return this;
                ulong ecrConfig = 0x0000000000000000;
                if(hybridMembers.EcrConfig.IsDisableCashControl)
                    ecrConfig |= 0x01;
                if(hybridMembers.EcrConfig.IsDisableErrorsMessages)
                    ecrConfig |= 0x02;
                if(hybridMembers.EcrConfig.IsDisableWarningMessages)
                    ecrConfig |= 0x04;
                if(hybridMembers.EcrConfig.IsDisableInformationMessages)
                    ecrConfig |= 0x08;
                if(hybridMembers.EcrConfig.IsDisableReceiptFooterMessages)
                    ecrConfig |= 0x10;
                if(hybridMembers.EcrConfig.IsDisableDepartmentLimitMessages)
                    ecrConfig |= 0x80;
                if(hybridMembers.EcrConfig.IsActiveEArchiveSecondCopy)
                    ecrConfig |= 0x0200;
                if(hybridMembers.EcrConfig.IsActiveEInvoiceSecondCopy)
                    ecrConfig |= 0x0400;
                if(hybridMembers.EcrConfig.IsActiveCollectionReceiptsSecondCopy)
                    ecrConfig |= 0x0800;
                if(hybridMembers.EcrConfig.IsDisableCardInfoRequest)
                    ecrConfig |= 0x1000;
                if(hybridMembers.EcrConfig.IsDisableExitFromSaleWithKeyX)
                    ecrConfig |= 0x4000;
                if(hybridMembers.EcrConfig.IsDisableExitDeviceInSetMenu)
                    ecrConfig |= 0x2000;
                if(hybridMembers.EcrConfig.IsDisableSalesScreenTimeout)
                    ecrConfig |= 0x8000;
                if(hybridMembers.EcrConfig.IsEnableRed51)
                    ecrConfig |= 0x10000;
                if(hybridMembers.EcrConfig.IsEnableDiscount)
                    ecrConfig |= 0x20000;
                Members requestMembers = hybridMembers.RequestMembers;
                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                requestMembers.EcrConfig = string.Format("{0:x16}", ecrConfig);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_EcrConfig, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryRestartApp()
        {
            try
            {
                MethodInit(Request, "TryRestartApp");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_RestartApp, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetExchange(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TrySetExchange");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.CurrencyCodes))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetExchange işleminde {0} alanı dolu olmalıdır.", "CurrencyCodes"));
                    return this;
                }

                if(requestMembers.CurrencyCodes.Length < 2)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetExchange işleminde {0} en az bir adet döviz kuru seçilmelidir.",
                            "CurrencyCodes"));
                    return this;
                }

                _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetExchange, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetPaper(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TrySetPaper");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.PaperStatus))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPaper işleminde {0} alanı dolu olmalıdır.", "PaperStatus"));
                    return this;
                }

                _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetPaperStatus, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetDepartmentList()
        {
            try
            {
                MethodInit(Request, "TryGetDepartmentList");
                if(!CheckOkcConnection())
                    return this;
                _result = new Members()
                {
                    DepList = new DepListTable[8]
                };
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetDepList, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetDepartmentList(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TrySetDepartmentList");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.ItemName))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetDepartmentList işlemi için {0} alanı dolu olmalı.", "ItemName"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.Amount))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetDepartmentList işlemi için {0} alanı dolu olmalı.", "Amount"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.DepLimitAmount))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetDepartmentList işlemi için {0} alanı dolu olmalı.", "DepLimitAmount"));
                    return this;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetDepList, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetDepartmentList(params Department[] departments)
        {
            try
            {
                MethodInit(Request, "TrySetDepartmentList");
                if(!CheckOkcConnection())
                    return this;
                if(departments == null || !((IEnumerable<Department>)departments).Any() || departments.Length > 8)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        "TrySetDepartmentList işlemi için Departman boş olamaz || Departman sayısı 8 den büyük olamaz.");
                    return this;
                }

                foreach(Department department in departments)
                {
                    Members members = new Members()
                    {
                        Amount = department.Amount,
                        DepLimitAmount = department.LimitAmount,
                        VatGroup = ((byte)department.VatGroup).ToString().PadLeft(2, '0'),
                        ItemName = department.DepartmentName,
                        DepartmentId = department.Id
                    };
                    Helpers.MembersHelper.SetDefaultPadLeft(ref members);
                    _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetDepList, members, ref _result);
                    int code = int.Parse(_result.InternalErrNum);
                    if(code != 0)
                    {
                        Helpers.Conditional.SetOKCWarningInformation(ref _processInformation, code);
                        break;
                    }
                }

                Helpers.Conditional.SetSuccessInformation(ref _processInformation);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryDisconnect()
        {
            try
            {
                MethodInit(Request, "TryDisconnect");
                _ecrInterface?.COMM_Close();
                Helpers.Conditional.SetSuccessInformation(ref _processInformation);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public string TryGetAppVersion()
        {
            MethodInit(Request, "TryGetAppVersion");
            return TryGMP3Echo()._result.groupDF6F.VersionInfo;
        }

        public string TryGetFromResultCodeToDescription(int okcresult)
        {
            return EcrInterface.GetErrorExplain(okcresult);
        }

        public OKC TryPowerOFF()
        {
            try
            {
                MethodInit(Request, "TryPowerOFF");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_PowerOFF, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryInfoInquiryResponse(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryInfoInquiryResponse");
                if(!string.IsNullOrEmpty(requestMembers.InstallmentCnt))
                    requestMembers.InstallmentCnt = requestMembers.InstallmentCnt.PadLeft(4, '0');
                if(!string.IsNullOrEmpty(requestMembers.Amount))
                    requestMembers.Amount = requestMembers.Amount.PadLeft(12, '0');
                if(!string.IsNullOrEmpty(requestMembers.AcquirerId))
                    requestMembers.AcquirerId = requestMembers.AcquirerId.PadLeft(4, '0');
                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgRSP_InfoInquiry, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryOpenDrawer()
        {
            try
            {
                MethodInit(Request, "TryOpenDrawer");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_OpenDrawer, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetDrawerStatus()
        {
            try
            {
                MethodInit(Request, "TryGetDrawerStatus");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetDrawerStat, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetUniqueId()
        {
            try
            {
                MethodInit(Request, "TryGetUniqueId");
                if(!CheckOkcConnection())
                    return this;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetUniqueId, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetGroup(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TrySetGroup");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.GroupNo))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetGroup işleminde {0} alanı dolu olmalıdır.", "GroupNo"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.DepartmentId))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetGroup işleminde {0} alanı dolu olmalıdır.", "DepartmentId"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.GroupName))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetGroup işleminde {0} alanı dolu olmalıdır.", "GroupName"));
                    return this;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetGroup, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TrySetPLU(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TrySetPLU");
                if(!CheckOkcConnection())
                    return this;
                if(string.IsNullOrEmpty(requestMembers.PLUNo))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPLU işleminde {0} alanı dolu olmalıdır.", "PLUNo"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.Barcode))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPLU işleminde {0} alanı dolu olmalıdır.", "Barcode"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.Amount))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPLU işleminde {0} alanı dolu olmalıdır.", "Amount"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.UnitCode))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPLU işleminde {0} alanı dolu olmalıdır.", "UnitCode"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.ItemName))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPLU işleminde {0} alanı dolu olmalıdır.", "ItemName"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.GroupNo))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("SetPLU işleminde {0} alanı dolu olmalıdır.", "GroupNo"));
                    return this;
                }

                if(requestMembers.StockControl.Equals("1") && string.IsNullOrEmpty(requestMembers.StockPiece))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format(
                            "SetPLU işleminde StockControl alanı 1 olarak seçildiğinde {0} alanı dolu olmalıdır.",
                            "StockPiece"));
                    return this;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                if(Convert.ToInt32(requestMembers.PLUNo) > 100000)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("{0} 100.000 den büyük olamaz.", requestMembers.PLUNo));
                    return this;
                }

                _ecrInterface.SendCmd2ECR(Tags.msgREQ_SetPLU, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetBankList()
        {
            try
            {
                MethodInit(Request, "TryGetBankList");
                if(!CheckOkcConnection())
                    return this;
                _result = new Members()
                {
                    BankList = new BankListTable[25]
                };
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetBankList, new Members(), ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public void TrySetLogStat(string logFolder = "Logs")
        {
            MethodInit(Request, "TrySetLogStat");
            _ecrInterface.SetLogStat(true, string.IsNullOrEmpty(logFolder) ? "Logs" : logFolder);
        }

        private void ApplicationInit(bool gmp3Pair = false)
        {
            if(_ecrInterface == null)
                _ecrInterface = new EcrInterface();
            switch(_connectionType)
            {
                case ConnectionType.TCP_IP:
                    OKCConfiguration = OKCConfigurationFactory.Build(_ethernetConfiguration);
                    TryConnectToEthernet();
                    break;

                case ConnectionType.COM:
                    OKCConfiguration = OKCConfigurationFactory.Build(_comConfiguration);
                    TryConnectToCOM();
                    break;
            }

            if(gmp3Pair)
            {
                TryGMP3Pair();
            }
            else
            {
                TryGMP3Echo();
                if(string.IsNullOrEmpty(_result.InternalErrNum) || !_result.InternalErrNum.Equals("0") ||
                    (string.IsNullOrEmpty(_result.groupDF6F.status) || _result.groupDF6F.status.Equals("1D")))
                    return;
                TryGMP3Pair();
            }
        }

        private OKC TryPrintReport(Members requestMembers, string reportType)
        {
            try
            {
                MethodInit(Request, "TryPrintReport"); //Değişebilir.
                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                requestMembers.ReportType = reportType;
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_PrintReport, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        private void SetApplicationResult(int resultId = -100, object returnResult = null)
        {
            if(resultId.Equals(-100))
                resultId = int.Parse(_result.InternalErrNum);
            if(resultId == 0)
            {
                if(returnResult == null)
                    Helpers.Conditional.SetSuccessInformation(ref _processInformation);
                else
                    Helpers.Conditional.SetSuccessInformation(ref _processInformation, returnResult);
            }
            else
                Helpers.Conditional.SetOKCWarningInformation(ref _processInformation, resultId);
        }

        private void MethodInit(Members requestMembers, [CallerMemberName] string caller = "")
        {
            _result = new Members();
            _request = requestMembers;
            _processInformation = new ProcessInformation();
        }

        private bool CheckOkcConnection()
        {
            TryPing();
            if(!string.IsNullOrEmpty(_result.InternalErrNum) && _result.InternalErrNum.Equals("0"))
                return true;
            ApplicationInit(false);
            if(!string.IsNullOrEmpty(_result.InternalErrNum))
                return _result.InternalErrNum.Equals("0");
            return false;
        }

        public bool CheckGmp3PairStatus()
        {
            TryGMP3Echo();
            if(!string.IsNullOrEmpty(_result.InternalErrNum) && _result.InternalErrNum.Equals("0"))
                return true;
            ApplicationInit(false);
            if(!string.IsNullOrEmpty(_result.InternalErrNum))
                return _result.InternalErrNum.Equals("0");
            return false;
        }

        public OKC TryGetPLUList(Members requestMembers)
        {
            try
            {
                MethodInit(requestMembers, "TryGetPLUList");
                if(!CheckOkcConnection())
                    return this;
                _result = new Members()
                {
                    PLUList = new PLUTable[100]
                };
                if(string.IsNullOrEmpty(requestMembers.StartPLUNo))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("GetPLUList işleminde {0} alanı dolu olmalıdır.", "StartPLUNo"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.EndPLUNo))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("GetPLUList işleminde {0} alanı dolu olmalıdır.", "EndPLUNo"));
                    return this;
                }

                Helpers.MembersHelper.SetDefaultPadLeft(ref requestMembers);
                int startPLUNo = Convert.ToInt32(requestMembers.StartPLUNo);
                int endPLUNo = Convert.ToInt32(requestMembers.EndPLUNo);
                int pluLimit = 100000;
                if(startPLUNo > pluLimit)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("GetPLUList işleminde {0} 100.000 den büyük olamaz.", "StartPLUNo"));
                    return this;
                }

                if(endPLUNo > 100000)
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("GetPLUList işleminde {0} 100.000 den büyük olamaz.", "EndPLUNo"));
                    return this;
                }

                _ecrInterface.SendCmd2ECR(Tags.msgREQ_GetPLUList, requestMembers, ref _result);
                SetApplicationResult(-100, null);
            }
            catch(FormatException ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }
            catch(OverflowException ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryPrintZReport()
        {
            try
            {
                MethodInit(Request, "TryPrintZReport");
                if(!CheckOkcConnection())
                    return this;
                Members members = new Members()
                {
                    ReportType = string.Format("{0:x3}", 2097152)
                };
                Helpers.MembersHelper.SetDefaultPadLeft(ref members);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_PrintReport, members, ref _result);
                SetApplicationResult(-100, _result.SoftCopyOfReport);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryGetLastZReportSoftCopy()
        {
            try
            {
                MethodInit(Request, "TryGetLastZReportSoftCopy");
                if(!CheckOkcConnection())
                    return this;
                Members members = new Members()
                {
                    ReportType = string.Format("{0:x3}", ECR_RPRT_TYPS.Z_SOFT_LAST_COPY)
                };
                Helpers.MembersHelper.SetDefaultPadLeft(ref members);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_PrintReport, members, ref _result);
                SetApplicationResult(-100, _result.SoftCopyOfReport);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryPrintLastZReportCopy()
        {
            MethodInit(Request, "TryPrintLastZReportCopy");
            if(CheckOkcConnection())
                return TryPrintReport(new Members(), string.Format("{0:x3}", ECR_RPRT_TYPS.Z_LAST_COPY));
            return this;
        }

        public OKC TryPrintXReport()
        {
            try
            {
                MethodInit(Request, "TryPrintZReport");
                if(!CheckOkcConnection())
                    return this;
                Members members = new Members()
                {
                    ReportType = string.Format("{0:x3}", 1048576)
                };
                Helpers.MembersHelper.SetDefaultPadLeft(ref members);
                _ecrInterface.SendCmd2ECR(Tags.msgREQ_PrintReport, members, ref _result);
                SetApplicationResult(-100, _result.SoftCopyOfReport);
            }
            catch(Exception ex)
            {
                Helpers.Conditional.SetExceptionInformation(ref _processInformation, ex);
            }

            return this;
        }

        public OKC TryPrintXPLUSaleReport(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintXPLUSaleReport");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartPLUNo.Length != 0 || requestMembers.EndPLUNo.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.X_PLU_SALE_PP));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "Start ve End PluNo lar eksik olamaz.");
            return this;
        }

        public OKC TryPrintZPLUSaleReport(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintZPLUSaleReport");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartPLUNo.Length != 0 || requestMembers.EndPLUNo.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.Z_PLU_SALE_PP));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "Start ve End PluNo lar eksik olamaz.");
            return this;
        }

        public OKC TryPrintXPLUProgram(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintXPLUProgram");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartPLUNo.Length != 0 && requestMembers.EndPLUNo.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.X_PLU_PRG_PP));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "Start ve End PluNo lar eksik olamaz.");
            return this;
        }

        public OKC TryPrintEkuDetailReport()
        {
            MethodInit(Request, "TryPrintEkuDetailReport");
            if(CheckOkcConnection())
                return TryPrintReport(new Members(), string.Format("{0:x3}", ECR_RPRT_TYPS.EJ_DETAIL));
            return this;
        }

        public OKC TryPrintEkuZDetailReport()
        {
            MethodInit(Request, "TryPrintEkuZDetailReport");
            if(CheckOkcConnection())
                return TryPrintReport(new Members(), string.Format("{0:x3}", ECR_RPRT_TYPS.EJ_Z_DETAIL));
            return this;
        }

        public OKC TryPrintEkuReceiptDetailReportWithDatetime(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintEkuReceiptDetailReportWithDatetime");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartDate.Length != 0 && requestMembers.EndDate.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.EJ_R_DETAIL_DT));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "StartDate veya EndDate alanları boş olamaz.");
            return this;
        }

        public OKC TryPrintEkuReceiptDetailReportWithZNoAndReceiptNo(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintEkuReceiptDetailReportWithZNoAndReceiptNo");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartZNo.Length != 0 && requestMembers.EndZNo.Length != 0 &&
                (requestMembers.StartReceiptNo.Length != 0 && requestMembers.EndReceiptNo.Length != 0))
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.EJ_R_DETAIL_ZR));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "StartZNo,EndZNo,StartReceiptNo,EndReceiptNo alanları boş olamaz.");
            return this;
        }

        public OKC TryPrintFinancalZDetailReportWithDateTime(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintFinancalZDetailReportWithDateTime");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartDate.Length != 0 && requestMembers.EndDate.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.FM_Z_DTL_DD));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "StartDate veya EndDate alanları boş olamaz.");
            return this;
        }

        public OKC TryPrintFinancalZDetailReportWithZNo(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintFinancalZDetailReportWithZNo");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartZNo.Length != 0 && requestMembers.EndZNo.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.FM_Z_DTL_ZZ));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "StartZNo,EndZNo, alanları boş olamaz.");
            return this;
        }

        public OKC TryPrintFinancalZReportWithDateTime(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintFinancalZReportWithDateTime");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartDate.Length != 0 && requestMembers.EndDate.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.FM_Z_SMRY_DD));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "StartDate veya EndDate alanları boş olamaz.");
            return this;
        }

        public OKC TryPrintFinancalZReportWithZNo(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintFinancalZReportWithZNo");
            if(!CheckOkcConnection())
                return this;
            if(requestMembers.StartZNo.Length != 0 && requestMembers.EndZNo.Length != 0)
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.FM_Z_SMRY_ZZ));
            Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                "StartZNo,EndZNo, alanları boş olamaz.");
            return this;
        }

        public OKC TryPrintLastSaleReceiptCopy()
        {
            MethodInit(Request, "TryPrintLastSaleReceiptCopy");
            if(!CheckOkcConnection())
                return TryPrintReport(new Members(), string.Format("{0:x3}", ECR_RPRT_TYPS.EJ_R_SNGL_CPY_LASTSALE));
            return this;
        }

        public OKC TryPrintSalesReportWihtZNo(Members requestMembers)
        {
            MethodInit(requestMembers, "TryPrintSalesReportWihtZNo");
            if(!CheckOkcConnection())
                return TryPrintReport(requestMembers, string.Format("{0:x3}", ECR_RPRT_TYPS.FM_SALE_Z));
            return this;
        }

        public OKC TryPrintBankEOD()
        {
            MethodInit(Request, "TryPrintBankEOD");
            if(!CheckOkcConnection())
                return TryPrintReport(new Members(), string.Format("{0:x3}", ECR_RPRT_TYPS.BANK_EOD));
            return this;
        }

        public OKC TryPrintBankSlipCopy(Members requestMembers)
        {
            string str;
            MethodInit(requestMembers, "TryPrintBankSlipCopy");
            if(!CheckOkcConnection())
            {
                return this;
            }

            string processType = requestMembers.ProcessType;
            if(processType == "1" || processType == "2" || processType == "3")
            {
                if(string.IsNullOrEmpty(requestMembers.ZNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "ZNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.ReceiptNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "ReceiptNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.EJNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "EJNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.BatchNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "BatchNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.StanNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "StanNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.AcquirerId))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "AcquirerId"));
                    return this;
                }
            }
            else
            {
                if(processType != "4")
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde hatalı {0}.", "ProcessType"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.BatchNum))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "BatchNum"));
                    return this;
                }

                if(string.IsNullOrEmpty(requestMembers.AcquirerId))
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde {0} alanı dolu olmalıdır.", "AcquirerId"));
                    return this;
                }
            }

            processType = requestMembers.ProcessType;
            if(processType == "1")
            {
                str = string.Format("{0:x3}", ECR_RPRT_TYPS.BANK_CUSTOMER_SLIP_COPY);
            }
            else if(processType == "2")
            {
                str = string.Format("{0:x3}", ECR_RPRT_TYPS.BANK_MERCHANT_SLIP_COPY);
            }
            else if(processType == "3")
            {
                str = string.Format("{0:x3}", ECR_RPRT_TYPS.BANK_BOTH_SLIP_COPY);
            }
            else
            {
                if(processType != "4")
                {
                    Helpers.Conditional.SetCustomWarningInformation(ref _processInformation,
                        string.Format("PrintBankSlipCopy işleminde hatalı {0}.", "ProcessType"));
                    return this;
                }

                str = string.Format("{0:x3}", ECR_RPRT_TYPS.BANK_EOD_SLIP_COPY);
            }

            return TryPrintReport(requestMembers, str);
        }
    }
}