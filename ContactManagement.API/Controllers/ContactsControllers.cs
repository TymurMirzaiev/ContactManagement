using ContractManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactsController : ControllerBase
{
    // Temporary in-memory store (replace with DB/EF later)
    private static readonly List<ContactDto> _contacts = new List<ContactDto>();

    [HttpGet]
    public ActionResult<IEnumerable<ContactDto>> GetAll()
    {
        return Ok(_contacts);
    }

    [HttpGet("{id}")]
    public ActionResult<ContactDto> GetById(Guid id)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        if (contact == null) return NotFound();
        return Ok(contact);
    }

    [HttpPost]
    public ActionResult<ContactDto> Create(ContactDto contact)
    {
        contact.Id = Guid.NewGuid();
        contact.CreatedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;
        _contacts.Add(contact);
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, ContactDto contact)
    {
        var dbContact = _contacts.FirstOrDefault(c => c.Id == id);
        if (dbContact == null) return NotFound();

        dbContact.Name = contact.Name;
        dbContact.Email = contact.Email;
        dbContact.Phone = contact.Phone;
        dbContact.UpdatedAt = DateTime.UtcNow;
        dbContact.CustomFields = contact.CustomFields;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        if (contact == null) return NotFound();

        _contacts.Remove(contact);
        return NoContent();
    }
}