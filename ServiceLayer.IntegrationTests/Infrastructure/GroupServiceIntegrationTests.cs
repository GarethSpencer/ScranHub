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
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Responses.Users;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class GroupServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<GroupService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private OutputChecks<GroupService> _checks = new(new FakeLogger<GroupService>());
    private GroupService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<GroupService>();
        _checks = new OutputChecks<GroupService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

        _service = new GroupService(
            tokenData: _tokenData.Object,
            logger: _logger,
            userRepository: new UserRepository(_context),
            groupRepository: new GroupRepository(_context),
            userGroupRepository: new UserGroupRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }

    #region CreateGroupAsync
    [Fact]
    public async Task CreateGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new CreateGroupRequest
        {
            GroupName = "New Test Group"
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "CreateGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateGroupAsync_NameAlreadyTaken_ReturnsConflict()
    {
        var request = new CreateGroupRequest
        {
            GroupName = TestGroup2Name
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputFailureCheck(result, "already exists", "CreateGroupAsync", HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateGroupAsync_MemberNotFriendOfCreator_ReturnsForbidden()
    {
        var request = new CreateGroupRequest
        {
            GroupName = "New Test Group",
            InitialMemberIds = [TestUser3AdminId]
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputFailureCheck(result, "not friends", "CreateGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateGroupAsync_ValidNewName_ReturnsCreated()
    {
        var request = new CreateGroupRequest
        {
            GroupName = "New Test Group"
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "CreateGroupAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddGroupResponse>().Subject;
        _logger.Entries.Should().ContainSingle(e => e.Message.Contains(request.GroupName, StringComparison.InvariantCultureIgnoreCase));
        _context!.UserGroups.Should().ContainSingle(e => e.UserId == TestUser2NonAdminId && e.GroupId == typedResult.GroupId);
    }

    [Fact]
    public async Task CreateGroupAsync_ValidMember_ReturnsCreated()
    {
        var request = new CreateGroupRequest
        {
            GroupName = "New Test Group",
            InitialMemberIds = [TestUser1AdminId]
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "CreateGroupAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddGroupResponse>().Subject;
        _context!.UserGroups.Should().Contain(e => e.UserId == TestUser2NonAdminId && e.GroupId == typedResult.GroupId);
        _context!.UserGroups.Should().Contain(e => e.UserId == TestUser1AdminId && e.GroupId == typedResult.GroupId);
    }
    #endregion

    #region GetGroupAsync
    [Fact]
    public async Task GetGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.GetGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupAsync_NotAdminOrMember_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.GetGroupAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "only admins or group members", "GetGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGroupAsync_AdminNotMember_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetGroupAsync(TestGroup2Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupResponse>().Subject;
        typedResult.Group!.GroupId.Should().Be(TestGroup2Id);
    }

    [Fact]
    public async Task GetGroupAsync_MemberNotAdmin_ReturnsOk()
    {
        var result = await _service!.GetGroupAsync(TestGroup1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupResponse>().Subject;
        typedResult.Group!.GroupId.Should().Be(TestGroup1Id);
    }
    #endregion

    #region SearchGroupsAsync
    [Fact]
    public async Task SearchGroupsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SearchGroupsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchGroupsAsync_ValidNonAdminSearch_ReturnsOk()
    {
        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsResponse>().Subject;
        typedResult.TotalCount.Should().Be(2); //Group 2 is inactive
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup3Id);
    }

    [Fact]
    public async Task SearchGroupsAsync_ValidAdminSearch_ReturnsOk()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsResponse>().Subject;
        typedResult.TotalCount.Should().Be(1);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup1Id);
    }

    [Fact]
    public async Task SearchGroupsAsync_ValidFriendlessSearch_ReturnsNoContent()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupsAsync", HttpStatusCode.NoContent);

        var typedResult = result.Should().BeOfType<GetGroupsResponse>().Subject;
        typedResult.TotalCount.Should().Be(0);
        typedResult.Groups.Should().BeEmpty();
    }
    #endregion

    #region UpdateGroupAsync
    [Fact]
    public async Task UpdateGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup1Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatingSomeoneElsesGroup_ReturnsForbidden()
    {
        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup1Id, request, ct);
        _checks.OutputFailureCheck(result, "not create", "UpdateGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGroupAsync_AdminUpdatingSomeoneElsesGroup_ReturnsForbidden()
    {
        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputFailureCheck(result, "not create", "UpdateGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatingGroupToExistingName_ReturnsConflict()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateGroupRequest
        {
            GroupName = TestGroup1Name.ToUpperInvariant(),
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputFailureCheck(result, "already exists", "UpdateGroupAsync", HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Updated Test Group")]
    [InlineData("Test Group 3")]
    [InlineData("test group 3")]
    [InlineData("TEST GROUP 3")]
    public async Task UpdateGroupAsync_ValidDetailsSameNameDifferentCase_ReturnsOK(string newGroupName)
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateGroupRequest
        {
            GroupName = newGroupName,
            Active = false
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateGroupAsync", HttpStatusCode.OK);
        _context!.Groups.Should().ContainSingle(x => x.GroupId == TestGroup3Id && x.GroupName == newGroupName);
    }
    #endregion

    #region GetGroupsForUserAsync
    [Fact]
    public async Task GetGroupsForUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupsForUserAsync(ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupsForUserAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupsForUserAsync_ValidNonAdminUser_ReturnsOK()
    {
        var result = await _service!.GetGroupsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserGroupsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser2NonAdminId);
        typedResult.UserGroups.Should().HaveCount(2);
        typedResult.UserGroups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.UserGroups.Should().Contain(e => e.GroupId == TestGroup2Id);
    }

    [Fact]
    public async Task GetGroupsForUserAsync_ValidAdminUser_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetGroupsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserGroupsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser3AdminId);
        typedResult.UserGroups.Should().HaveCount(1);
        typedResult.UserGroups.Should().Contain(e => e.GroupId == TestGroup1Id);
    }
    #endregion

    #region LeaveGroupAsync
    [Fact]
    public async Task LeaveGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.LeaveGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "LeaveGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LeaveGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.LeaveGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "LeaveGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LeaveGroupAsync_NotInGroup_ReturnsBadRequest()
    {
        var result = await _service!.LeaveGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "not a member", "LeaveGroupAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LeaveGroupAsync_CreatedGroup_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.LeaveGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "created", "LeaveGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LeaveGroupAsync_ValidRequest_ReturnsOK()
    {
        var result = await _service!.LeaveGroupAsync(TestGroup2Id, ct);
        _checks.OutputSuccessCheck(result, "success", "LeaveGroupAsync", HttpStatusCode.OK);
        _context!.UserGroups.Should().NotContain(e => e.UserId == TestUser2NonAdminId && e.GroupId == TestGroup2Id);
    }
    #endregion

    #region JoinGroupAsync
    [Fact]
    public async Task JoinGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.JoinGroupAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "JoinGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task JoinGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.JoinGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "JoinGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task JoinGroupAsync_AlreadyInGroup_ReturnsBadRequest()
    {
        var result = await _service!.JoinGroupAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "already", "JoinGroupAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JoinGroupAsync_NotFriendsWithGroupMember_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.JoinGroupAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "not friends", "JoinGroupAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JoinGroupAsync_NotAcceptedFriendsWithGroupMember_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.JoinGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "not friends", "JoinGroupAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JoinGroupAsync_ValidRequest_ReturnsCreated()
    {
        var result = await _service!.JoinGroupAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "JoinGroupAsync", HttpStatusCode.Created);

        _context!.UserGroups.Should().ContainSingle(e => e.UserId == TestUser2NonAdminId && e.GroupId == TestGroup3Id);
    }
    #endregion

    #region DeleteGroupAsync
    [Fact]
    public async Task DeleteGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.DeleteGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGroupAsync_NotAdminOrCreator_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

        var result = await _service!.DeleteGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "permission", "DeleteGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteGroupAsync_ValidRequest_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.DeleteGroupAsync(TestGroup1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteGroupAsync", HttpStatusCode.OK);
        _context!.Groups.Should().NotContain(x => x.GroupId == TestGroup1Id);
    }
    #endregion

    #region GetAllGroupsAsync
    [Fact]
    public async Task GetAllGroupsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.GetAllGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetAllGroupsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllGroupsAsync_NotAdmin_ReturnsForbidden()
    {
        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.GetAllGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "admin", "GetAllGroupsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllGroupsAsync_Admin_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 2
        };

        var result = await _service!.GetAllGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "GetAllGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsDetailedResponse>().Subject;
        typedResult.TotalCount.Should().Be(3);
        typedResult.Groups!.Count().Should().Be(2); //ordered by name
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup2Id);
    }
    #endregion

    #region SearchAllGroupsAsync
    [Fact]
    public async Task SearchAllGroupsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchAllGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SearchAllGroupsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchAllGroupsAsync_NonAdminSearch_ReturnsForbidden()
    {
        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchAllGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "admin", "SearchAllGroupsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchAllGroupsAsync_ValidAdminSearch_ReturnsOk()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchAllGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchAllGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsDetailedResponse>().Subject;
        typedResult.TotalCount.Should().Be(3);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup2Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup3Id);
    }
    #endregion

    #region GetGroupMembersAsync
    [Fact]
    public async Task GetGroupMembersAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };

        var result = await _service!.GetGroupMembersAsync(TestGroup1Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupMembersAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupMembersAsync_InvalidGroup_ReturnsNotFound()
    {
        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };

        var result = await _service!.GetGroupMembersAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupMembersAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupMembersAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };

        var result = await _service!.GetGroupMembersAsync(TestGroup3Id, request, ct);
        _checks.OutputFailureCheck(result, "only group members", "GetGroupMembersAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGroupMembersAsync_UserInGroup_ReturnsOK()
    {
        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };

        var result = await _service!.GetGroupMembersAsync(TestGroup1Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupMembersAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetUsersResponse>().Subject;
        typedResult.TotalCount.Should().Be(5);
        typedResult.Users.Should().Contain(e => e.UserId == TestUser1AdminId);
        typedResult.Users.Should().Contain(e => e.UserId == TestUser2NonAdminId);
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
