using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ABP.Infrastructure.Persistence.Contexts
{
    public class ArtemisBankingAppContext : DbContext
    {
        public ArtemisBankingAppContext(DbContextOptions<ArtemisBankingAppContext> options) : base(options) { }

        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<CardTransaction> CardTransactions { get; set; }
        public DbSet<Commerce> Commerces { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<SavingAccount> SavingAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
