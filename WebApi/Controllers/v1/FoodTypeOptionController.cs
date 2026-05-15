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
/// Provides endpoints for managing food type options, including setting, adding, updating and deleting custom options,
/// as well as retrieving options for specific groups.
/// </summary>
/// <param name="foodTypeOptionService"></param>
/// <param name="setOptionsRequestValidator"></param>
/// <param name="setOptionRequestValidator"></param>
/// <param name="updateOptionRequestValidator"></param>
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class FoodTypeOptionController(
    IFoodTypeOptionService foodTypeOptionService,
    IValidator<SetOptionsRequest> setOptionsRequestValidator,
    IValidator<SetOptionRequest> setOptionRequestValidator,
    IValidator<UpdateOptionRequest> updateOptionRequestValidator)
    : ControllerBase
{
    private readonly IFoodTypeOptionService _foodTypeOptionService = foodTypeOptionService;
    private readonly IValidator<SetOptionsRequest> _setOptionsRequestValidator = setOptionsRequestValidator;
    private readonly IValidator<SetOptionRequest> _setOptionRequestValidator = setOptionRequestValidator;
    private readonly IValidator<UpdateOptionRequest> _updateOptionRequestValidator = updateOptionRequestValidator;

    /// <summary>
    /// Add custom food type options to a specific group.
    /// Pass the custom labels in the request and the options will be added for the group, but note that all existing food type options will be unmapped.
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

        var response = await _foodTypeOptionService.SetGroupCustomOptionsAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Remove custom food type options from a specific group.
    /// This will remove all custom options for the group and remap any existing options back to defaults.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{groupId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCustomOptions([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _foodTypeOptionService.RemoveGroupCustomOptionsAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Add a single custom food type option to a specific group.
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

        var response = await _foodTypeOptionService.AddOptionAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update a single custom food type option label for a specific group.
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

        var response = await _foodTypeOptionService.UpdateOptionAsync(optionId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Remove a single custom food type option from a specific group.
    /// This is used when custom options have been set up for the group, to make incremental changes.
    /// </summary>
    /// <param name="optionId"></param>
    /// <param name="ct"></param>
    [HttpDelete("custom/{optionId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCustomOption([FromRoute] Guid optionId, CancellationToken ct)
    {
        var response = await _foodTypeOptionService.DeleteOptionAsync(optionId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get the set of food type options for a group.
    /// This will be the default options if no custom options have been set.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet]
    [ProducesResponseType(typeof(GetTypeOptionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypeOptionsForGroup([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var response = await _foodTypeOptionService.GetGroupTypeOptionsAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get a single food type option by its ID.
    /// </summary>
    /// <param name="optionId"></param>
    /// <param name="ct"></param>
    [HttpGet("custom/{optionId}")]
    [ProducesResponseType(typeof(GetTypeOptionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypeOption([FromRoute] Guid optionId, CancellationToken ct)
    {
        var response = await _foodTypeOptionService.GetTypeOptionAsync(optionId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
