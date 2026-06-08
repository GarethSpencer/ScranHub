using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for managing quality options, including setting, adding, updating, deleting, and reordering custom options,
/// as well as retrieving options for specific groups.
/// </summary>
/// <param name="qualityOptionService">Service for managing quality options.</param>
/// <param name="setOptionsRequestValidator">Validator for setting options requests.</param>
/// <param name="setOptionRequestValidator">Validator for setting a single option request.</param>
/// <param name="updateOptionRequestValidator">Validator for updating option requests.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class QualityOptionController(
    IQualityOptionService qualityOptionService,
    IValidator<SetOptionsRequest> setOptionsRequestValidator,
    IValidator<SetOptionRequest> setOptionRequestValidator,
    IValidator<UpdateOptionRequest> updateOptionRequestValidator)
    : ControllerBase
{
    private readonly IQualityOptionService _qualityOptionService = qualityOptionService;
    private readonly IValidator<SetOptionsRequest> _setOptionsRequestValidator = setOptionsRequestValidator;
    private readonly IValidator<SetOptionRequest> _setOptionRequestValidator = setOptionRequestValidator;
    private readonly IValidator<UpdateOptionRequest> _updateOptionRequestValidator = updateOptionRequestValidator;

    /// <summary>
    /// Add custom quality rating options to a specific group.
    /// Pass the custom labels in the request and the options will be added for the group, along with remappings of existing ratings.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType(typeof(SetOptionsResponse), StatusCodes.Status201Created)]
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

        var response = await _qualityOptionService.SetGroupCustomOptionsAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Remove custom quality rating options from a specific group.
    /// This will remove all custom options for the group and remap any existing ratings back to default options.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{groupId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCustomOptions([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityOptionService.RemoveGroupCustomOptionsAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Add a single custom quality rating option to a specific group.
    /// This is used when custom options have been set up for the group, to make incremental changes.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost("custom")]
    [ProducesResponseType(typeof(SetOptionResponse), StatusCodes.Status201Created)]
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

        var response = await _qualityOptionService.AddOptionAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update a single custom quality rating option label for a specific group.
    /// This is used when custom options have been set up for the group, to make incremental changes.
    /// </summary>
    /// <param name="optionId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("custom/{optionId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
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

        var response = await _qualityOptionService.UpdateOptionAsync(optionId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Remove a single custom quality rating option from a specific group.
    /// This is used when custom options have been set up for the group, to make incremental changes.
    /// </summary>
    /// <param name="optionId"></param>
    /// <param name="ct"></param>
    [HttpDelete("custom/{optionId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCustomOption([FromRoute] Guid optionId, CancellationToken ct)
    {
        var response = await _qualityOptionService.DeleteOptionAsync(optionId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Reorder the set of custom quality rating options for a specific group.
    /// This is used when custom options have been set up for the group, to change the rating associated with each label.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderCustomOptions([FromBody] OrderOptionsRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityOptionService.ReorderOptionsAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get the set of quality rating options for a group.
    /// This will be the default options if no custom options have been set.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet]
    [ProducesResponseType(typeof(GetRatingOptionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRatingOptionsForGroup([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var response = await _qualityOptionService.GetGroupRatingOptionsAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get a single quality rating option by its ID.
    /// </summary>
    /// <param name="optionId"></param>
    /// <param name="ct"></param>
    [HttpGet("custom/{optionId}")]
    [ProducesResponseType(typeof(GetRatingOptionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRatingOption([FromRoute] Guid optionId, CancellationToken ct)
    {
        var response = await _qualityOptionService.GetRatingOptionAsync(optionId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
