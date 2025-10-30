using CorporateSystem.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateSystem.Auth.Domain.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Token).IsRequired();
            builder.Property(rt => rt.IpAddress).IsRequired();
            builder.Property(rt => rt.ExpiryOn).IsRequired();

            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("IX_RefreshToken_Token");
        }
    }
}
