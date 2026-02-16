using ContractManagement.Models;
using ContractManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Controllers;

[ApiController]
[Route("api/custom-fields")]
public class CustomFieldsController : ControllerBase
{
    private readonly ICustomFieldService _customFieldService;

    public CustomFieldsController(ICustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    /// <summary>
    /// Get all custom fields
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CustomFieldDto>>> GetAll()
    {
        var customFields = await _customFieldService.GetAllAsync();
        return Ok(customFields);
    }

    /// <summary>
    /// Create a new custom field definition
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomFieldDto>> Create([FromBody] CreateCustomFieldDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var customField = await _customFieldService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetAll), new { id = customField.Id }, customField);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a custom field definition
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _customFieldService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
