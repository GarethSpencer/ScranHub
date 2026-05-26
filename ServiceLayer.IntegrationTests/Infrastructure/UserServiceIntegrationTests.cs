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
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
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
    private OutputChecks<UserService> _checks = new(new FakeLogger<UserService>());
    private UserService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<UserService>();
        _checks = new OutputChecks<UserService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

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
        _checks.OutputFailureCheck(result, "unauthorized", "GetFriendsForUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_InvalidUserId_ReturnsNotFound()
    {
        _tokenData.Setup(x => x.UserId).Returns(Guid.Empty);

        var result = await _service!.GetFriendsForUserAsync(ct);
        _checks.OutputFailureCheck(result, "not found", "GetFriendsForUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User1ValidFriends_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser1AdminId);
        typedResult.Friends!.Count().Should().Be(3);
        typedResult.FriendCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User2ValidFriends_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser2NonAdminId);
        typedResult.Friends!.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User3ValidNoFriends_ReturnsNoContent()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.NoContent);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser3AdminId);
        typedResult.Friends!.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User4ValidNoFriends_ReturnsNoContent()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.NoContent);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser4NonAdminId);
        typedResult.Friends!.Count().Should().Be(1);
        typedResult.FriendCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_User5ValidFriends_ReturnsOk()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser5NonAdminId);

        var result = await _service!.GetFriendsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetFriendsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser5NonAdminId);
        typedResult.Friends!.Count().Should().Be(1);
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
        _checks.OutputFailureCheck(result, "unauthorized", "CreateUserAsync", HttpStatusCode.Unauthorized);
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
        _checks.OutputFailureCheck(result, "already exists", "CreateUserAsync", HttpStatusCode.Conflict);
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
        _checks.OutputFailureCheck(result, "already taken", "CreateUserAsync", HttpStatusCode.Conflict);
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
        _checks.OutputFailureCheck(result, "admin", "CreateUserAsync", HttpStatusCode.Unauthorized);
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
        _checks.OutputSuccessCheck(result, "success", "CreateUserAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddUserResponse>().Subject;
        _context!.Users.Should().Contain(x => x.UserId == typedResult.UserId && x.Email == "new@testemail.com"
            && x.Admin == false && x.DisplayName == "New User");
    }

    [Fact]
    public async Task CreateUserAsync_ValidAdminRequest_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new CreateUserRequest
        {
            Email = "new@testemail.com",
            Admin = true,
            DisplayName = "New User"
        };

        var result = await _service!.CreateUserAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "CreateUserAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddUserResponse>().Subject;
        _context!.Users.Should().Contain(x => x.UserId == typedResult.UserId && x.Email == "new@testemail.com"
            && x.Admin == true && x.DisplayName == "New User");
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
        _checks.OutputFailureCheck(result, "unauthorized", "SearchUsersAsync", HttpStatusCode.Unauthorized);
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
        _checks.OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

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
        _checks.OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

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
        _checks.OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

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
        _checks.OutputSuccessCheck(result, "success", "SearchUsersAsync", HttpStatusCode.OK);

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

        var result = await _service!.UpdateUserAsync(TestUser2NonAdminId, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateUserAsync", HttpStatusCode.Unauthorized);
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
        _checks.OutputFailureCheck(result, "update this user", "UpdateUserAsync", HttpStatusCode.Unauthorized);
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

        var result = await _service!.UpdateUserAsync(TestUser2NonAdminId, request, ct);
        _checks.OutputFailureCheck(result, "make yourself an admin", "UpdateUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatingInvalidUser_ReturnsNotFound()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserAsync_AdminUpdatingAnotherAdmin_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = false,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(TestUser3AdminId, request, ct);
        _checks.OutputFailureCheck(result, "update this user", "UpdateUserAsync", HttpStatusCode.Unauthorized);
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

        var result = await _service!.UpdateUserAsync(TestUser2NonAdminId, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateUserAsync", HttpStatusCode.OK);
        _context!.Users.Should().ContainSingle(e => e.UserId == TestUser2NonAdminId && e.DisplayName == "New Test User" && e.Admin == false && e.Active == true);
    }

    [Fact]
    public async Task UpdateUserAsync_AdminUpdatingNonAdmin_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateUserRequest
        {
            DisplayName = "New Test User",
            Admin = true,
            Active = true
        };

        var result = await _service!.UpdateUserAsync(TestUser2NonAdminId, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateUserAsync", HttpStatusCode.OK);
        _context!.Users.Should().ContainSingle(e => e.UserId == TestUser2NonAdminId && e.DisplayName == "New Test User" && e.Admin == true && e.Active == true);
    }
    #endregion

    #region GetUserAsync
    [Fact]
    public async Task GetUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetUserAsync(TestUser2NonAdminId, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAsync_NotValidUserId_ReturnsNotFound()
    {
        var result = await _service!.GetUserAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserAsync_NotAdminAndUserNotFriend_ReturnsForbidden()
    {
        var result = await _service!.GetUserAsync(TestUser3AdminId, ct);
        _checks.OutputFailureCheck(result, "admins or friends", "GetUserAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserAsync_NonAdminSearchingForFriend_ReturnsOK()
    {
        var result = await _service!.GetUserAsync(TestUser1AdminId, ct);
        _checks.OutputSuccessCheck(result, "success", "GetUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUserResponse>().Subject;
        typedResult.User!.UserId.Should().Be(TestUser1AdminId);
    }

    [Fact]
    public async Task GetUserAsync_AdminSearchingForNonFriend_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetUserAsync(TestUser2NonAdminId, ct);
        _checks.OutputSuccessCheck(result, "success", "GetUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUserResponse>().Subject;
        typedResult.User!.UserId.Should().Be(TestUser2NonAdminId);
    }
    #endregion

    #region AddUserFriendAsync
    [Fact]
    public async Task AddUserFriendAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.AddUserFriendAsync(TestUser3AdminId, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "AddUserFriendAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddSelf_ReturnsBadRequest()
    {
        var result = await _service!.AddUserFriendAsync(TestUser2NonAdminId, ct);
        _checks.OutputFailureCheck(result, "add yourself", "AddUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddInvalidaUserId_ReturnsNotFound()
    {
        var result = await _service!.AddUserFriendAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not exist", "AddUserFriendAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddExistingAcceptedUserFriend_ReturnsBadRequest()
    {
        var result = await _service!.AddUserFriendAsync(TestUser1AdminId, ct);
        _checks.OutputFailureCheck(result, "already requested", "AddUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddUserFriendAsync_AddExistingRejectedUserFriend_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.AddUserFriendAsync(TestUser1AdminId, ct);
        _checks.OutputFailureCheck(result, "already requested", "AddUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddUserFriendAsync_NonAdminAddingAdmin_ReturnsCreated()
    {
        var result = await _service!.AddUserFriendAsync(TestUser3AdminId, ct);
        _checks.OutputSuccessCheck(result, "success", "AddUserFriendAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddUserFriendResponse>().Subject;
        _context!.UserFriends.Should().Contain(x => x.UserFriendId == typedResult.UserFriendId);
    }

    [Fact]
    public async Task AddUserFriendAsync_AdminAddingNonAdmin_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.AddUserFriendAsync(TestUser2NonAdminId, ct);
        _checks.OutputSuccessCheck(result, "success", "AddUserFriendAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddUserFriendResponse>().Subject;
        _context!.UserFriends.Should().Contain(x => x.UserFriendId == typedResult.UserFriendId);
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

        var result = await _service!.UpdateUserFriendAsync(TestUser1AdminId, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateUserFriendAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserFriendAsync_UpdateSelf_ReturnsBadRequest()
    {
        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(TestUser2NonAdminId, request, ct);
        _checks.OutputFailureCheck(result, "yourself", "UpdateUserFriendAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserFriendAsync_UpdateInvalidUserId_ReturnsNotFound()
    {
        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateUserFriendAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserFriendAsync_UpdateAsTheCaller_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateUserFriendRequest
        {
            Status = FriendshipStatus.Accepted
        };

        var result = await _service!.UpdateUserFriendAsync(TestUser2NonAdminId, request, ct);
        _checks.OutputFailureCheck(result, "requested user", "UpdateUserFriendAsync", HttpStatusCode.Forbidden);
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

        var result = await _service!.UpdateUserFriendAsync(TestUser1AdminId, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateUserFriendAsync", HttpStatusCode.OK);
        _context!.UserFriends.Should().Contain(x => x.UserId == TestUser1AdminId && x.FriendId == TestUser2NonAdminId && x.Status == status);
    }
    #endregion

    #region DeleteUserAsync
    [Fact]
    public async Task DeleteUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteUserAsync(TestUser2NonAdminId, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUserAsync_InvalidUserId_ReturnsNotFound()
    {
        var result = await _service!.DeleteUserAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteUserAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserAsync_NonAdminDeletingOtherUser_ReturnsForbidden()
    {
        var result = await _service!.DeleteUserAsync(TestUser4NonAdminId, ct);
        _checks.OutputFailureCheck(result, "not delete other users", "DeleteUserAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUserAsync_AdminDeletingOtherAdmin_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.DeleteUserAsync(TestUser3AdminId, ct);
        _checks.OutputFailureCheck(result, "not delete other admins", "DeleteUserAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUserAsync_AdminDeletingOtherUser_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.DeleteUserAsync(TestUser4NonAdminId, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteUserAsync", HttpStatusCode.OK);
        _context!.Users.Should().NotContain(x => x.UserId == TestUser4NonAdminId);
    }

    [Fact]
    public async Task DeleteUserAsync_NonAdminDeletingSelf_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser5NonAdminId);

        var result = await _service!.DeleteUserAsync(TestUser5NonAdminId, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteUserAsync", HttpStatusCode.OK);
        _context!.Users.Should().NotContain(x => x.UserId == TestUser5NonAdminId);
    }
    #endregion

    #region AddUserFriendByEmailAsync
    [Fact]
    public async Task AddUserFriendByEmailAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new AddFriendRequest
        {
            Email = TestUser2NonAdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "AddUserFriendByEmailAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_InvalidEmail_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = "invalid@example.com"
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        _checks.OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);

        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("User with that email does not exist.", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_OwnEmail_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = TestUser2NonAdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        _checks.OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);

        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("User tried to add themselves as a friend.", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task AddUserFriendByEmailAsync_ExistingFriend_ReturnsOkWithDetailInLog()
    {
        var request = new AddFriendRequest
        {
            Email = TestUser1AdminEmail
        };

        var result = await _service!.AddUserFriendByEmailAsync(request, ct);
        _checks.OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
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
        _checks.OutputSuccessCheck(result, "If a user with this email exists, a friend request will be sent to them.",
            "AddUserFriendByEmailAsync", HttpStatusCode.OK);
        _logger.Entries.Should().ContainSingle(e => e.Message.Contains("requested successfully via email.", StringComparison.InvariantCultureIgnoreCase));
        _context!.UserFriends.Should().ContainSingle(x => x.UserId == TestUser2NonAdminId && x.FriendId == TestUser3AdminId && x.Status == FriendshipStatus.Pending);
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
        _checks.OutputFailureCheck(result, "unauthorized", "GetAllUsersAsync", HttpStatusCode.Unauthorized);
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
        _checks.OutputFailureCheck(result, "admin", "GetAllUsersAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsersAsync_Admin_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 4
        };

        var result = await _service!.GetAllUsersAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "GetAllUsersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersDetailedResponse>().Subject;
        typedResult.TotalCount.Should().Be(5);
        typedResult.Users!.Count().Should().Be(4); //ordered by name
        typedResult.Users.Should().Contain(e => e.UserId == TestUser1AdminId);
        typedResult.Users.Should().Contain(e => e.UserId == TestUser3AdminId);
        typedResult.Users.Should().Contain(e => e.UserId == TestUser4NonAdminId);
        typedResult.Users.Should().Contain(e => e.UserId == TestUser5NonAdminId);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
