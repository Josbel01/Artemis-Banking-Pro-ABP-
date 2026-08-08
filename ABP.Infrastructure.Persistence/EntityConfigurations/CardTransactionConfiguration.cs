using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class CardTransactionConfiguration : IEntityTypeConfiguration<CardTransaction>
    {
        public void Configure(EntityTypeBuilder<CardTransaction> builder)
        {
            //Fluent api
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("CardTransactions");
            #endregion

            #region Property configurations
            builder.Property(ct => ct.CreditCardId).IsRequired();
            builder.Property(ct => ct.TransactionDate).IsRequired();
            builder.Property(ct => ct.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(ct => ct.CommerceName).IsRequired();
            builder.Property(ct => ct.Status).IsRequired();
            #endregion

            #region relationships
            builder.HasOne(ct => ct.CreditCard)
                .WithMany(c => c.CardTransactions)
                .HasForeignKey(ct => ct.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ct => ct.Commerce)
                .WithMany()
                .HasForeignKey(ct => ct.CommerceId)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion
        }
    }
}
