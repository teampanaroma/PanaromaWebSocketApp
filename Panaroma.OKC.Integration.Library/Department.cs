using System.ComponentModel;

namespace Panaroma.OKC.Integration.Library
{
    public class Department
    {
        [Description("Zorunlu alan.")] public string Id { get; set; }

        [Description("Zorunlu alan.")] public VATGroup VatGroup { get; set; }

        [Description("Zorunlu alan.")] public string DepartmentName { get; set; }

        [Description("Zorunlu alan.")] public string Amount { get; set; }

        [Description("Zorunlu alan.")] public string LimitAmount { get; set; }

        public Unit Unit { get; set; }
    }
}