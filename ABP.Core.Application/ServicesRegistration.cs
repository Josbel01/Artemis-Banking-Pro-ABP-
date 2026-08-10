using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ABP.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            // AutoMapper Configuration
            services.AddAutoMapper(config =>
            {
                config.AddMaps(Assembly.GetExecutingAssembly());
            });

            services.AddTransient<IBeneficiaryService, BeneficiaryService>();
            services.AddTransient<ITransactionService, TransactionService>();
        }
    }
}
