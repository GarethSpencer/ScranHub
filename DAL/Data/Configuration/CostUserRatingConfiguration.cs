using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class CostUserRatingConfiguration : IEntityTypeConfiguration<CostUserRating>
{
    public void Configure(EntityTypeBuilder<CostUserRating> builder)
    {
        builder.HasOne(co => co.GroupVenue)
            .WithMany(g => g.CostUserRatings)
            .HasForeignKey(co => co.GroupVenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.User)
            .WithMany(u => u.CostUserRatings)
            .HasForeignKey(co => co.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(co => co.CostOption)
            .WithMany(u => u.CostUserRatings)
            .HasForeignKey(co => co.CostOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
