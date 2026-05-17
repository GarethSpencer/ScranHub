using DAL.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;
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
    private FakeLogger<UserService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private UserService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<UserService>();

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        _service = new UserService(
            tokenData: _tokenData.Object,
            logger: _logger,
            userRepository: new UserRepository(_context),
            userFriendRepository: new UserFriendRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }

    #region GetFriendsForUserAsync
    [Fact]
    public async Task GetFriendsForUserAsync_User1ValidFriends_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.OK);
        
        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(SeedUser1AdminId);
        typedResult.Friends?.Count().Should().Be(3);
        typedResult.FriendCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User2ValidFriends_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(SeedUser2NonAdminId);
        typedResult.Friends?.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User3ValidNoFriends_ReturnsNoContent()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.NoContent);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser3AdminId);
        typedResult.Friends?.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User4ValidNoFriends_ReturnsNoContent()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.NoContent);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser4NonAdminId);
        typedResult.Friends?.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User5ValidFriends_ReturnsOk()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser5NonAdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser5NonAdminId);
        typedResult.Friends?.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputFailureCheck(result, "unauthorized", "GetFriendsForUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_InvalidUserId_ReturnsNotFound()
    {
        _tokenData.Setup(x => x.UserId).Returns(Guid.Empty);

        var result = await _service!.GetFriendsForUserAsync(ct);
        OutputFailureCheck(result, "not found", "GetFriendsForUserAsync", HttpStatusCode.NotFound);
    }
    #endregion

    #region CreateUserAsync
    [Fact]
    public async Task CreateUserAsync_ValidNonAdminRequest_ReturnsCreated()
    {
        var request = new CreateUserRequest
        {
            Email = "new@testemail.com",
            Admin = false,
            DisplayName = "New User"
        };

        var result = await _service!.CreateUserAsync(request, ct);
        OutputSuccessCheck(result, "success", "CreateUserAsync", HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateUserAsync_ValidAdminRequest_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new CreateUserRequest
        {
            Email = "new@testemail.com",
            Admin = true,
            DisplayName = "New User"
        };

        var result = await _service!.CreateUserAsync(request, ct);
        OutputSuccessCheck(result, "success", "CreateUserAsync", HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new CreateUserRequest
        {
            Email = "new@testemail.com",
            Admin = false,
            DisplayName = "New User"
        };

        var result = await _service!.CreateUserAsync(request, ct);
        OutputFailureCheck(result, "unauthorized", "CreateUserAsync", HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("admin@example.com")]
    [InlineData("USER3@EXAMPLE.COM")]
    [InlineData("User5@example.com")]
    public async Task CreateUserAsync_WhenEmailAlreadyExists_ReturnsConflict(string emailInput)
    {
        var request = new CreateUserRequest
        {
            Email = emailInput,
            Admin = false,
            DisplayName = "New User"
        };

        var result = await _service!.CreateUserAsync(request, ct);
        OutputFailureCheck(result, "already exists", "CreateUserAsync", HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Admin User")]
    [InlineData("charles")]
    [InlineData("ELLEN")]
    public async Task CreateUserAsync_WhenDisplayNameAlreadyExists_ReturnsConflict(string nameInput)
    {
        var request = new CreateUserRequest
        {
            Email = "new@testemail.com",
            Admin = false,
            DisplayName = nameInput
        };

        var result = await _service!.CreateUserAsync(request, ct);
        OutputFailureCheck(result, "already taken", "CreateUserAsync", HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUserAsync_WhenNonAdminCreatesAdmin_ReturnsUnauthorized()
    {
        var request = new CreateUserRequest
        {
            Email = "new@testemail.com",
            Admin = true,
            DisplayName = "New User"
        };

        var result = await _service!.CreateUserAsync(request, ct);
        OutputFailureCheck(result, "admin", "CreateUserAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    private void OutputFailureCheck(CommonResponse result, string errorText, string methodName, HttpStatusCode code)
    {
        result.StatusCode.Should().Be(code);
        result.Message?.ToLowerInvariant().Should().Contain(errorText.ToLowerInvariant());

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains(methodName));
    }

    private void OutputSuccessCheck(CommonResponse result, string successText, string methodName, HttpStatusCode code)
    {
        result.StatusCode.Should().Be(code);
        result.Message?.ToLowerInvariant().Should().Contain(successText.ToLowerInvariant());

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(methodName));
    }

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
