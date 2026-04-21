using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class RatingOptionConfiguration : IEntityTypeConfiguration<RatingOption>
{
    public void Configure(EntityTypeBuilder<RatingOption> builder)
    {
        builder.Property(ft => ft.Label).HasMaxLength(30);

        builder.HasOne(ro => ro.Group)
            .WithMany(g => g.RatingOptions)
            .HasForeignKey(ro => ro.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000001"), GroupId = null, Label = "Great",   DisplayOrder = 1, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000002"), GroupId = null, Label = "Good",    DisplayOrder = 2, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000003"), GroupId = null, Label = "Average", DisplayOrder = 3, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000004"), GroupId = null, Label = "Poor",    DisplayOrder = 4, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId }
        );
    }
}
