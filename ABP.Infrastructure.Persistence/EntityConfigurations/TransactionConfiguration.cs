using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Transactions");
            #endregion

            #region Property configurations
            builder.Property(t => t.SavingAccountId).IsRequired();
            builder.Property(t => t.TransactionDate).IsRequired();
            builder.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.Type).IsRequired();
            builder.Property(t => t.Beneficiary).IsRequired();
            builder.Property(t => t.Origin).IsRequired();
            builder.Property(t => t.Status).IsRequired();
            #endregion

            #region relationships
            builder.HasOne<SavingAccount>(t => t.SavingAccount)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.SavingAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
