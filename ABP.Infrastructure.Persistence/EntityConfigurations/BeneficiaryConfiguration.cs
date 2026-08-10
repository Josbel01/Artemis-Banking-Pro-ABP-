using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            //Fluent api
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Beneficiaries");
            #endregion

            #region Property configurations
            builder.Property(b => b.ClientId).IsRequired();
            builder.Property(b => b.BeneficiaryAccountNumber).IsRequired().HasMaxLength(9);
            builder.Property(b => b.BeneficiaryName).IsRequired();
            builder.Property(b => b.BeneficiaryLastName).IsRequired();
            builder.HasIndex(b => new { b.ClientId, b.BeneficiaryAccountNumber }).IsUnique();
            #endregion
        }
    }
}
