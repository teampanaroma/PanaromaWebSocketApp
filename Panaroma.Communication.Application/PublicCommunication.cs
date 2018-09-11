using Newtonsoft.Json;

namespace Panaroma.Communication.Application
{
    internal class PublicCommunication
    {
        public bool IsSuccess { get; set; }

        public object Results { get; set; }

        public string Method { get; set; }

        public static string ConvertFromInternalCommunication(InternalCommunication internalCommunication)
        {
            return JsonConvert.SerializeObject(new PublicCommunication()
            {
                IsSuccess = internalCommunication.IsSuccess,
                Results = internalCommunication.Results,
                Method = internalCommunication.Method
            });
        }
    }
}