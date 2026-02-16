using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ContactManagement.API.Application.Features.Contacts.Commands.BulkMergeContacts;

public record BulkMergeContactsCommand(List<BulkMergeContactDto> Contacts) : IRequest;

public class BulkMergeContactDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    public Dictionary<string, object>? CustomFields { get; set; }
}
