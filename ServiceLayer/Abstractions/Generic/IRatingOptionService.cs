using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions.Generic;

public interface IRatingOptionService
{
    Task<CommonResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct);
    Task<CommonResponse> AddOptionAsync(SetOptionRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateOptionAsync(Guid optionId, UpdateOptionRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteOptionAsync(Guid optionId, CancellationToken ct);
    Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct);
    Task<CommonResponse> ReorderOptionsAsync(OrderOptionsRequest request, CancellationToken ct);
    Task<CommonResponse> GetGroupRatingOptionsAsync(Guid? groupId, CancellationToken ct);
    Task<CommonResponse> GetRatingOptionAsync(Guid optionId, CancellationToken ct);
}
