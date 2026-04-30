using Microsoft.AspNetCore.Mvc;

namespace Utilities.Models.Responses.Generic;

public class BadRequestResponse : ObjectResult
{
    public BadRequestResponse(ErrorResultResponse errorResult) : base(errorResult)
    {
        StatusCode = 400;
    }
}
