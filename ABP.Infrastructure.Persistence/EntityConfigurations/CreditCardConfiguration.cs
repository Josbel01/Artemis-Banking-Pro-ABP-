using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("CreditCards");
            #endregion

            #region Property configurations
            builder.Property(c => c.CardNumber).IsRequired().HasMaxLength(16);
            builder.HasIndex(c => c.CardNumber).IsUnique();
            builder.Property(c => c.ClientId).IsRequired();
            builder.Property(c => c.CreditLimit).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(c => c.CurrentDebt).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(c => c.ExpirationDate).IsRequired();
            builder.Property(c => c.Cvc).IsRequired();
            builder.Property(c => c.Status).IsRequired();
            builder.Property(c => c.AssignedByUserId).IsRequired();
            #endregion

            #region relationships
            builder.HasMany<CardTransaction>(c => c.CardTransactions)
                .WithOne(ct => ct.CreditCard)
                .HasForeignKey(ct => ct.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
