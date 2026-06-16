using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;
using WebApi.Controllers.v1;
using WebApi.UnitTests.Helpers;

namespace WebApi.UnitTests.Controllers.v1;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IValidator<CreateUserRequest>> _createUserRequestValidatorMock = new();
    private readonly Mock<IValidator<UpdateUserRequest>> _updateUserRequestValidatorMock = new();
    private readonly Mock<IValidator<UpdateUserFriendRequest>> _updateUserFriendRequestValidatorMock = new();
    private readonly Mock<IValidator<SearchUserRequest>> _searchUserRequestValidatorMock = new();
    private readonly Mock<IValidator<AddFriendRequest>> _addFriendRequestValidatorMock = new();
    private readonly Mock<IValidator<GetUserFriendRequest>> _getUserFriendRequestValidatorMock = new();
    private readonly UserController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public UserControllerTests()
    {
        _sut = new UserController(
            _userServiceMock.Object,
            _createUserRequestValidatorMock.Object,
            _updateUserRequestValidatorMock.Object,
            _updateUserFriendRequestValidatorMock.Object,
            _searchUserRequestValidatorMock.Object,
            _addFriendRequestValidatorMock.Object,
            _getUserFriendRequestValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetUser_ValidRequest_ReturnsCorrectResult()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new GetUserResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetUser(userId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetUserResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetUser_ValidRequest_CallsServiceCorrectly()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new GetUserResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetUser(userId, ct);

        _userServiceMock.Verify(s => s.GetUserAsync(userId, ct), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_ValidRequest_ReturnsCorrectResult()
    {
        var expectedResponse = new GetUserDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetCurrentUser(ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetUserDetailedResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetCurrentUser_ValidRequest_CallsServiceCorrectly()
    {
        var expectedResponse = new GetUserDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetCurrentUser(ct);

        _userServiceMock.Verify(s => s.GetCurrentUserAsync(ct), Times.Once);
    }

    [Fact]
    public async Task SearchUsers_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.SearchUsers(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task SearchUsers_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_searchUserRequestValidatorMock);
        var request = new SearchUserRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };

        var result = await _sut.SearchUsers(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.SearchUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchUsers_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_searchUserRequestValidatorMock);
        var request = new SearchUserRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };
        var expectedResponse = new GetUsersResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.SearchUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.SearchUsers(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetUsersResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task SearchUsers_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_searchUserRequestValidatorMock);
        var request = new SearchUserRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };
        var expectedResponse = new GetUsersResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.SearchUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.SearchUsers(request, ct);

        _userServiceMock.Verify(s => s.SearchUsersAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task CreateUser_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.CreateUser(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_createUserRequestValidatorMock);
        var request = new CreateUserRequest { Admin = false, DisplayName = "Test User", Email = "test@example.com" };

        var result = await _sut.CreateUser(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_createUserRequestValidatorMock);
        var request = new CreateUserRequest { Admin = false, DisplayName = "Test User", Email = "test@example.com" };
        var expectedResponse = new AddUserResponse { StatusCode = HttpStatusCode.Created };
        _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.CreateUser(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<AddUserResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_createUserRequestValidatorMock);
        var request = new CreateUserRequest { Admin = false, DisplayName = "Test User", Email = "test@example.com" };
        var expectedResponse = new AddUserResponse { StatusCode = HttpStatusCode.Created };
        _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.CreateUser(request, ct);

        _userServiceMock.Verify(s => s.CreateUserAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_NullRequest_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.UpdateUser(userId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateUser_InvalidRequest_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_updateUserRequestValidatorMock);
        var request = new UpdateUserRequest { Admin = false, DisplayName = "Test User", Active = true };

        var result = await _sut.UpdateUser(userId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ValidRequest_ReturnsCorrectResult()
    {
        var userId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateUserRequestValidatorMock);
        var request = new UpdateUserRequest { Admin = false, DisplayName = "Test User", Active = true };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.UpdateUser(userId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateUser_ValidRequest_CallsServiceCorrectly()
    {
        var userId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateUserRequestValidatorMock);
        var request = new UpdateUserRequest { Admin = false, DisplayName = "Test User", Active = true };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.UpdateUser(userId, request, ct);

        _userServiceMock.Verify(s => s.UpdateUserAsync(userId, request, ct), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ValidRequest_ReturnsCorrectResult()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.DeleteUser(userId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task DeleteUser_ValidRequest_CallsServiceCorrectly()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.DeleteUser(userId, ct);

        _userServiceMock.Verify(s => s.DeleteUserAsync(userId, ct), Times.Once);
    }

    [Fact]
    public async Task GetFriends_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.GetFriends(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task GetFriends_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_getUserFriendRequestValidatorMock);
        var request = new GetUserFriendRequest { PageNumber = 0, PageSize = 10, Status = FriendshipStatus.Accepted };

        var result = await _sut.GetFriends(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.GetFriendsForUserAsync(It.IsAny<GetUserFriendRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetFriends_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_getUserFriendRequestValidatorMock);
        var request = new GetUserFriendRequest { PageNumber = 0, PageSize = 10, Status = FriendshipStatus.Accepted };

        var expectedResponse = new UserFriendsResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetFriendsForUserAsync(It.IsAny<GetUserFriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetFriends(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<UserFriendsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetFriends_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_getUserFriendRequestValidatorMock);
        var request = new GetUserFriendRequest { PageNumber = 0, PageSize = 10, Status = FriendshipStatus.Accepted };

        var expectedResponse = new UserFriendsResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetFriendsForUserAsync(It.IsAny<GetUserFriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetFriends(request, ct);

        _userServiceMock.Verify(s => s.GetFriendsForUserAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task AddFriend_ValidRequest_ReturnsCorrectResult()
    {
        var friendId = Guid.NewGuid();
        var expectedResponse = new AddUserFriendResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.AddUserFriendAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.AddFriend(friendId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<AddUserFriendResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task AddFriend_ValidRequest_CallsServiceCorrectly()
    {
        var friendId = Guid.NewGuid();
        var expectedResponse = new AddUserFriendResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.AddUserFriendAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.AddFriend(friendId, ct);

        _userServiceMock.Verify(s => s.AddUserFriendAsync(friendId, ct), Times.Once);
    }

    [Fact]
    public async Task AddFriendByEmail_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.AddFriendByEmail(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task AddFriendByEmail_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_addFriendRequestValidatorMock);
        var request = new AddFriendRequest { Email = "test@example.com" };

        var result = await _sut.AddFriendByEmail(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.AddUserFriendByEmailAsync(It.IsAny<AddFriendRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddFriendByEmail_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_addFriendRequestValidatorMock);
        var request = new AddFriendRequest { Email = "test@example.com" };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.AddUserFriendByEmailAsync(It.IsAny<AddFriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.AddFriendByEmail(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task AddFriendByEmail_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_addFriendRequestValidatorMock);
        var request = new AddFriendRequest { Email = "test@example.com" };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.AddUserFriendByEmailAsync(It.IsAny<AddFriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.AddFriendByEmail(request, ct);

        _userServiceMock.Verify(s => s.AddUserFriendByEmailAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task UpdateFriend_NullRequest_ReturnsBadRequest()
    {
        var friendId = Guid.NewGuid();

        var result = await _sut.UpdateFriend(friendId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateFriend_InvalidRequest_ReturnsBadRequest()
    {
        var friendId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_updateUserFriendRequestValidatorMock);
        var request = new UpdateUserFriendRequest { Status = FriendshipStatus.Accepted };

        var result = await _sut.UpdateFriend(friendId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.UpdateUserFriendAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserFriendRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFriend_ValidRequest_ReturnsCorrectResult()
    {
        var friendId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateUserFriendRequestValidatorMock);
        var request = new UpdateUserFriendRequest { Status = FriendshipStatus.Accepted };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.UpdateUserFriendAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserFriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.UpdateFriend(friendId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateFriend_ValidRequest_CallsServiceCorrectly()
    {
        var friendId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateUserFriendRequestValidatorMock);
        var request = new UpdateUserFriendRequest { Status = FriendshipStatus.Accepted };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.UpdateUserFriendAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserFriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.UpdateFriend(friendId, request, ct);

        _userServiceMock.Verify(s => s.UpdateUserFriendAsync(friendId, request, ct), Times.Once);
    }

    [Fact]
    public async Task DeleteUserFriend_ValidRequest_ReturnsCorrectResult()
    {
        var userFriendId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.DeleteUserFriendAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.DeleteFriend(userFriendId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task DeleteUserFriend_ValidRequest_CallsServiceCorrectly()
    {
        var userFriendId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.DeleteUserFriendAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.DeleteFriend(userFriendId, ct);

        _userServiceMock.Verify(s => s.DeleteUserFriendAsync(userFriendId, ct), Times.Once);
    }
}