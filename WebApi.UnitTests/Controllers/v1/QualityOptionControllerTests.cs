using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;
using WebApi.Controllers.v1;
using WebApi.UnitTests.Helpers;

namespace WebApi.UnitTests.Controllers.v1;

public class QualityOptionControllerTests
{
    private readonly Mock<IQualityOptionService> _qualityOptionServiceMock = new();
    private readonly Mock<IValidator<SetOptionsRequest>> _setOptionsRequestValidatorMock = new();
    private readonly Mock<IValidator<SetOptionRequest>> _setOptionRequestValidatorMock = new();
    private readonly Mock<IValidator<UpdateOptionRequest>> _updateOptionRequestValidatorMock = new();
    private readonly QualityOptionController _sut;
    private static readonly CancellationToken ct = CancellationToken.None;

    public QualityOptionControllerTests()
    {
        _sut = new QualityOptionController(
            _qualityOptionServiceMock.Object,
            _setOptionsRequestValidatorMock.Object,
            _setOptionRequestValidatorMock.Object,
            _updateOptionRequestValidatorMock.Object
        );
    }

    [Fact]
    public async Task SetCustomOptions_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.SetCustomOptions(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task SetCustomOptions_InvalidRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_setOptionsRequestValidatorMock);
        var request = new SetOptionsRequest { GroupId = groupId, Labels = ["Test Label 1", "Test Label 2"] };

