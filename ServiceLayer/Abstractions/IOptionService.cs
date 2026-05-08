using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;

namespace ServiceLayer.Abstractions;

public interface IOptionService
{
    Task<SetOptionsResponse> SetGroupSpecificOptions(SetOptionsRequest request, CancellationToken ct);
    Task<SetOptionResponse> AddOption(Guid groupId, SetOptionRequest request, CancellationToken ct);
    Task<CommonResponse> UpdateOption(Guid optionId, SetOptionRequest request, CancellationToken ct);
    Task<SetOptionResponse> DeleteOption(Guid optionId, CancellationToken ct);
    Task<CommonResponse> RemoveGroupSpecificOptions(Guid groupId, CancellationToken ct);
    Task<CommonResponse> ReorderOptions(OrderOptionsRequest request, CancellationToken ct);
}
