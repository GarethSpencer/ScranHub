using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class ScranHubDbContext(DbContextOptions<ScranHubDbContext> options) : DbContext(options)
{
    // DbSets will be added here as you build out entities, e.g:
    // public DbSet<Recipe> Recipes => Set<Recipe>();
}
