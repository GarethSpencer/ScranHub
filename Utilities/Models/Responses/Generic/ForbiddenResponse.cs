using Microsoft.AspNetCore.Mvc;

namespace Utilities.Models.Responses.Generic;

public class ForbiddenResponse : ObjectResult
{
    public ForbiddenResponse(ErrorResultResponse errorResult) : base(errorResult)
    {
        StatusCode = 403;
    }
}