        var result = await _sut.SetCustomOptions(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _qualityOptionServiceMock.Verify(s => s.SetGroupCustomOptionsAsync(It.IsAny<SetOptionsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetCustomOptions_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_setOptionsRequestValidatorMock);
        var request = new SetOptionsRequest { GroupId = groupId, Labels = ["Test Label 1", "Test Label 2"] };
        var expectedResponse = new SetOptionsResponse { StatusCode = HttpStatusCode.Created };
        _qualityOptionServiceMock.Setup(s => s.SetGroupCustomOptionsAsync(It.IsAny<SetOptionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.SetCustomOptions(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<SetOptionsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task SetCustomOptions_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_setOptionsRequestValidatorMock);
        var request = new SetOptionsRequest { GroupId = groupId, Labels = ["Test Label 1", "Test Label 2"] };
        var expectedResponse = new SetOptionsResponse { StatusCode = HttpStatusCode.Created };
        _qualityOptionServiceMock.Setup(s => s.SetGroupCustomOptionsAsync(It.IsAny<SetOptionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.SetCustomOptions(request, ct);

        _qualityOptionServiceMock.Verify(s => s.SetGroupCustomOptionsAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task RemoveCustomOptions_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.RemoveGroupCustomOptionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.RemoveCustomOptions(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task RemoveCustomOptions_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.RemoveGroupCustomOptionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.RemoveCustomOptions(groupId, ct);

        _qualityOptionServiceMock.Verify(s => s.RemoveGroupCustomOptionsAsync(groupId, ct), Times.Once);
    }

    [Fact]
    public async Task AddCustomOption_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.AddCustomOption(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task AddCustomOption_InvalidRequest_ReturnsBadRequest()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_setOptionRequestValidatorMock);
        var request = new SetOptionRequest { GroupId = groupId, Label = "Test" };

        var result = await _sut.AddCustomOption(request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _qualityOptionServiceMock.Verify(s => s.AddOptionAsync(It.IsAny<SetOptionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddCustomOption_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_setOptionRequestValidatorMock);
        var request = new SetOptionRequest { GroupId = groupId, Label = "Test" };
        var expectedResponse = new SetOptionResponse { StatusCode = HttpStatusCode.Created };
        _qualityOptionServiceMock.Setup(s => s.AddOptionAsync(It.IsAny<SetOptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.AddCustomOption(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<SetOptionResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task AddCustomOption_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_setOptionRequestValidatorMock);
        var request = new SetOptionRequest { GroupId = groupId, Label = "Test" };
        var expectedResponse = new SetOptionResponse { StatusCode = HttpStatusCode.Created };
        _qualityOptionServiceMock.Setup(s => s.AddOptionAsync(It.IsAny<SetOptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.AddCustomOption(request, ct);

        _qualityOptionServiceMock.Verify(s => s.AddOptionAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomOption_NullRequest_ReturnsBadRequest()
    {
        var optionId = Guid.NewGuid();

        var result = await _sut.UpdateCustomOption(optionId, null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateCustomOption_InvalidRequest_ReturnsBadRequest()
    {
        var optionId = Guid.NewGuid();
        SetupHelpers.SetupValidatorFail(_updateOptionRequestValidatorMock);
        var request = new UpdateOptionRequest { Label = "Updated Label" };

        var result = await _sut.UpdateCustomOption(optionId, request, ct);

        Assert.IsType<BadRequestObjectResult>(result);
        _qualityOptionServiceMock.Verify(s => s.UpdateOptionAsync(It.IsAny<Guid>(), It.IsAny<UpdateOptionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCustomOption_ValidRequest_ReturnsCorrectResult()
    {
        var optionId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateOptionRequestValidatorMock);
        var request = new UpdateOptionRequest { Label = "Updated Label" };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.UpdateOptionAsync(It.IsAny<Guid>(), It.IsAny<UpdateOptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.UpdateCustomOption(optionId, request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateCustomOption_ValidRequest_CallsServiceCorrectly()
    {
        var optionId = Guid.NewGuid();
        SetupHelpers.SetupValidatorPass(_updateOptionRequestValidatorMock);
        var request = new UpdateOptionRequest { Label = "Updated Label" };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.UpdateOptionAsync(It.IsAny<Guid>(), It.IsAny<UpdateOptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.UpdateCustomOption(optionId, request, ct);

        _qualityOptionServiceMock.Verify(s => s.UpdateOptionAsync(optionId, request, ct), Times.Once);
    }

    [Fact]
    public async Task RemoveCustomOption_ValidRequest_ReturnsCorrectResult()
    {
        var optionId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.DeleteOptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.RemoveCustomOption(optionId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task RemoveCustomOption_ValidRequest_CallsServiceCorrectly()
    {
        var optionId = Guid.NewGuid();
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.DeleteOptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.RemoveCustomOption(optionId, ct);

        _qualityOptionServiceMock.Verify(s => s.DeleteOptionAsync(optionId, ct), Times.Once);
    }

    [Fact]
    public async Task ReorderCustomOptions_NullRequest_ReturnsBadRequest()
    {
        var result = await _sut.ReorderCustomOptions(null!, ct);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
    }

    [Fact]
    public async Task ReorderCustomOptions_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var request = new OrderOptionsRequest { GroupId = groupId, OptionsIds = [optionId1, optionId2] };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.ReorderOptionsAsync(It.IsAny<OrderOptionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.ReorderCustomOptions(request, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<CommonResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task ReorderCustomOptions_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var request = new OrderOptionsRequest { GroupId = groupId, OptionsIds = [optionId1, optionId2] };
        var expectedResponse = new CommonResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.ReorderOptionsAsync(It.IsAny<OrderOptionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.ReorderCustomOptions(request, ct);

        _qualityOptionServiceMock.Verify(s => s.ReorderOptionsAsync(request, ct), Times.Once);
    }

    [Fact]
    public async Task GetRatingOptionsForGroup_ValidRequest_ReturnsCorrectResult()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetRatingOptionsResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.GetGroupRatingOptionsAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetRatingOptionsForGroup(groupId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetRatingOptionsResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetRatingOptionsForGroup_ValidRequest_CallsServiceCorrectly()
    {
        var groupId = Guid.NewGuid();
        var expectedResponse = new GetRatingOptionsResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.GetGroupRatingOptionsAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetRatingOptionsForGroup(groupId, ct);

        _qualityOptionServiceMock.Verify(s => s.GetGroupRatingOptionsAsync(groupId, ct), Times.Once);
    }

    [Fact]
    public async Task GetRatingOption_ValidRequest_ReturnsCorrectResult()
    {
        var optionId = Guid.NewGuid();
        var expectedResponse = new GetRatingOptionResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.GetRatingOptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.GetRatingOption(optionId, ct);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.IsType<GetRatingOptionResponse>(statusCodeResult.Value);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
        Assert.Equal(expectedResponse, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetRatingOption_ValidRequest_CallsServiceCorrectly()
    {
        var optionId = Guid.NewGuid();
        var expectedResponse = new GetRatingOptionResponse { StatusCode = HttpStatusCode.OK };
        _qualityOptionServiceMock.Setup(s => s.GetRatingOptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        await _sut.GetRatingOption(optionId, ct);

        _qualityOptionServiceMock.Verify(s => s.GetRatingOptionAsync(optionId, ct), Times.Once);
    }
}