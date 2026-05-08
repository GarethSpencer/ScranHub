using Utilities.Models.Responses.Generic;

namespace Utilities.Models.Responses.Options;

public class SetOptionResponse : CommonResponse
{
    public Guid? OptionsId { get; set; }
}
