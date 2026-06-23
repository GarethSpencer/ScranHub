using Microsoft.AspNetCore.Mvc;
using Utilities.Enums;
using Utilities.Models.Results;
using WebApi.Controllers.v1;

namespace WebApi.UnitTests.Controllers.v1;

public class LookupControllerTests
{
    private readonly LookupController _sut = new();

    [Fact]
    public void GetFriendshipStatuses_ReturnsCorrectResultType()
    {
        var result = _sut.GetFriendshipStatuses();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetFriendshipStatuses_ReturnsFriendshipStatusResultEnumValues()
    {
        var expectedCount = Enum.GetValues<FriendshipStatus>().Length;

        var result = _sut.GetFriendshipStatuses();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var statuses = Assert.IsType<List<FriendshipStatusResult>>(okResult.Value);
        Assert.Equal(expectedCount, statuses.Count);
    }

    [Fact]
    public void GetFriendshipStatuses_EachItemHasCorrectValueAndName()
    {
        var result = _sut.GetFriendshipStatuses();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var statuses = Assert.IsType<List<FriendshipStatusResult>>(okResult.Value);

        foreach (var status in Enum.GetValues<FriendshipStatus>().Cast<FriendshipStatus>())
        {
            var match = statuses.SingleOrDefault(s => s.Value == (int)status);
            Assert.NotNull(match);
            Assert.Equal(status.ToString(), match.Name);
        }
    }

    [Fact]
    public void GetFriendshipStatuses_CalledTwice_ReturnsSameInstance()
    {
        var first = (_sut.GetFriendshipStatuses() as OkObjectResult)!.Value;
        var second = (_sut.GetFriendshipStatuses() as OkObjectResult)!.Value;

        Assert.Same(first, second);
    }

    [Fact]
    public void GetGroupVenueSortParameters_ReturnsCorrectResultType()
    {
        var result = _sut.GetGroupVenueSortParameters();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetGroupVenueSortParameters_ReturnsGroupVenueSortParameterEnumValues()
    {
        var expectedCount = Enum.GetValues<GroupVenueSortParameters>().Length;

        var result = _sut.GetGroupVenueSortParameters();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var statuses = Assert.IsType<List<GroupVenueSortParametersResult>>(okResult.Value);
        Assert.Equal(expectedCount, statuses.Count);
    }

    [Fact]
    public void GetGroupVenueSortParameters_EachItemHasCorrectValueAndName()
    {
        var result = _sut.GetGroupVenueSortParameters();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var parameters = Assert.IsType<List<GroupVenueSortParametersResult>>(okResult.Value);

        foreach (var parameter in Enum.GetValues<GroupVenueSortParameters>().Cast<GroupVenueSortParameters>())
        {
            var match = parameters.SingleOrDefault(s => s.Value == (int)parameter);
            Assert.NotNull(match);
            Assert.Equal(parameter.ToString(), match.Name);
        }
    }

    [Fact]
    public void GetGroupVenueSortParameters_CalledTwice_ReturnsSameInstance()
    {
        var first = (_sut.GetGroupVenueSortParameters() as OkObjectResult)!.Value;
        var second = (_sut.GetGroupVenueSortParameters() as OkObjectResult)!.Value;

        Assert.Same(first, second);
    }
}