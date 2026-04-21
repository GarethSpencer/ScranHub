using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class GroupVenueConfiguration : IEntityTypeConfiguration<GroupVenue>
{
    public void Configure(EntityTypeBuilder<GroupVenue> builder)
    {
        builder.Property(gv => gv.VenueName).HasMaxLength(50);

        builder.HasOne(gv => gv.Group)
            .WithMany(g => g.GroupVenues)
            .HasForeignKey(gv => gv.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gv => gv.FoodTypeOption)
            .WithMany(ft => ft.GroupVenues)
            .HasForeignKey(gv => gv.FoodTypeOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gv => gv.VenueTypeOption)
            .WithMany(vt => vt.GroupVenues)
            .HasForeignKey(gv => gv.VenueTypeOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
