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
    [InlineData(TestAuthId, TestUser2NonAdminEmail)]
    [InlineData(TestAuthId, null)]
    public async Task ResolveUserAsync_ValidAuthIdValidOrNotProvidedEmail_ReturnsCorrectUser(string authId, string? email)
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync(authId, email, ct);

        userId.Should().Be(TestUser2NonAdminId);
        isAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(0);
    }

    [Fact]
    public async Task ResolveUserAsync_ValidAuthIdNewEmail_ReturnsCorrectUserUpdatesEmail()
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync(TestAuthId, "new@email.com", ct);

        userId.Should().Be(TestUser2NonAdminId);
        isAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] email updated for [{userId}] from Auth0 token.");
    }

    [Fact]
    public async Task ResolveUserAsync_ValidAuthIdAnotherUsersEmail_ReturnsUnresolvedUserWithWarning()
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync(TestAuthId, TestUser1AdminEmail, ct);

        userId.Should().BeNull();
        isAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] AuthId [{TestAuthId}] email [{TestUser1AdminEmail}] already belongs to another user.");
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("nomatch123", null)]
    public async Task ResolveUserAsync_NoMatchingAuthIdNullEmail_ReturnsUnresolvedUserWithWarning(string authId, string? email)
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync(authId, email, ct);

        userId.Should().BeNull();
        isAdmin.Should().BeFalse();

        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] not resolved for AuthId [{authId}] as email is not provided and no user found with the authId.");
    }

    [Fact]
    public async Task ResolveUserAsync_NewAuthIdValidEmail_ReturnsCorrectUserAddsAuthId()
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync("validnew123", TestUser1AdminEmail, ct);

        userId.Should().Be(TestUser1AdminId);
        isAdmin.Should().BeTrue();

        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] resolved successfully for [{userId}] and AuthId value set.");

        var user = _context!.Users.Where(x => x.UserId == TestUser1AdminId).Single();
        user.AuthId.Should().Be("validnew123");
    }

    [Fact]
    public async Task ResolveUserAsync_InvalidAuthIdValidEmail_ReturnsUnresolvedUserWithWarning()
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync("valid123", TestUser2NonAdminEmail, ct);

        userId.Should().BeNull();
        isAdmin.Should().BeFalse();
        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] not resolved for AuthId [valid123] email [{TestUser2NonAdminEmail}] as the authId does not match the database value.");

        var user = _context!.Users.Where(x => x.UserId == TestUser2NonAdminId).Single();
        user.AuthId.Should().Be(TestAuthId);
    }

    [Fact]
    public async Task ResolveUserAsync_ValidAuthIdValidEmailNewUser_ReturnsCorrectUserAddsNewUser()
    {
        var (userId, isAdmin) = await _service!.ResolveUserAsync("valid123", "newuser@example.com", ct);

        userId.Should().NotBeNull();
        isAdmin.Should().BeFalse();
        _logger.Entries.Should().HaveCount(1);
        _logger.Entries.Should().ContainSingle(e => e.Message == $"[ResolveUserAsync] resolved for AuthId [valid123] email [newuser@example.com] and new user [{userId}] created.");

        var user = _context!.Users.Where(x => x.UserId == userId).Single();
        user.AuthId.Should().Be("valid123");
        user.Admin.Should().BeFalse();
        user.Email.Should().Be("newuser@example.com");
        user.DisplayName.Should().Be("newuser");
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
