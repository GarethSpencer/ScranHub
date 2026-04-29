using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(30);
        builder.Property(u => u.Email).HasMaxLength(256);

        builder.HasData(new User
        {
            UserId = SeedConstants.AdminId,
            DisplayName = "Admin User",
            Email = "admin@example.com",
            Active = true,
            Admin = true,
            CreatedOn = SeedConstants.CreatedDate,
            CreatedBy = SeedConstants.AdminId
        },
        new User
        {
            UserId = SeedConstants.NonAdminId,
            DisplayName = "Non-Admin User",
            Email = "nonadmin@example.com",
            Active = true,
            Admin = false,
            CreatedOn = SeedConstants.CreatedDate,
            CreatedBy = SeedConstants.AdminId
        }
        );
    }
}
