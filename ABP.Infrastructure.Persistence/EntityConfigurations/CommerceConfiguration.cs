using ABP.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABP.Infrastructure.Persistence.EntityConfigurations
{
    public class CommerceConfiguration : IEntityTypeConfiguration<Commerce>
    {
        public void Configure(EntityTypeBuilder<Commerce> builder)
        {
            //Fluent api
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Commerces");
            #endregion

            #region Property configurations
            builder.Property(c => c.Name).IsRequired();
            builder.Property(c => c.Email).IsRequired();
            builder.HasIndex(c => c.Email).IsUnique();
            builder.Property(c => c.PhoneNumber).IsRequired();
            builder.Property(c => c.Rnc).IsRequired();
            builder.HasIndex(c => c.Rnc).IsUnique();
            builder.HasIndex(c => c.UserId).IsUnique();
            #endregion
        }
    }
}
