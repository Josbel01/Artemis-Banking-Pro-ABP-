using ABP.Core.Domain.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.CardTransactions
{
    public class CardTransactionViewModel
    {
        public int Id { get; set; }
        
        public int CreditCardId { get; set; }
        
        [Display(Name = "Comercio")]
        public string CommerceName { get; set; } = string.Empty;
        
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }
        
        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
        
        [Display(Name = "Estado")]
        public TransactionStatus Status { get; set; }
    }
}
