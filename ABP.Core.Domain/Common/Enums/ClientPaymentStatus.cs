namespace ABP.Core.Domain.Common.Enums
{
    public enum ClientPaymentStatus
    {
        UpToDate = 0,    // Al día
        Late = 1,        // Atrasado
        Defaulted = 2    // En mora
    }
}
