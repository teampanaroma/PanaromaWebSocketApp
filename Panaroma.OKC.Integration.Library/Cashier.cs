using System.ComponentModel;

namespace Panaroma.OKC.Integration.Library
{
    public class Cashier
    {
        [Description("Zorunlu alan. 2 karakterden uzun olamaz.")]
        public string Id { get; set; }

        [Description("Zorunlu alan. 4 karakterden uzun olamaz.")]
        public string Password { get; set; }
    }
}