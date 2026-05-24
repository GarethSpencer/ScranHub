using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Responses.Users;
using WebApi.Controllers.v1;

namespace WebApi.UnitTests.Controllers.v1;

public class AdminControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IGroupService> _groupServiceMock = new();
    private readonly Mock<IValidator<PaginationBaseRequest>> _validatorMock = new();
    private readonly AdminController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public AdminControllerTests()
    {
        _sut = new AdminController(
            _userServiceMock.Object,
            _groupServiceMock.Object,
            _validatorMock.Object
        );
    }

    private void SetupValidatorPass()
    {
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupValidatorFail(string propertyName = "PageNumber", string errorMessage = "Must be greater than 0")
    {
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure(propertyName, errorMessage)
            ]));
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
        SetupValidatorFail();
        var request = new PaginationBaseRequest { PageNumber = 0, PageSize = 10 };

        var result = await _sut.GetAllUsers(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _userServiceMock.Verify(s => s.GetAllUsersAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllUsers_ValidRequest_ReturnsCorrectResult()
    {
        SetupValidatorPass();
        var request = new PaginationBaseRequest { PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetUsersDetailedResponse { StatusCode = HttpStatusCode.OK };
        _userServiceMock.Setup(s => s.GetAllUsersAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetAllUsers(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
        _userServiceMock.Verify(s => s.GetAllUsersAsync(request, It.IsAny<CancellationToken>()), Times.Once);
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
        SetupValidatorFail();
        var request = new PaginationBaseRequest { PageNumber = 0, PageSize = 10 };

        var result = await _sut.GetAllGroups(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupServiceMock.Verify(s => s.GetAllGroupsAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllGroups_ValidRequest_ReturnsCorrectResult()
    {
        SetupValidatorPass();
        var request = new PaginationBaseRequest { PageNumber = 1, PageSize = 10 };
        var expectedResponse = new GetGroupsDetailedResponse { StatusCode = HttpStatusCode.OK };
        _groupServiceMock.Setup(s => s.GetAllGroupsAsync(It.IsAny<PaginationBaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetAllGroups(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
        _groupServiceMock.Verify(s => s.GetAllGroupsAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}