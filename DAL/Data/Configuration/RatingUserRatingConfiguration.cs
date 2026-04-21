using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class RatingUserRatingConfiguration : IEntityTypeConfiguration<RatingUserRating>
{
    public void Configure(EntityTypeBuilder<RatingUserRating> builder)
    {
        builder.HasOne(co => co.GroupVenue)
            .WithMany(g => g.RatingUserRatings)
            .HasForeignKey(co => co.GroupVenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(co => co.User)
            .WithMany(u => u.RatingUserRatings)
            .HasForeignKey(co => co.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(co => co.RatingOption)
            .WithMany(u => u.RatingUserRatings)
            .HasForeignKey(co => co.RatingOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
