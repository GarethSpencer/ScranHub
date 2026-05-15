using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;

namespace ServiceLayer.Abstractions.Generic;

public interface IRatingOptionService
{
    Task<SetOptionsResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct);
    Task<SetOptionResponse> AddOptionAsync(SetOptionRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateOptionAsync(Guid optionId, UpdateOptionRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteOptionAsync(Guid optionId, CancellationToken ct);
    Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct);
    Task<CommonResponse> ReorderOptionsAsync(OrderOptionsRequest request, CancellationToken ct);
    Task<GetRatingOptionsResponse> GetGroupRatingOptionsAsync(Guid? groupId, CancellationToken ct);
    Task<GetRatingOptionResponse> GetRatingOptionAsync(Guid optionId, CancellationToken ct);
}
