using System.Net;

namespace Utilities.Models.Responses.Generic;

public class CommonResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Message { get; set; }
}
