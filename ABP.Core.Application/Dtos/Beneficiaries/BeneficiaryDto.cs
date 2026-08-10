namespace ABP.Core.Application.Dtos.Beneficiaries
{
    public class BeneficiaryDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryLastName { get; set; } = string.Empty;
    }
}
