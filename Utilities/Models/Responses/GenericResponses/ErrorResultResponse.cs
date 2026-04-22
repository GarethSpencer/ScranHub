using System.Text.Json;

namespace Utilities.Models.Responses.GenericResponses;

public class ErrorResultResponse
{
    public string[]? Errors { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
