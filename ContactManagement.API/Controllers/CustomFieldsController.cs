using ContactManagement.API.Application.Features.CustomFields.Commands.CreateCustomField;
using ContactManagement.API.Application.Features.CustomFields.Commands.DeleteCustomField;
using ContactManagement.API.Application.Features.CustomFields.Queries.GetAllCustomFields;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Controllers;

[ApiController]
[Route("api/custom-fields")]
public class CustomFieldsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomFieldsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all custom fields
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CustomFieldDto>>> GetAll()
    {
        var query = new GetAllCustomFieldsQuery();
        var customFields = await _mediator.Send(query);
        return Ok(customFields);
    }

    /// <summary>
    /// Create a new custom field definition
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateCustomFieldResult>> Create([FromBody] CreateCustomFieldCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
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
        var command = new DeleteCustomFieldCommand(id);
        var success = await _mediator.Send(command);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
