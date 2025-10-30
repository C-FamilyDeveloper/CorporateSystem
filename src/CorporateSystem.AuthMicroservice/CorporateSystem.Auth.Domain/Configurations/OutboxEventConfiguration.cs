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
    public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
    {
        public void Configure(EntityTypeBuilder<OutboxEvent> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.EventType).IsRequired().HasMaxLength(255);
            builder.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.Processed).HasDefaultValue(false);

            builder.HasIndex(x => new { x.Processed, x.CreatedAtUtc })
                .HasDatabaseName("IX_OutboxEvent_Processed_CreatedAt");
        }
    }
}
