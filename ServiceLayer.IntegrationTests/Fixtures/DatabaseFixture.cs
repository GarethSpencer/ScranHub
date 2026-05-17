using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace ServiceLayer.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public MsSqlContainer Db { get; } = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => Db.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Db.StartAsync();

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context = new ScranHubDbContext(options);
        await context.Database.MigrateAsync();
        await SetupDatabase(context);
    }

    public async Task DisposeAsync() => await Db.DisposeAsync();

    private static async Task SetupDatabase(ScranHubDbContext context)
    {
        context!.Users.Add(new User
        {
            Email = "test@example.com",
            Active = true,
            Admin = true,
            DisplayName = "Test Admin 1",

        });
        await context.SaveChangesAsync();
    }
}
