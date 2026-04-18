using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configuration;

public class UserFriendConfiguration : IEntityTypeConfiguration<UserFriend>
{
    public void Configure(EntityTypeBuilder<UserFriend> builder)
    {
        builder.HasOne(uf => uf.User)
            .WithMany(u => u.InitiatedFriendships)
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(uf => uf.Friend)
            .WithMany(u => u.ReceivedFriendships)
            .HasForeignKey(uf => uf.FriendId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
