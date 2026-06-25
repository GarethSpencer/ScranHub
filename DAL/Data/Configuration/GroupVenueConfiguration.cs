using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class GroupVenueConfiguration : IEntityTypeConfiguration<GroupVenue>
{
    public void Configure(EntityTypeBuilder<GroupVenue> builder)
    {
        builder.Property(gv => gv.VenueName).HasMaxLength(50);
        builder.Property(gv => gv.GooglePlaceId).HasMaxLength(255);
        builder.Property(gv => gv.FormattedAddress).HasMaxLength(512);
        builder.Property(gv => gv.Latitude).HasPrecision(8, 6);
        builder.Property(gv => gv.Longitude).HasPrecision(9, 6);

        builder.HasOne(gv => gv.Group)
            .WithMany(g => g.GroupVenues)
            .HasForeignKey(gv => gv.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gv => gv.FoodTypeOption)
            .WithMany(ft => ft.GroupVenues)
            .HasForeignKey(gv => gv.FoodTypeOptionId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(gv => gv.VenueTypeOption)
            .WithMany(vt => vt.GroupVenues)
            .HasForeignKey(gv => gv.VenueTypeOptionId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
