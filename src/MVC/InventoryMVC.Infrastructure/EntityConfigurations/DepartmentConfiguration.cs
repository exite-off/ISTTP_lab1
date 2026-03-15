using InventoryMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryMVC.Infrastructure.EntityConfigurations;

internal class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Address).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Phone).IsRequired().HasMaxLength(20);

        builder.HasMany(d => d.ResponsiblePersons)
            .WithOne(rp => rp.Department)
            .HasForeignKey(rp => rp.DepartmentId);

        builder.HasMany(d => d.Rooms)
            .WithOne(r => r.Department)
            .HasForeignKey(r => r.DepartmentId);
    }
}
