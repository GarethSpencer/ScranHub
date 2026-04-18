using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class FoodTypeOptionConfiguration : IEntityTypeConfiguration<FoodTypeOption>
{
    public void Configure(EntityTypeBuilder<FoodTypeOption> builder)
    {
        builder.HasOne(ft => ft.Group)
            .WithMany(g => g.FoodTypeOptions)
            .HasForeignKey(ft => ft.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000001"), GroupId = null, Label = "Café",     DisplayOrder = 1, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000002"), GroupId = null, Label = "Burgers",  DisplayOrder = 2, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000003"), GroupId = null, Label = "Chicken",  DisplayOrder = 3, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000004"), GroupId = null, Label = "Pizza",    DisplayOrder = 4, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000005"), GroupId = null, Label = "Oriental", DisplayOrder = 5, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000006"), GroupId = null, Label = "Other",    DisplayOrder = 6, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId }
        );
    }
}
