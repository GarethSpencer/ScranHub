using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class CostRatingConfiguration : IEntityTypeConfiguration<CostRating>
{
    public void Configure(EntityTypeBuilder<CostRating> builder)
    {
        builder.HasOne(co => co.GroupVenue)
            .WithMany(g => g.CostRatings)
            .HasForeignKey(co => co.GroupVenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.User)
            .WithMany(u => u.CostRatings)
            .HasForeignKey(co => co.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.CostOption)
            .WithMany(u => u.CostRatings)
            .HasForeignKey(co => co.CostOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
