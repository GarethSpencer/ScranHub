using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Options;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class VenueTypeOptionController(
    IVenueTypeOptionService venueTypeOptionService,
    IValidator<SetOptionsRequest> setOptionsRequestValidator,
    IValidator<SetOptionRequest> setOptionRequestValidator,
    IValidator<UpdateOptionRequest> updateOptionRequestValidator)
    : ControllerBase
{
    private readonly IVenueTypeOptionService _venueTypeOptionService = venueTypeOptionService;
    private readonly IValidator<SetOptionsRequest> _setOptionsRequestValidator = setOptionsRequestValidator;
    private readonly IValidator<SetOptionRequest> _setOptionRequestValidator = setOptionRequestValidator;
    private readonly IValidator<UpdateOptionRequest> _updateOptionRequestValidator = updateOptionRequestValidator;

    [HttpPost]
    public async Task<IActionResult> SetCustomOptions([FromBody] SetOptionsRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _setOptionsRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _venueTypeOptionService.SetGroupCustomOptionsAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> RemoveCustomOptions([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _venueTypeOptionService.RemoveGroupCustomOptionsAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("custom")]
    public async Task<IActionResult> AddCustomOption([FromBody] SetOptionRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _setOptionRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _venueTypeOptionService.AddOptionAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("custom/{optionId}")]
    public async Task<IActionResult> UpdateCustomOption([FromRoute] Guid optionId, [FromBody] UpdateOptionRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _updateOptionRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _venueTypeOptionService.UpdateOptionAsync(optionId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("custom/{optionId}")]
    public async Task<IActionResult> RemoveCustomOption([FromRoute] Guid optionId, CancellationToken ct)
    {
        var response = await _venueTypeOptionService.DeleteOptionAsync(optionId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetTypeOptionsForGroup([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var response = await _venueTypeOptionService.GetGroupTypeOptionsAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("custom/{optionId}")]
    public async Task<IActionResult> GetTypeOption([FromRoute] Guid optionId, CancellationToken ct)
    {
        var response = await _venueTypeOptionService.GetTypeOptionAsync(optionId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
