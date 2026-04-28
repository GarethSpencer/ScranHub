using DAL.Data;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Token;

namespace RepositoryLayer.Infrastructure.Generic;

public class UnitOfWork : IUnitOfWork
{
    protected readonly ScranHubDbContext _dbContext;
    private readonly ITokenData _tokenData;

    public UnitOfWork(ScranHubDbContext dbContext, ITokenData tokenData)
    {
        _dbContext = dbContext;
        _tokenData = tokenData;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (_tokenData?.UserId != null)
        {
            return await _dbContext.SaveChangesAsync(_tokenData.UserId.Value, ct);
        }
        else
        {
            return await _dbContext.SaveChangesAsync(ct);
        }
    }
}
