using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.UnitTests.Helpers;
using System.Net;
using Utilities.Enums;
using Utilities.Models.Responses.Users;
using Utilities.Models.Results;
using Utilities.Token;

namespace ServiceLayer.UnitTests.Infrastructure;

public class UserServiceTests
{
    private readonly UserService _service;
    private readonly Mock<ITokenData> _tokenData;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IUserFriendRepository> _userFriendRepository;
    private readonly FakeLogger<UserService> _logger;

    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid TestFriend1Id = Guid.Parse("00000000-0000-0000-0001-000000000000");
    private static readonly Guid TestFriend2Id = Guid.Parse("00000000-0000-0000-0002-000000000000");
    private static readonly Guid TestFriend3Id = Guid.Parse("00000000-0000-0000-0003-000000000000");
    private static readonly Guid TestFriend4Id = Guid.Parse("00000000-0000-0000-0004-000000000000");
    private static readonly CancellationToken ct = CancellationToken.None;

    public UserServiceTests()
    {
        _tokenData = new Mock<ITokenData>();
        _userRepository = new Mock<IUserRepository>();
        _userFriendRepository = new Mock<IUserFriendRepository>();
        _logger = new FakeLogger<UserService>();

        _tokenData.Setup(x => x.UserId).Returns(TestUserId);

        _service = new UserService(
            tokenData: _tokenData.Object,
            logger: _logger,
            userRepository: _userRepository.Object,
            userFriendRepository: _userFriendRepository.Object,
            unitOfWork: new Mock<IUnitOfWork>().Object
            );
    }

    [Fact]
    public async Task GetFriendsForUserAsync_ReturnsNotFoundWithInvalidToken()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service.GetFriendsForUserAsync(ct);
        result.Should().NotBeOfType<UserFriendsResponse>();

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Message?.ToLowerInvariant().Should().Contain("unauthorized");

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("GetFriendsForUserAsync"));
    }

    [Fact]
    public async Task GetFriendsForUserAsync_ReturnsNotFoundWithInvalidId()
    {
        _userRepository.Setup(x => x.GetFriendsForUserAsync(TestUserId, ct))
            .ReturnsAsync((IEnumerable<FriendResult>?)null);

        var result = await _service.GetFriendsForUserAsync(ct);
        result.Should().NotBeOfType<UserFriendsResponse>();

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message?.ToLowerInvariant().Should().Contain("found");

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("GetFriendsForUserAsync"));
    }

    [Fact]
    public async Task GetFriendsForUserAsync_ReturnsOkWithFriendData()
    {
        _userRepository.Setup(x => x.GetFriendsForUserAsync(TestUserId, ct))
            .ReturnsAsync(GetFriendResults());

        var result = await _service.GetFriendsForUserAsync(ct);
        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;

        typedResult.UserId.Should().Be(TestUserId);
        typedResult.Friends?.Count().Should().Be(4);
        typedResult.StatusCode.Should().Be(HttpStatusCode.OK);
        typedResult.FriendCount.Should().Be(2);
        typedResult.Message?.ToLowerInvariant().Should().Contain("success");

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("GetFriendsForUserAsync"));
    }

    [Fact]
    public async Task GetFriendsForUserAsync_ReturnsOkWithNoFriendData()
    {
        _userRepository.Setup(x => x.GetFriendsForUserAsync(TestUserId, ct))
            .ReturnsAsync([]);

        var result = await _service.GetFriendsForUserAsync(ct);
        var typedResult = result.Should().BeOfType<UserFriendsResponse>().Subject;

        typedResult.UserId.Should().Be(TestUserId);
        typedResult.Friends?.Count().Should().Be(0);
        typedResult.StatusCode.Should().Be(HttpStatusCode.OK);
        typedResult.FriendCount.Should().Be(0);
        typedResult.Message?.ToLowerInvariant().Should().Contain("success");

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains("GetFriendsForUserAsync"));
    }

    private static List<FriendResult> GetFriendResults() => [
            new FriendResult
            {
                UserFriendId = TestFriend1Id,
                Active = true,
                DisplayName = "Adam",
                Status = FriendshipStatus.Accepted,
                Initiator = false,
                FriendId =  TestUserId
            },
            new FriendResult
            {
                UserFriendId = TestFriend2Id,
                Active = true,
                DisplayName = "Beth",
                Status = FriendshipStatus.Accepted,
                Initiator = true,
                FriendId = TestUserId
            },
            new FriendResult
            {
                UserFriendId = TestFriend3Id,
                Active = true,
                DisplayName = "Carol",
                Status = FriendshipStatus.Pending,
                Initiator = true,
                FriendId = TestUserId
            },
            new FriendResult
            {
                UserFriendId = TestFriend4Id,
                Active = true,
                DisplayName = "Doris",
                Status = FriendshipStatus.Declined,
                Initiator = true,
                FriendId = TestUserId
            }
        ];
}
