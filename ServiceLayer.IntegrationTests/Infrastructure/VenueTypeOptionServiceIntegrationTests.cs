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
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Options;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class VenueTypeOptionServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<VenueTypeOptionService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private OutputChecks<VenueTypeOptionService> _checks = new(new FakeLogger<VenueTypeOptionService>());
    private VenueTypeOptionService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<VenueTypeOptionService>();
        _checks = new OutputChecks<VenueTypeOptionService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        _service = new VenueTypeOptionService(
            tokenData: _tokenData.Object,
            venueTypeOptionRepository: new VenueTypeOptionRepository(_context),
            logger: _logger,
            groupRepository: new GroupRepository(_context),
            userGroupRepository: new UserGroupRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }

    #region SetGroupCustomOptionsAsync
    [Fact]
    public async Task SetGroupCustomOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SetGroupCustomOptionsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_InvalidGroup_ReturnsNotFound()
    {
        var request = new SetOptionsRequest
        {
            GroupId = Guid.Empty,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "SetGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_InactiveGroup_ReturnsNotFound()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup2Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "SetGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_NotInGroup_ReturnsForbidden()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup3Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "member", "SetGroupCustomOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_GroupAlreadyHasCustomOptions_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup3Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "already", "SetGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_ValidRequest_ReturnsCreated()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        for (int i = 0; i < typedResult.OptionsIds!.Count(); i++)
        {
            _context!.VenueTypeOptions.Should().Contain(x => x.VenueTypeOptionId == typedResult.OptionsIds!.Skip(i).First()
                && x.GroupId == TestGroup1Id && x.Label == request.Labels[i]);
        }

        var venues = _context!.GroupVenues.Where(x => x.GroupId == TestGroup1Id).ToList();
        foreach (var venue in venues)
        {
            venue.VenueTypeOptionId.Should().BeNull();
        }
    }
    #endregion

    #region RemoveGroupCustomOptionsAsync
    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "RemoveGroupCustomOptionsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_InvalidGroup_ReturnsNotFound()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "RemoveGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_InactiveGroup_ReturnsNotFound()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "not found", "RemoveGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_NotInGroup_ReturnsForbidden()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "member", "RemoveGroupCustomOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_GroupHasNoCustomOptions_ReturnsBadRequest()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "already", "RemoveGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_ValidRequest_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);

        _checks.OutputSuccessCheck(result, "removed", "RemoveGroupCustomOptionsAsync", HttpStatusCode.OK);
        _context!.VenueTypeOptions.Should().NotContain(x => x.GroupId == TestGroup3Id);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).ToList();
        foreach (var venue in venues)
        {
            venue.VenueTypeOptionId.Should().BeNull();
        }
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
