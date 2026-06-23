using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Responses.Users;
using WebApi.Controllers.v1;
using WebApi.UnitTests.Helpers;

namespace WebApi.UnitTests.Controllers.v1;

public class AdminControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IGroupService> _groupServiceMock = new();
    private readonly Mock<IValidator<PaginationBaseRequest>> _paginationValidatorMock = new();
    private readonly Mock<IValidator<SearchUserRequest>> _searchUserValidatorMock = new();
    private readonly Mock<IValidator<SearchGroupRequest>> _searchGroupValidatorMock = new();
    private readonly AdminController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public AdminControllerTests()
    {
        _sut = new AdminController(
            _userServiceMock.Object,
            _groupServiceMock.Object,
            _paginationValidatorMock.Object,
            _searchUserValidatorMock.Object,
            _searchGroupValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetAllUsers_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.GetAllUsers(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task GetAllUsers_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_paginationValidatorMock);
        var request = new PaginationBaseRequest { PageNumber = 0, PageSize = 10 };

        var result = await _sut.GetAllUsers(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.GetAllUsersAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllUsers_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_paginationValidatorMock);
        var request = new PaginationBaseRequest { PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetUsersDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetAllUsers(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetAllUsers_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_paginationValidatorMock);
        var request = new PaginationBaseRequest { PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetUsersDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetAllUsers(request, ct);

        _userServiceMock.Verify(s => s.GetAllUsersAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAllUsers_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.SearchAllUsers(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task SearchAllUsers_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_searchUserValidatorMock);
        var request = new SearchUserRequest { PageNumber = 0, PageSize = 10, SearchText = "Test" };

        var result = await _sut.SearchAllUsers(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.SearchAllUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAllUsers_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_searchUserValidatorMock);
        var request = new SearchUserRequest { PageNumber = 0, PageSize = 10, SearchText = "Test" };
        var expectedResponse = new GetUsersDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.SearchAllUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.SearchAllUsers(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task SearchAllUsers_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_searchUserValidatorMock);
        var request = new SearchUserRequest { PageNumber = 0, PageSize = 10, SearchText = "Test" };
        var expectedResponse = new GetUsersDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.SearchAllUsersAsync(It.IsAny<SearchUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.SearchAllUsers(request, ct);

        _userServiceMock.Verify(s => s.SearchAllUsersAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllGroups_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.GetAllGroups(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task GetAllGroups_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_paginationValidatorMock);
        var request = new PaginationBaseRequest { PageNumber = 0, PageSize = 10 };

        var result = await _sut.GetAllGroups(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupServiceMock.Verify(s => s.GetAllGroupsAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllGroups_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_paginationValidatorMock);
        var request = new PaginationBaseRequest { PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetGroupsDetailedResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetAllGroupsAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetAllGroups(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetAllGroups_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_paginationValidatorMock);
        var request = new PaginationBaseRequest { PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetGroupsDetailedResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetAllGroupsAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetAllGroups(request, ct);

        _groupServiceMock.Verify(s => s.GetAllGroupsAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAllGroups_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.SearchAllGroups(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task SearchAllGroups_InvalidRequest_ReturnsBadRequest()
    {
        SetupHelpers.SetupValidatorFail(_searchGroupValidatorMock);
        var request = new SearchGroupRequest { PageNumber = 0, PageSize = 10, SearchText = "Test" };

        var result = await _sut.SearchAllGroups(null!, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupServiceMock.Verify(s => s.SearchAllGroupsAsync(It.IsAny<SearchGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAllGroups_ValidRequest_ReturnsCorrectResult()
    {
        SetupHelpers.SetupValidatorPass(_searchGroupValidatorMock);
        var request = new SearchGroupRequest { PageNumber = 0, PageSize = 10, SearchText = "Test" };
        var expectedResponse = new GetGroupsDetailedResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.SearchAllGroupsAsync(It.IsAny<SearchGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.SearchAllGroups(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task SearchAllGroups_ValidRequest_CallsServiceCorrectly()
    {
        SetupHelpers.SetupValidatorPass(_searchGroupValidatorMock);
        var request = new SearchGroupRequest { PageNumber = 0, PageSize = 10, SearchText = "Test" };
        var expectedResponse = new GetGroupsDetailedResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.SearchAllGroupsAsync(It.IsAny<SearchGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.SearchAllGroups(request, ct);

        _groupServiceMock.Verify(s => s.SearchAllGroupsAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}