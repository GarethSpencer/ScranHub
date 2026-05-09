using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class FoodTypeOptionConfiguration : IEntityTypeConfiguration<FoodTypeOption>
{
    public void Configure(EntityTypeBuilder<FoodTypeOption> builder)
    {
        builder.Property(ft => ft.Label).HasMaxLength(30);

        builder.HasOne(ft => ft.Group)
            .WithMany(g => g.FoodTypeOptions)
            .HasForeignKey(ft => ft.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000001"), GroupId = null, Label = "Café",     CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000002"), GroupId = null, Label = "Burgers",  CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000003"), GroupId = null, Label = "Chicken",  CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000004"), GroupId = null, Label = "Pizza",    CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000005"), GroupId = null, Label = "Oriental", CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000006"), GroupId = null, Label = "Other",    CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId }
        );
    }
}
