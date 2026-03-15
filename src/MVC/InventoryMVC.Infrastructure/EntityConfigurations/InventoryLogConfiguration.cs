using InventoryMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryMVC.Infrastructure.EntityConfigurations;

internal class InventoryLogConfiguration : IEntityTypeConfiguration<InventoryLog>
{
    public void Configure(EntityTypeBuilder<InventoryLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ActionDate).IsRequired();
        builder.Property(l => l.ActionType).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Description).HasMaxLength(500);

        // SetNull — log entry stays after person is deleted
        builder.HasOne(l => l.OldResponsiblePerson)
            .WithMany()
            .HasForeignKey(l => l.OldResponsiblePersonId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.NewResponsiblePerson)
            .WithMany()
            .HasForeignKey(l => l.NewResponsiblePersonId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
