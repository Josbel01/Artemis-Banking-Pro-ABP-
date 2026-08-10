namespace ABP.Core.Application.ViewModels.Beneficiaries
{
    public class BeneficiaryViewModel : BasicViewModel<int>
    {
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryLastName { get; set; } = string.Empty;
    }
}
