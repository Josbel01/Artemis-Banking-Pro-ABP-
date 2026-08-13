using ABP.Core.Domain.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.CreditCards
{
    public class CreditCardViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Tarjeta")]
        public string CardNumber { get; set; } = string.Empty;
        
        [Display(Name = "Cliente")]
        public string ClientId { get; set; } = string.Empty;
        
        public string ClientName { get; set; } = string.Empty; // For UI
        
        [Display(Name = "Límite de Crédito")]
        [DataType(DataType.Currency)]
        public decimal CreditLimit { get; set; }
        
        [Display(Name = "Deuda Actual")]
        [DataType(DataType.Currency)]
        public decimal CurrentDebt { get; set; }
        
        [Display(Name = "Crédito Disponible")]
        [DataType(DataType.Currency)]
        public decimal AvailableCredit { get; set; }
        
        [Display(Name = "Fecha de Expiración")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }
        
        [Display(Name = "Estado")]
        public CreditCardStatus Status { get; set; }
    }
}
