using System.Net;

namespace Utilities.Models.Responses.GenericResponses;

public class CommonResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Message { get; set; }
}
