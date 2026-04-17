using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class ScranHubDbContext(DbContextOptions<ScranHubDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserFriend> UserFriends => Set<UserFriend>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFriend>()
            .HasOne(uf => uf.User)
            .WithMany(uf => uf.InitiatedFriendships)
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFriend>()
            .HasOne(uf => uf.Friend)
            .WithMany(uf => uf.ReceivedFriendships)
            .HasForeignKey(uf => uf.FriendId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
