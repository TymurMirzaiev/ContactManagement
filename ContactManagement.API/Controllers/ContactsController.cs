using ContactManagement.API.Application.Common.Models;
using ContactManagement.API.Application.Features.Contacts.Commands.AssignCustomFieldValue;
using ContactManagement.API.Application.Features.Contacts.Commands.BulkMergeContacts;
using ContactManagement.API.Application.Features.Contacts.Commands.CreateContact;
using ContactManagement.API.Application.Features.Contacts.Commands.DeleteContact;
using ContactManagement.API.Application.Features.Contacts.Commands.UpdateContact;
using ContactManagement.API.Application.Features.Contacts.Queries.GetAllContacts;
using GetAllContacts = ContactManagement.API.Application.Features.Contacts.Queries.GetAllContacts;
using ContactManagement.API.Application.Features.Contacts.Queries.GetContactById;
using GetContactById = ContactManagement.API.Application.Features.Contacts.Queries.GetContactById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all contacts with pagination, sorting, and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetAllContacts.ContactDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? filterEmail = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Invalid pagination parameters");
        }

        var query = new GetAllContactsQuery(page, pageSize, sortBy, filterEmail);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a contact by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GetContactById.ContactDto>> GetById(Guid id)
    {
        var query = new GetContactByIdQuery(id);
        var contact = await _mediator.Send(query);

        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateContactResult>> Create([FromBody] CreateContactCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing contact
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Set the ID from the route parameter
            var commandWithId = command with { Id = id };
            var success = await _mediator.Send(commandWithId);

            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteContactCommand(id);
        var success = await _mediator.Send(command);

        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Bulk merge contacts by email
    /// Matches by email, updates existing, creates new, removes absent contacts
    /// </summary>
    [HttpPost("bulk-merge")]
    public async Task<IActionResult> BulkMerge([FromBody] BulkMergeContactsCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Bulk merge completed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Assign a custom field value to a contact
    /// </summary>
    [HttpPost("{id}/custom-fields/{fieldId}")]
    public async Task<IActionResult> AssignCustomFieldValue(
        Guid id,
        Guid fieldId,
        [FromBody] AssignCustomFieldValueRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var command = new AssignCustomFieldValueCommand(id, fieldId, request.Value);
            var success = await _mediator.Send(command);

            if (!success)
                return NotFound("Contact or CustomField not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class AssignCustomFieldValueRequest
{
    public object Value { get; set; } = null!;
}
