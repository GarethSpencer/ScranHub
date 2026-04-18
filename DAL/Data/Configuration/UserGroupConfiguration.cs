using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
