using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.Property(g => g.GroupName).HasMaxLength(30);
        builder.Property(g => g.Icon).HasMaxLength(32);

        builder.HasOne(ft => ft.CreatedByUser)
            .WithMany(g => g.CreatedGroups)
            .HasForeignKey(ft => ft.CreatedBy)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
