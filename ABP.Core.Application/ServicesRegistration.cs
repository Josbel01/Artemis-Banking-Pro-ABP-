using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ABP.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(config =>
            {
                config.AddMaps(Assembly.GetExecutingAssembly());
            });
            #endregion

            #region Services IOC
            services.AddTransient<IBeneficiaryService, BeneficiaryService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<ICashierService, CashierService>();
            services.AddTransient<ISavingAccountService, SavingAccountService>();
            services.AddTransient<ICreditCardService, CreditCardService>();
            services.AddTransient<ILoanService, LoanService>();
            services.AddTransient<ICardTransactionService, CardTransactionService>();
            services.AddTransient<ILoanInstallmentService, LoanInstallmentService>();
            #endregion
        }
    }
}
