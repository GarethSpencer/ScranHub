using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class CostOptionConfiguration : IEntityTypeConfiguration<CostOption>
{
    public void Configure(EntityTypeBuilder<CostOption> builder)
    {
        builder.Property(ft => ft.Label).HasMaxLength(30);

        builder.HasOne(co => co.Group)
            .WithMany(g => g.CostOptions)
            .HasForeignKey(co => co.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new CostOption { CostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000001"), GroupId = null, Label = "Cheap",      DisplayOrder = 1, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new CostOption { CostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000002"), GroupId = null, Label = "Reasonable", DisplayOrder = 2, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId },
            new CostOption { CostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000003"), GroupId = null, Label = "Pricey",     DisplayOrder = 3, CreatedOn = SeedConstants.CreatedDate, CreatedBy = SeedConstants.AdminId }
        );
    }
}
