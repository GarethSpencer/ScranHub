using Utilities.Models.Responses.Generic;
using Utilities.Models.Results.Generic;

namespace Utilities.Models.Responses.Options;

public class GetTypeOptionResponse : CommonResponse
{
    public TypeOptionResult? Option { get; set; }
}
