using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;

namespace ServiceLayer.Abstractions.Generic;

public interface ITypeOptionService
{
    Task<SetOptionsResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct);
    Task<SetOptionResponse> AddOptionAsync(SetOptionRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateOptionAsync(Guid optionId, UpdateOptionRequest request, CancellationToken ct);
    Task<CommonResponse> DeleteOptionAsync(Guid optionId, CancellationToken ct);
    Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct);
    Task<GetTypeOptionsResponse> GetGroupTypeOptionsAsync(Guid? groupId, CancellationToken ct);
    Task<GetTypeOptionResponse> GetTypeOptionAsync(Guid optionId, CancellationToken ct);
}
