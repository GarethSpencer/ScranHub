using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Options;

public class GetTypeOptionsResponse : CommonResponse
{
    public IEnumerable<TypeOptionResult>? Options { get; set; }
}
