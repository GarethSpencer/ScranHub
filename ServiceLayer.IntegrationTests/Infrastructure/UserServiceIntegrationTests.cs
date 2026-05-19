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
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
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
    #endregion

    #region CreateUserAsync
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
        result.Should().BeOfType<AddUserResponse>();
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
        result.Should().BeOfType<AddUserResponse>();
    }
    #endregion

    #region SearchUsersAsync
    [Fact]
    public async Task SearchUsersAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SearchUserRequest
        {
            SearchText = "user",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.SearchUsersAsync(request, ct);
        OutputFailureCheck(result, "unauthorized", "SearchUsersAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchUsersAsync_ValidSearchRequestForTwoNames_ReturnsOk()
    {
        var request = new SearchUserRequest
        {
            SearchText = "user",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.SearchUsersAsync(request, ct);
        OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersResponse>().Subject;
        typedResult.TotalCount.Should().Be(2);
        typedResult.Users.Should().HaveCount(2);
        typedResult.Users.Should().Contain(e => e.DisplayName.Equals("Admin User", StringComparison.InvariantCultureIgnoreCase));
        typedResult.Users.Should().Contain(e => e.DisplayName.Equals("Non-Admin User", StringComparison.InvariantCultureIgnoreCase));

    }

    [Fact]
    public async Task SearchUsersAsync_ValidSearchRequestForOneName_ReturnsOk()
    {
        var request = new SearchUserRequest
        {
            SearchText = "char",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.SearchUsersAsync(request, ct);
        OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersResponse>().Subject;
        typedResult.TotalCount.Should().Be(1);
        typedResult.Users.Should().HaveCount(1);
        typedResult.Users.Should().Contain(e => e.DisplayName.Equals("Charles", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task SearchUsersAsync_ValidSearchRequestForPageSize1OfTwoNames_ReturnsOk()
    {
        var request = new SearchUserRequest
        {
            SearchText = "user",
            PageNumber = 1,
            PageSize = 1
        };

        var result = await _service!.SearchUsersAsync(request, ct);
        OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersResponse>().Subject;
        typedResult.TotalCount.Should().Be(2);
        typedResult.Users.Should().HaveCount(1);
        typedResult.Users.Should().Contain(e => e.DisplayName.Equals("Admin User", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task SearchUsersAsync_ValidSearchRequestForPage2OfOneName_ReturnsOk()
    {
        var request = new SearchUserRequest
        {
            SearchText = "char",
            PageNumber = 2,
            PageSize = 1
        };

        var result = await _service!.SearchUsersAsync(request, ct);
        OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersResponse>().Subject;
        typedResult.TotalCount.Should().Be(1);
        typedResult.Users.Should().HaveCount(0);
    }
    #endregion

    #region UpdateUserAsync
    [Fact]
    public async Task UpdateUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(SeedUser2NonAdminId, request, ct);
        OutputFailureCheck(result, "unauthorized", "UpdateUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserAsync_NotAdminUpdatingSomeoneElse_ReturnsUnauthorized()
    {
        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(TestUser4NonAdminId, request, ct);
        OutputFailureCheck(result, "update this user", "UpdateUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserAsync_NotAdminUpdatingSelfToAdmin_ReturnsUnauthorized()
    {
        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = true,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(SeedUser2NonAdminId, request, ct);
        OutputFailureCheck(result, "make yourself an admin", "UpdateUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatingInvalidUser_ReturnsNotFound()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(Guid.Empty, request, ct);
        OutputFailureCheck(result, "not found", "UpdateUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserAsync_AdminUpdatingAnotherAdmin_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(TestUser3AdminId, request, ct);
        OutputFailureCheck(result, "update this user", "UpdateUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserAsync_NonAdminUpdatingSelf_ReturnsOK()
    {
        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(SeedUser2NonAdminId, request, ct);
        OutputSuccessCheck(result, "success", "UpdateUserAsync", HttpStatusCode.OK);
        _context!.Users.Should().ContainSingle(e => e.UserId == SeedUser2NonAdminId && e.DisplayName == "New Test User" && e.Admin == false && e.Active == true);
    }

    [Fact]
    public async Task UpdateUserAsync_AdminUpdatingNonAdmin_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = true,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(SeedUser2NonAdminId, request, ct);
        OutputSuccessCheck(result, "success", "UpdateUserAsync", HttpStatusCode.OK);
        _context!.Users.Should().ContainSingle(e => e.UserId == SeedUser2NonAdminId && e.DisplayName == "New Test User" && e.Admin == true && e.Active == true);
    }
    #endregion

    #region GetUserAsync
    [Fact]
    public async Task GetUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetUserAsync(SeedUser2NonAdminId, ct);
        OutputFailureCheck(result, "unauthorized", "GetUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAsync_NotValidUserId_ReturnsNotFound()
    {
        var result = await _service!.GetUserAsync(Guid.Empty, ct);
        OutputFailureCheck(result, "not found", "GetUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserAsync_NotAdminAndUserNotFriend_ReturnsForbidden()
    {
        var result = await _service!.GetUserAsync(TestUser3AdminId, ct);
        OutputFailureCheck(result, "admins or friends", "GetUserAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserAsync_NonAdminSearchingForFriend_ReturnsOK()
    {
        var result = await _service!.GetUserAsync(SeedUser1AdminId, ct);
        OutputSuccessCheck(result, "success", "GetUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUserResponse>().Subject;
        typedResult.User!.UserId.Should().Be(SeedUser1AdminId);
    }

    [Fact]
    public async Task GetUserAsync_AdminSearchingForNonFriend_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetUserAsync(SeedUser2NonAdminId, ct);
        OutputSuccessCheck(result, "success", "GetUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUserResponse>().Subject;
        typedResult.User!.UserId.Should().Be(SeedUser2NonAdminId);
    }
    #endregion

    #region AddUserFriendAsync
    [Fact]
    public async Task AddUserFriendAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.AddUserFriendAsync(TestUser3AdminId, ct);
        OutputFailureCheck(result, "unauthorized", "AddUserFriendAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddSelf_ReturnsBadRequest()
    {
        var result = await _service!.AddUserFriendAsync(SeedUser2NonAdminId, ct);
        OutputFailureCheck(result, "add yourself", "AddUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddInvalidaUserId_ReturnsNotFound()
    {
        var result = await _service!.AddUserFriendAsync(Guid.Empty, ct);
        OutputFailureCheck(result, "not exist", "AddUserFriendAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddExistingAcceptedUserFriend_ReturnsBadRequest()
    {
        var result = await _service!.AddUserFriendAsync(SeedUser1AdminId, ct);
        OutputFailureCheck(result, "already requested", "AddUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddExistingRejectedUserFriend_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.AddUserFriendAsync(SeedUser1AdminId, ct);
        OutputFailureCheck(result, "already requested", "AddUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddUserFriendAsync_NonAdminAddingAdmin_ReturnsCreated()
    {
        var result = await _service!.AddUserFriendAsync(TestUser3AdminId, ct);
        OutputSuccessCheck(result, "success", "AddUserFriendAsync", HttpStatusCode.Created);

        result.Should().BeOfType<AddUserFriendResponse>();
    }

    [Fact]
    public async Task AddUserFriendAsync_AdminAddingNonAdmin_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.AddUserFriendAsync(SeedUser2NonAdminId, ct);
        OutputSuccessCheck(result, "success", "AddUserFriendAsync", HttpStatusCode.Created);

        result.Should().BeOfType<AddUserFriendResponse>();
    }
    #endregion

    #region UpdateUserFriendAsync
    [Fact]
    public async Task UpdateUserFriendAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(SeedUser1AdminId, request, ct);
        OutputFailureCheck(result, "unauthorized", "UpdateUserFriendAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserFriendAsync_UpdateSelf_ReturnsBadRequest()
    {
        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(SeedUser2NonAdminId, request, ct);
        OutputFailureCheck(result, "yourself", "UpdateUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserFriendAsync_UpdateInvalidUserId_ReturnsNotFound()
    {
        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(Guid.Empty, request, ct);
        OutputFailureCheck(result, "not found", "UpdateUserFriendAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserFriendAsync_UpdateAsTheCaller_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(SeedUser2NonAdminId, request, ct);
        OutputFailureCheck(result, "requested user", "UpdateUserFriendAsync", HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(FriendshipStatus.Declined)]
    [InlineData(FriendshipStatus.Pending)]
    [InlineData(FriendshipStatus.Accepted)]
    public async Task UpdateUserFriendAsync_UpdateValidFriendRequest_ReturnsOK(FriendshipStatus status)
    {
        var request = new UpdateUserFriendRequest
        {
            Status = status
        };

        var result = await _service!.UpdateUserFriendAsync(SeedUser1AdminId, request, ct);
        OutputSuccessCheck(result, "success", "UpdateUserFriendAsync", HttpStatusCode.OK);
    }
    #endregion

    #region DeleteUserAsync
    [Fact]
    public async Task DeleteUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteUserAsync(SeedUser2NonAdminId, ct);
        OutputFailureCheck(result, "unauthorized", "DeleteUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUserAsync_InvalidUserId_ReturnsNotFound()
    {
        var result = await _service!.DeleteUserAsync(Guid.Empty, ct);
        OutputFailureCheck(result, "not found", "DeleteUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserAsync_NonAdminDeletingOtherUser_ReturnsForbidden()
    {
        var result = await _service!.DeleteUserAsync(TestUser4NonAdminId, ct);
        OutputFailureCheck(result, "not delete other users", "DeleteUserAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUserAsync_AdminDeletingOtherAdmin_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.DeleteUserAsync(TestUser3AdminId, ct);
        OutputFailureCheck(result, "not delete other admins", "DeleteUserAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUserAsync_AdminDeletingOtherUser_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.DeleteUserAsync(TestUser4NonAdminId, ct);
        OutputSuccessCheck(result, "success", "DeleteUserAsync", HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUserAsync_NonAdminDeletingSelf_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser5NonAdminId);

        var result = await _service!.DeleteUserAsync(TestUser5NonAdminId, ct);
        OutputSuccessCheck(result, "success", "DeleteUserAsync", HttpStatusCode.OK);
    }
    #endregion

    #region AddUserFriendByEmailAsync
    [Fact]
    public async Task AddUserFriendByEmailAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new AddFriendRequest
        {
            Email = SeedUser2NonAdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        OutputFailureCheck(result, "unauthorized", "AddUserFriendByEmailAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_InvalidEmail_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = "invalid@example.com"
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);

        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("User with that email does not exist.", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_OwnEmail_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = SeedUser2NonAdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);

        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("User tried to add themselves as a friend.", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_ExistingFriend_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = SeedUser1AdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);

        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("User tried to add a friend they already have.", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_NewFriend_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = TestUser3AdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);

        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("requested successfully via email.", StringComparison.InvariantCultureIgnoreCase));
    }
    #endregion

    #region GetAllUsersAsync
    [Fact]
    public async Task GetAllUsersAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.GetAllUsersAsync(request, ct);
        OutputFailureCheck(result, "unauthorized", "GetAllUsersAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsersAsync_NotAdmin_ReturnsForbidden()
    {
        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.GetAllUsersAsync(request, ct);
        OutputFailureCheck(result, "admin", "GetAllUsersAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsersAsync_Admin_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.GetAllUsersAsync(request, ct);
        OutputSuccessCheck(result, "success", "GetAllUsersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersDetailedResponse>().Subject;
        typedResult.TotalCount.Should().Be(5);
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
