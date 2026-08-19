using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABP.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration configuration)
        {
            #region Contexts
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ArtemisBankingAppContext>(options =>
                    options.UseInMemoryDatabase("ArtemisBankingDb"));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ArtemisBankingAppContext>((serviceProvider, options) =>
                {
                    options.UseSqlServer(connectionString,
                        m => {
                            m.MigrationsAssembly(typeof(ArtemisBankingAppContext).Assembly.FullName);
                            m.EnableRetryOnFailure();
                        });
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Scoped);
            }
            #endregion

            #region Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISavingAccountRepository, SavingAccountRepository>();
            services.AddScoped<ICreditCardRepository, CreditCardRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ICardTransactionRepository, CardTransactionRepository>();
            services.AddScoped<ICommerceRepository, CommerceRepository>();
            services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
            #endregion
        }
    }
}
