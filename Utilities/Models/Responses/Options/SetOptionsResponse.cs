using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.Options;

public class SetOptionsResponse : CommonResponse
{
    public IEnumerable<Guid>? OptionsIds { get; set; }
}
