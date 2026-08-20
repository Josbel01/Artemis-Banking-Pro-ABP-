using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class SavingAccountConfiguration : IEntityTypeConfiguration<SavingAccount>
    {
        public void Configure(EntityTypeBuilder<SavingAccount> builder)
        {
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("SavingAccounts");
            #endregion

            #region Property configurations
            builder.Property(s => s.AccountNumber).IsRequired().HasMaxLength(9);
            builder.HasIndex(s => s.AccountNumber).IsUnique();
            builder.Property(s => s.ClientId).IsRequired();
            builder.Property(s => s.Balance).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(s => s.AccountType).IsRequired();
            builder.Property(s => s.Status).IsRequired();
            builder.Property(s => s.AssignedByUserId).IsRequired();
            #endregion

            #region relationships
            builder.HasMany<Transaction>(s => s.Transactions)
                .WithOne(t => t.SavingAccount)
                .HasForeignKey(t => t.SavingAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
