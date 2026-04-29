using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class VenueTypeOptionConfiguration : IEntityTypeConfiguration<VenueTypeOption>
{
    public void Configure(EntityTypeBuilder<VenueTypeOption> builder)
    {
        builder.Property(ft => ft.Label).HasMaxLength(30);

        builder.HasOne(vt => vt.Group)
            .WithMany(g => g.VenueTypeOptions)
            .HasForeignKey(vt => vt.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new VenueTypeOption { VenueTypeOptionId = Guid.Parse("00000000-0000-0000-0004-000000000001"), GroupId = null, Label = "Eat-In",   DisplayOrder = 1, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new VenueTypeOption { VenueTypeOptionId = Guid.Parse("00000000-0000-0000-0004-000000000002"), GroupId = null, Label = "Takeaway", DisplayOrder = 2, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new VenueTypeOption { VenueTypeOptionId = Guid.Parse("00000000-0000-0000-0004-000000000003"), GroupId = null, Label = "Both",     DisplayOrder = 3, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId }
        );
    }
}
