using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Loans");
            #endregion

            #region Property configurations
            builder.Property(l => l.LoanNumber).IsRequired().HasMaxLength(9);
            builder.HasIndex(l => l.LoanNumber).IsUnique();
            builder.Property(l => l.ClientId).IsRequired();
            builder.Property(l => l.AmountApproved).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(l => l.AmountPending).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(l => l.AnnualInterestRate).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(l => l.TermInMonths).IsRequired();
            builder.Property(l => l.Status).IsRequired();
            builder.Property(l => l.AssignedByUserId).IsRequired();
            builder.Property(l => l.TotalInstallments).IsRequired();
            builder.Property(l => l.PaidInstallments).IsRequired();
            builder.Property(l => l.ClientPaymentStatus).IsRequired();
            #endregion

            #region relationships
            builder.HasMany<LoanInstallment>(l => l.LoanInstallments)
                .WithOne(li => li.Loan)
                .HasForeignKey(li => li.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
