using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(new User
        {
            UserId = SeedConstants.AdminId,
            DisplayName = "Admin User",
            Active = true,
            Admin = true,
            CreatedOn = SeedConstants.CreatedDate,
            CreatedBy = SeedConstants.AdminId
        });
    }
}
