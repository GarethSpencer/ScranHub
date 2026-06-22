using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.GroupVenues;
using WebApi.Controllers.v1;
using WebApi.UnitTests.Helpers;

namespace WebApi.UnitTests.Controllers.v1;

public class GroupVenueControllerTests
{
    private readonly Mock<IGroupVenueService> _groupVenueServiceMock = new();
    private readonly Mock<IValidator<CreateGroupVenueRequest>> _createGroupVenueRequestValidatorMock = new();
    private readonly Mock<IValidator<UpdateGroupVenueRequest>> _updateGroupVenueRequestValidatorMock = new();
    private readonly Mock<IValidator<SearchGroupVenueRequest>> _searchGroupVenueRequestValidatorMock = new();
    private readonly Mock<IValidator<PaginationBaseRequest>> _paginationBaseRequestValidatorMock = new();
    private readonly GroupVenueController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public GroupVenueControllerTests()
    {
        _sut = new GroupVenueController(
            _groupVenueServiceMock.Object,
            _createGroupVenueRequestValidatorMock.Object,
            _updateGroupVenueRequestValidatorMock.Object,
            _searchGroupVenueRequestValidatorMock.Object,
            _paginationBaseRequestValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetGroupVenue_ValidRequest_ReturnsCorrectResult()
    {
        var groupVenueId = Guid.NewGuid();
        var expectedResponse = new GetGroupVenueResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.GetGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetGroupVenue(groupVenueId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetGroupVenueResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetGroupVenue_ValidRequest_CallsServiceCorrectly()
    {
        var groupVenueId = Guid.NewGuid();
        var expectedResponse = new GetGroupVenueResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.GetGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetGroupVenue(groupVenueId, ct);

        _groupVenueServiceMock.Verify(s => s.GetGroupVenueAsync(groupVenueId, ct), Times.Once);
    }

    [Fact]
    public async Task GetVenuesForGroup_NullRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();

        var result = await _sut.GetVenuesForGroup(groupId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task GetVenuesForGroup_InvalidRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_paginationBaseRequestValidatorMock);
        var request = new SortableGroupVenueRequest { PageNumber = 0, PageSize = 10, SortBy = GroupVenueSortParameters.VenueName, SortDescending = false };

        var result = await _sut.GetVenuesForGroup(groupId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupVenueServiceMock.Verify(s => s.GetAllVenuesForGroupAsync(It.IsAny<Guid>(), It.IsAny<SortableGroupVenueRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetVenuesForGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_paginationBaseRequestValidatorMock);
        var request = new SortableGroupVenueRequest { PageNumber = 0, PageSize = 10, SortBy = GroupVenueSortParameters.VenueName, SortDescending = false };
        var expectedResponse = new GetGroupVenuesResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.GetAllVenuesForGroupAsync(It.IsAny<Guid>(), It.IsAny<SortableGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetVenuesForGroup(groupId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetGroupVenuesResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetVenuesForGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_paginationBaseRequestValidatorMock);
        var request = new SortableGroupVenueRequest { PageNumber = 0, PageSize = 10, SortBy = GroupVenueSortParameters.VenueName, SortDescending = false };
        var expectedResponse = new GetGroupVenuesResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.GetAllVenuesForGroupAsync(It.IsAny<Guid>(), It.IsAny<SortableGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetVenuesForGroup(groupId, request, ct);

        _groupVenueServiceMock.Verify(s => s.GetAllVenuesForGroupAsync(groupId, request, ct), Times.Once);
    }

    [Fact]
    public async Task SearchGroupVenues_NullRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();

        var result = await _sut.SearchGroupVenues(groupId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task SearchGroupVenues_InvalidRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_searchGroupVenueRequestValidatorMock);
        var request = new SearchGroupVenueRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };

        var result = await _sut.SearchGroupVenues(groupId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupVenueServiceMock.Verify(s => s.SearchGroupVenuesAsync(It.IsAny<Guid>(), It.IsAny<SearchGroupVenueRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchGroupVenues_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_searchGroupVenueRequestValidatorMock);
        var request = new SearchGroupVenueRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };
        var expectedResponse = new GetGroupVenuesResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.SearchGroupVenuesAsync(It.IsAny<Guid>(), It.IsAny<SearchGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.SearchGroupVenues(groupId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetGroupVenuesResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task SearchGroupVenues_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_searchGroupVenueRequestValidatorMock);
        var request = new SearchGroupVenueRequest { SearchText = "Test", PageNumber = 0, PageSize = 10 };
        var expectedResponse = new GetGroupVenuesResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.SearchGroupVenuesAsync(It.IsAny<Guid>(), It.IsAny<SearchGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.SearchGroupVenues(groupId, request, ct);

        _groupVenueServiceMock.Verify(s => s.SearchGroupVenuesAsync(groupId, request, ct), Times.Once);
    }

    [Fact]
    public async Task CreateGroupVenue_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.CreateGroupVenue(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task CreateGroupVenue_InvalidRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_createGroupVenueRequestValidatorMock);
        var request = new CreateGroupVenueRequest { VenueName = "Test Venue", GroupId = groupId };

        var result = await _sut.CreateGroupVenue(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupVenueServiceMock.Verify(s => s.CreateGroupVenueAsync(It.IsAny<CreateGroupVenueRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateGroupVenue_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_createGroupVenueRequestValidatorMock);
        var request = new CreateGroupVenueRequest { VenueName = "Test Venue", GroupId = groupId };
        var expectedResponse = new AddGroupVenueResponse { StatusCode = HttpStatusCode.Created };
        _groupVenueServiceMock.Setup(s => s.CreateGroupVenueAsync(It.IsAny<CreateGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.CreateGroupVenue(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<AddGroupVenueResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task CreateGroupVenue_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_createGroupVenueRequestValidatorMock);
        var request = new CreateGroupVenueRequest { VenueName = "Test Venue", GroupId = groupId };
        var expectedResponse = new AddGroupVenueResponse { StatusCode = HttpStatusCode.Created };
        _groupVenueServiceMock.Setup(s => s.CreateGroupVenueAsync(It.IsAny<CreateGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.CreateGroupVenue(request, ct);

        _groupVenueServiceMock.Verify(s => s.CreateGroupVenueAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupVenue_NullRequest_ReturnsBadRequest()
    {
        var groupVenueId = Guid.NewGuid();

        var result = await _sut.UpdateGroupVenue(groupVenueId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateGroupVenue_InvalidRequest_ReturnsBadRequest()
    {
        var groupVenueId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_updateGroupVenueRequestValidatorMock);
        var request = new UpdateGroupVenueRequest { VenueName = "Updated Venue", Visited = true };

        var result = await _sut.UpdateGroupVenue(groupVenueId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _groupVenueServiceMock.Verify(s => s.UpdateGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<UpdateGroupVenueRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateGroupVenue_ValidRequest_ReturnsCorrectResult()
    {
        var groupVenueId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateGroupVenueRequestValidatorMock);
        var request = new UpdateGroupVenueRequest { VenueName = "Updated Venue", Visited = true };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.UpdateGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<UpdateGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.UpdateGroupVenue(groupVenueId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateGroupVenue_ValidRequest_CallsServiceCorrectly()
    {
        var groupVenueId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateGroupVenueRequestValidatorMock);
        var request = new UpdateGroupVenueRequest { VenueName = "Updated Venue", Visited = true };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.UpdateGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<UpdateGroupVenueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.UpdateGroupVenue(groupVenueId, request, ct);

        _groupVenueServiceMock.Verify(s => s.UpdateGroupVenueAsync(groupVenueId, request, ct), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupVenue_ValidRequest_ReturnsCorrectResult()
    {
        var groupVenueId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.DeleteGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.DeleteGroupVenue(groupVenueId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task DeleteGroupVenue_ValidRequest_CallsServiceCorrectly()
    {
        var groupVenueId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _groupVenueServiceMock.Setup(s => s.DeleteGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.DeleteGroupVenue(groupVenueId, ct);

        _groupVenueServiceMock.Verify(s => s.DeleteGroupVenueAsync(groupVenueId, ct), Times.Once);
    }
}