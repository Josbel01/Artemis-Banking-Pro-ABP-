using System;
using System.Collections.Generic;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class CreditCard : BasicEntity<int>
    {
        public string CardNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public string ExpirationDate { get; set; } = string.Empty; // Format: MM/AA
        public string Cvc { get; set; } = string.Empty;
        public CreditCardStatus Status { get; set; }
        public string AssignedByUserId { get; set; } = string.Empty; // Administrador responsable de la asignación

        public ICollection<CardTransaction> CardTransactions { get; set; } = new List<CardTransaction>();
    }
}
