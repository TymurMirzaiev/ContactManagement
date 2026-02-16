using ContractManagement.Models;
using ContractManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>
    /// Get all contacts with pagination, sorting, and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? filterEmail = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Invalid pagination parameters");
        }

        var result = await _contactService.GetAllAsync(page, pageSize, sortBy, filterEmail);
        return Ok(result);
    }

    /// <summary>
    /// Get a contact by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDto>> GetById(Guid id)
    {
        var contact = await _contactService.GetByIdAsync(id);
        if (contact == null) return NotFound();
        return Ok(contact);
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContactDto>> Create([FromBody] CreateContactDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var contact = await _contactService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await _contactService.UpdateAsync(id, updateDto);
            if (!success) return NotFound();
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
        var success = await _contactService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Bulk merge contacts by email
    /// Matches by email, updates existing, creates new, removes absent contacts
    /// </summary>
    [HttpPost("bulk-merge")]
    public async Task<IActionResult> BulkMerge([FromBody] BulkMergeRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _contactService.BulkMergeAsync(request.Contacts);
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
        [FromBody] AssignCustomFieldValueDto valueDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await _contactService.AssignCustomFieldValueAsync(id, fieldId, valueDto.Value);
            if (!success) return NotFound("Contact or CustomField not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}