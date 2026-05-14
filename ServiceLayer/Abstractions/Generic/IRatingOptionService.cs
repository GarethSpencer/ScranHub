using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;

namespace ServiceLayer.Abstractions.Generic;

public interface IRatingOptionService
{
    Task<SetOptionsResponse> SetGroupCustomOptions(SetOptionsRequest request, CancellationToken ct);
    Task<SetOptionResponse> AddOption(Guid groupId, SetOptionRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateOption(Guid optionId, SetOptionRequest request, CancellationToken ct);
    Task<SetOptionResponse> DeleteOption(Guid optionId, CancellationToken ct);
    Task<CommonResponse> RemoveGroupCustomOptions(Guid groupId, CancellationToken ct);
    Task<CommonResponse> ReorderOptions(OrderOptionsRequest request, CancellationToken ct);
}
