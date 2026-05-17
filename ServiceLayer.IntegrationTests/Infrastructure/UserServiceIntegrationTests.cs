using DAL.Data;
using DAL.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Testcontainers.MsSql;
using Utilities.Models.Requests.Users;
using Utilities.Token;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
public class UserServiceIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .WithCleanUp(true)
        .Build();
    private ScranHubDbContext? _context;
    private UserService? _service;
    
    private readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_db.GetConnectionString())
            .Options;

        _context = new ScranHubDbContext(options);
        await _context.Database.MigrateAsync();

        await SetupDatabase();

        var tokenData = new Mock<ITokenData>();
        tokenData.Setup(x => x.UserId).Returns(TestUserId);
        var logger = new FakeLogger<UserService>();
        var userRepository = new UserRepository(_context);
        var userFriendRepository = new UserFriendRepository(_context);
        var unitOfWork = new UnitOfWork(_context, tokenData.Object);

        _service = new UserService(
            tokenData: tokenData.Object,
            logger: logger,
            userRepository: userRepository,
            userFriendRepository: userFriendRepository,
            unitOfWork: unitOfWork
        );
    }

    private async Task SetupDatabase()
    {
        _context!.Users.Add(new User
        {
            Email = "test@example.com",
            Active = true,
            Admin = true,
            DisplayName = "Test Admin 1",

        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailAlreadyExists_ReturnsConflict()
    {
        var request = new CreateUserRequest {
            Email = "test@example.com",
            Admin = true,
            DisplayName = "Admin User 1"
        };

        var result = await _service!.CreateUserAsync(request, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    public async Task DisposeAsync()
    {
        await _context!.DisposeAsync();
        await _db.DisposeAsync();
    }
}
