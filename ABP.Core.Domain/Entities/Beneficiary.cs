using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class Beneficiary : BasicEntity<int>
    {
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryLastName { get; set; } = string.Empty;
    }
}
