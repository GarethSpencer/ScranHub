using DAL.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class GroupServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<GroupService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private GroupService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<GroupService>();

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        var userRepository = new UserRepository(_context);

        _service = new GroupService(
            tokenData: _tokenData.Object,
            logger: _logger,
            userRepository: new UserRepository(_context),
            groupRepository: new GroupRepository(_context),
            userGroupRepository: new UserGroupRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }
    private void OutputFailureCheck(CommonResponse result, string errorText, string methodName, HttpStatusCode code)
    {
        result.StatusCode.Should().Be(code);
        result.Message?.ToLowerInvariant().Should().Contain(errorText.ToLowerInvariant());

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains(methodName));
    }

    private void OutputSuccessCheck(CommonResponse result, string successText, string methodName, HttpStatusCode code)
    {
        result.StatusCode.Should().Be(code);
        result.Message?.ToLowerInvariant().Should().Contain(successText.ToLowerInvariant());

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(methodName));
    }

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
