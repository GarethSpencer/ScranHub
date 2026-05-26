using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Groups;
using WebApi.Controllers.v1;
using WebApi.UnitTests.Helpers;

namespace WebApi.UnitTests.Controllers.v1;

public class GroupControllerTests
{
    private readonly Mock<IGroupService> _groupServiceMock = new();
    private readonly Mock<IValidator<CreateGroupRequest>> _createGroupRequestValidatorMock = new();
    private readonly Mock<IValidator<UpdateGroupRequest>> _updateGroupRequestValidatorMock = new();
    private readonly Mock<IValidator<SearchGroupRequest>> _searchGroupRequestValidatorMock = new();
    private readonly GroupController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public GroupControllerTests()
    {
        _sut = new GroupController(
            _groupServiceMock.Object,
            _createGroupRequestValidatorMock.Object,
            _updateGroupRequestValidatorMock.Object,
            _searchGroupRequestValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetGroupResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetGroupResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetGroupResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetGroup(groupId, ct);

        _groupServiceMock.Verify(s => s.GetGroupAsync(groupId, ct), Times.Once);
    }

    [Fact]
    public async Task SearchGroups_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.SearchGroups(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task SearchGroups_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_searchGroupRequestValidatorMock);
        var request = new SearchGroupRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };

        var result = await _sut.SearchGroups(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupServiceMock.Verify(s => s.SearchGroupsAsync(It.IsAny<SearchGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchGroups_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_searchGroupRequestValidatorMock);
        var request = new SearchGroupRequest { SearchText = "Test", PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetGroupsResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.SearchGroupsAsync(It.IsAny<SearchGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.SearchGroups(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetGroupsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task SearchGroups_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_searchGroupRequestValidatorMock);
        var request = new SearchGroupRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };
        var expectedResponse = new GetGroupsResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.SearchGroupsAsync(It.IsAny<SearchGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.SearchGroups(request, ct);

        _groupServiceMock.Verify(s => s.SearchGroupsAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task CreateGroup_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.CreateGroup(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task CreateGroup_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_createGroupRequestValidatorMock);
        var request = new CreateGroupRequest { GroupName = "Test Group" };

        var result = await _sut.CreateGroup(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupServiceMock.Verify(s => s.CreateGroupAsync(It.IsAny<CreateGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateGroup_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_createGroupRequestValidatorMock);
        var request = new CreateGroupRequest { GroupName = "Test Group" };
        var expectedResponse = new AddGroupResponse { StatusCode = HttpStatusCode.Created };
        _groupServiceMock.Setup(s => s.CreateGroupAsync(It.IsAny<CreateGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.CreateGroup(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<AddGroupResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task CreateGroup_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_createGroupRequestValidatorMock);
        var request = new CreateGroupRequest { GroupName = "Test Group" };
        var expectedResponse = new AddGroupResponse { StatusCode = HttpStatusCode.Created };
        _groupServiceMock.Setup(s => s.CreateGroupAsync(It.IsAny<CreateGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.CreateGroup(request, ct);

        _groupServiceMock.Verify(s => s.CreateGroupAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task UpdateGroup_NullRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();

        var result = await _sut.UpdateGroup(groupId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateGroup_InvalidRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_updateGroupRequestValidatorMock);
        var request = new UpdateGroupRequest { GroupName = "Updated Group", Active = true };

        var result = await _sut.UpdateGroup(groupId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupServiceMock.Verify(s => s.UpdateGroupAsync(It.IsAny<Guid>(), It.IsAny<UpdateGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateGroupRequestValidatorMock);
        var request = new UpdateGroupRequest { GroupName = "Updated Group", Active = true };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.UpdateGroupAsync(It.IsAny<Guid>(), It.IsAny<UpdateGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.UpdateGroup(groupId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateGroupRequestValidatorMock);
        var request = new UpdateGroupRequest { GroupName = "Updated Group", Active = true };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.UpdateGroupAsync(It.IsAny<Guid>(), It.IsAny<UpdateGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.UpdateGroup(groupId, request, ct);

        _groupServiceMock.Verify(s => s.UpdateGroupAsync(groupId, request, ct), Times.Once);
    }

    [Fact]
    public async Task DeleteGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.DeleteGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.DeleteGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task DeleteGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.DeleteGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.DeleteGroup(groupId, ct);

        _groupServiceMock.Verify(s => s.DeleteGroupAsync(groupId, ct), Times.Once);
    }

    [Fact]
    public async Task GetUserGroups_ValidRequest_ReturnsCorrectResult()
    {
        var expectedResponse = new UserGroupsResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetGroupsForUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetUserGroups(ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<UserGroupsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetUserGroups_ValidRequest_CallsServiceCorrectly()
    {
        var expectedResponse = new UserGroupsResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetGroupsForUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetUserGroups(ct);

        _groupServiceMock.Verify(s => s.GetGroupsForUserAsync(ct), Times.Once);
    }

    [Fact]
    public async Task JoinGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.JoinGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.JoinGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task JoinGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.JoinGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.JoinGroup(groupId, ct);

        _groupServiceMock.Verify(s => s.JoinGroupAsync(groupId, ct), Times.Once);
    }

    [Fact]
    public async Task LeaveGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.LeaveGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.LeaveGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task LeaveGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.LeaveGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.LeaveGroup(groupId, ct);

        _groupServiceMock.Verify(s => s.LeaveGroupAsync(groupId, ct), Times.Once);
    }
}