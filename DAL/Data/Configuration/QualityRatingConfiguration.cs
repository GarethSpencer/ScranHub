using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class QualityRatingConfiguration : IEntityTypeConfiguration<QualityRating>
{
    public void Configure(EntityTypeBuilder<QualityRating> builder)
    {
        builder.HasOne(co => co.GroupVenue)
            .WithMany(g => g.QualityRatings)
            .HasForeignKey(co => co.GroupVenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.User)
            .WithMany(u => u.QualityRatings)
            .HasForeignKey(co => co.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.QualityOption)
            .WithMany(u => u.QualityRatings)
            .HasForeignKey(co => co.QualityOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
