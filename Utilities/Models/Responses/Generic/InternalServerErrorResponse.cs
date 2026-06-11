using Microsoft.AspNetCore.Mvc;

namespace Utilities.Models.Responses.Generic;

public class InternalServerErrorResponse : ObjectResult
{
    public InternalServerErrorResponse(ErrorResultResponse errorResult) : base(errorResult)
    {
        StatusCode = 500;
    }
}
