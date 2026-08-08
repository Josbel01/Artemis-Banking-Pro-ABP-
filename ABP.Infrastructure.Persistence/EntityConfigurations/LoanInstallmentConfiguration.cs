using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class LoanInstallmentConfiguration : IEntityTypeConfiguration<LoanInstallment>
    {
        public void Configure(EntityTypeBuilder<LoanInstallment> builder)
        {
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("LoanInstallments");
            #endregion

            #region Property configurations
            builder.Property(li => li.LoanId).IsRequired();
            builder.Property(li => li.InstallmentNumber).IsRequired();
            builder.Property(li => li.DueDate).IsRequired();
            builder.Property(li => li.InstallmentAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(li => li.InterestAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(li => li.CapitalAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(li => li.PendingAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(li => li.PaymentStatus).IsRequired();
            builder.Property(li => li.IsLate).IsRequired();
            #endregion

            #region relationships
            builder.HasOne<Loan>(li => li.Loan)
                .WithMany(l => l.LoanInstallments)
                .HasForeignKey(li => li.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
