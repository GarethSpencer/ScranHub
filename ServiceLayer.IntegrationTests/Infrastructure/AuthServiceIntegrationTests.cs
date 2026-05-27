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
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class AuthServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<AuthService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private AuthService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<AuthService>();

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

        _service = new AuthService(
            userRepository: new UserRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object),
            logger: _logger
        );
    }

    #region ResolveUserAsync
    [Theory]
    [InlineData("", null)]
    [InlineData("invalid123", null)]
    public async Task ResolveUserAsync_InvalidAuthIdNullEmail_ReturnsNoUser(string authId, string? email)
    {
        var (UserId, IsAdmin) = await _service!.ResolveUserAsync(authId, email, ct);

        UserId.Should().BeNull();
        IsAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] not resolved for AuthId [{authId}] email [(null)].");
    }

    [Theory]
    [InlineData(TestAuthId, TestUser2NonAdminEmail)]
    [InlineData(TestAuthId, "fake@email.com")]
    [InlineData(TestAuthId, "")]
    [InlineData(TestAuthId, null)]
    public async Task ResolveUserAsync_ValidAuthIdAnyEmail_ReturnsCorrectUser(string authId, string? email)
    {
        var (UserId, IsAdmin) = await _service!.ResolveUserAsync(authId, email, ct);
        
        UserId.Should().Be(TestUser2NonAdminId);
        IsAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("invalid123", "fake@email.com")]
    [InlineData("", "fake@email.com")]
    [InlineData("invalid123", "null")]
    [InlineData("", "null")]
    public async Task ResolveUserAsync_InvalidAuthIdInvalidEmail_ReturnsNoUser(string authId, string? email)
    {
        var (UserId, IsAdmin) = await _service!.ResolveUserAsync(authId, email, ct);

        UserId.Should().BeNull();
        IsAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] not resolved for AuthId [{authId}] email [{email}].");
    }

    [Theory]
    [InlineData("invalid123", TestUser2NonAdminEmail)]
    [InlineData("", TestUser2NonAdminEmail)]
    public async Task ResolveUserAsync_InvalidAuthIdValidEmail_ReturnsUserAddsAuthId(string authId, string? email)
    {
        var (UserId, IsAdmin) = await _service!.ResolveUserAsync(authId, email, ct);

        UserId.Should().Be(TestUser2NonAdminId);
        IsAdmin.Should().BeFalse();
        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] resolved successfully for [{TestUser2NonAdminId}] and AuthId value set.");

        var user = _context!.Users.Where(x => x.UserId == TestUser2NonAdminId).Single();
        user.AuthId.Should().Be(authId);
    }

    [Theory]
    [InlineData("invalid123", TestUser1AdminEmail)]
    [InlineData("", TestUser1AdminEmail)]
    public async Task ResolveUserAsync_InvalidAuthIdValidEmailNewUser_ReturnsUserAddsAuthId(string authId, string? email)
    {
        var (UserId, IsAdmin) = await _service!.ResolveUserAsync(authId, email, ct);

        UserId.Should().Be(TestUser1AdminId);
        IsAdmin.Should().BeTrue();
        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] resolved successfully for [{TestUser1AdminId}] and AuthId value set.");

        var user = _context!.Users.Where(x => x.UserId == TestUser1AdminId).Single();
        user.AuthId.Should().Be(authId);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
