using System.Text.Json.Serialization;

namespace ContactManagement.API.Application.Features.Contacts.Queries.GetAllContacts;

public class ContactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? CustomFields { get; set; }
}
