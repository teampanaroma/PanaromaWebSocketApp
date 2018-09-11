using System;
using System.Collections.Generic;

namespace Panaroma.OKC.Integration.Library
{
    public class BankResponse
    {
        private static readonly Dictionary<string, Bank> _panaromaSourceBanks = new Dictionary<string, Bank>()
        {
            {
                "0046",
                new Bank(2, "Akbank")
            },
            {
                "0062",
                new Bank(21, "Garanti")
            },
            {
                "0067",
                new Bank(52, "Yapı Kredi")
            },
            {
                "0064",
                new Bank(29, "İş Bankası")
            },
            {
                "0123",
                new Bank(24, "HSBC")
            },
            {
                "0203",
                new Bank(4, "Albaraka Türk Katılım Bankası")
            },
            {
                "0124",
                new Bank(5, "Alternatif Bank")
            },
            {
                "0135",
                new Bank(6, "Anadolu Bank")
            },
            {
                "0208",
                new Bank(55, "Bank Asya")
            },
            {
                "0134",
                new Bank(15, "Deniz Bank")
            },
            {
                "0111",
                new Bank(20, "Finansbank")
            },
            {
                "0012",
                new Bank(23, "Halkbank")
            },
            {
                "0205",
                new Bank(32, "Kuveyt Türk Katılım Bankası")
            },
            {
                "0099",
                new Bank(26, "ING Bank")
            },
            {
                "0059",
                new Bank(43, "Şekerbank")
            },
            {
                "0096",
                new Bank(46, "Turkish Bank")
            },
            {
                "0206",
                new Bank(49, "Türkiye Finans Katılım Bankası")
            },
            {
                "0015",
                new Bank(51, "Vakıflar Bankası")
            },
            {
                "0010",
                new Bank(53, "Ziraat Bankası")
            }
        };

        private string _acqurierId;

        public string AcquirerId
        {
            get { return _acqurierId; }
            set
            {
                _acqurierId = value;
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("Banka bilgisi alınamadı.");
                Bank bank;
                if (_panaromaSourceBanks.TryGetValue(value, out bank))
                {
                    Name = bank.Name;
                    Id = bank.Id;
                }
                else
                {
                    Name = "Unknow";
                    Id = -100;
                }
            }
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Amount { get; set; }

        public string AuthCode { get; set; }

        public string BatchNum { get; set; }

        public string CardNum { get; set; }

        public string CardType { get; set; }

        public string IssuerId { get; set; }

        public string MerchantId { get; set; }

        public string Posem { get; set; }

        public string ProcessType { get; set; }

        public string ResponseCode { get; set; }

        public string StanNum { get; set; }

        public string TerminalId { get; set; }

        public string TranDate { get; set; }

        public string ReferenceNumber { get; set; }
    }
}