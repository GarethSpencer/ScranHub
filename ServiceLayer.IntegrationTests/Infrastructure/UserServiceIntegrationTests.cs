using DAL.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Utilities.Models.Requests.Users;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class UserServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;

    private UserService? _service;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        var tokenData = new Mock<ITokenData>();
        tokenData.Setup(x => x.UserId).Returns(TestAdminId);

        _service = new UserService(
            tokenData: tokenData.Object,
            logger: new FakeLogger<UserService>(),
            userRepository: new UserRepository(_context),
            userFriendRepository: new UserFriendRepository(_context),
            unitOfWork: new UnitOfWork(_context, tokenData.Object)
        );
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailAlreadyExists_ReturnsConflict()
    {
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Admin = true,
            DisplayName = "Admin User 1"
        };

        var result = await _service!.CreateUserAsync(request, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
