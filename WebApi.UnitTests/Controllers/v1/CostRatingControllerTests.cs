using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;
using WebApi.Controllers.v1;

namespace WebApi.UnitTests.Controllers.v1;

public class CostRatingControllerTests
{
    private readonly Mock<ICostRatingService> _costRatingServiceMock = new();
    private readonly CostRatingController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public CostRatingControllerTests()
    {
        _sut = new CostRatingController(
            _costRatingServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateCostRating_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.CreateCostRating(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task CreateCostRating_ValidRequest_ReturnsCorrectResult()
    {
        var groupVenueId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var request = new CreateRatingRequest { GroupVenueId = groupVenueId, OptionId = optionId };
        var expectedResponse = new AddRatingResponse { StatusCode = HttpStatusCode.Created };
        _costRatingServiceMock.Setup(s => s.CreateRatingAsync(It.IsAny<CreateRatingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.CreateCostRating(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<AddRatingResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task CreateCostRating_ValidRequest_CallsServiceCorrectly()
    {
        var groupVenueId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var request = new CreateRatingRequest { GroupVenueId = groupVenueId, OptionId = optionId };
        var expectedResponse = new AddRatingResponse { StatusCode = HttpStatusCode.Created };
        _costRatingServiceMock.Setup(s => s.CreateRatingAsync(It.IsAny<CreateRatingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.CreateCostRating(request, ct);

        _costRatingServiceMock.Verify(s => s.CreateRatingAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task UpdateCostRating_NullRequest_ReturnsBadRequest()
    {
        var costRatingId = Guid.NewGuid();

        var result = await _sut.UpdateCostRating(costRatingId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateCostRating_ValidRequest_ReturnsCorrectResult()
    {
        var costRatingId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var request = new UpdateRatingRequest { OptionId = optionId };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.UpdateRatingAsync(It.IsAny<Guid>(), It.IsAny<UpdateRatingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.UpdateCostRating(costRatingId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateCostRating_ValidRequest_CallsServiceCorrectly()
    {
        var costRatingId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var request = new UpdateRatingRequest { OptionId = optionId };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.UpdateRatingAsync(It.IsAny<Guid>(), It.IsAny<UpdateRatingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.UpdateCostRating(costRatingId, request, ct);

        _costRatingServiceMock.Verify(s => s.UpdateRatingAsync(costRatingId, request, ct), Times.Once);
    }

    [Fact]
    public async Task DeleteCostRating_ValidRequest_ReturnsCorrectResult()
    {
        var costRatingId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.DeleteRatingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.DeleteCostRating(costRatingId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task DeleteCostRating_ValidRequest_CallsServiceCorrectly()
    {
        var costRatingId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.DeleteRatingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.DeleteCostRating(costRatingId, ct);

        _costRatingServiceMock.Verify(s => s.DeleteRatingAsync(costRatingId, ct), Times.Once);
    }

    [Fact]
    public async Task GetCostRating_ValidRequest_ReturnsCorrectResult()
    {
        var costRatingId = Guid.NewGuid();
        var expectedResponse = new GetRatingResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetRatingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetCostRating(costRatingId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetRatingResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetCostRating_ValidRequest_CallsServiceCorrectly()
    {
        var costRatingId = Guid.NewGuid();
        var expectedResponse = new GetRatingResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetRatingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetCostRating(costRatingId, ct);

        _costRatingServiceMock.Verify(s => s.GetRatingAsync(costRatingId, ct), Times.Once);
    }

    [Fact]
    public async Task GetCostRatingsForGroupVenue_ValidRequest_ReturnsCorrectResult()
    {
        var groupVenueId = Guid.NewGuid();
        var expectedResponse = new GetRatingsResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetRatingsForGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetCostRatingsForGroupVenue(groupVenueId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetRatingsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetCostRatingsForGroupVenue_ValidRequest_CallsServiceCorrectly()
    {
        var groupVenueId = Guid.NewGuid();
        var expectedResponse = new GetRatingsResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetRatingsForGroupVenueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetCostRatingsForGroupVenue(groupVenueId, ct);

        _costRatingServiceMock.Verify(s => s.GetRatingsForGroupVenueAsync(groupVenueId, ct), Times.Once);
    }

    [Fact]
    public async Task GetUserCostRatingsForGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetRatingsResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetUserRatingsForGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetUserCostRatingsForGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetRatingsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetUserCostRatingsForGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetRatingsResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetUserRatingsForGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetUserCostRatingsForGroup(groupId, ct);

        _costRatingServiceMock.Verify(s => s.GetUserRatingsForGroupAsync(groupId, ct), Times.Once);
    }

    [Fact]
    public async Task GetCostRatingsForGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetGroupRatingsResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetRatingsForGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetCostRatingsForGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetGroupRatingsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetCostRatingsForGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetGroupRatingsResponse { StatusCode = HttpStatusCode.OK };
        _costRatingServiceMock.Setup(s => s.GetRatingsForGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetCostRatingsForGroup(groupId, ct);

        _costRatingServiceMock.Verify(s => s.GetRatingsForGroupAsync(groupId, ct), Times.Once);
    }
}