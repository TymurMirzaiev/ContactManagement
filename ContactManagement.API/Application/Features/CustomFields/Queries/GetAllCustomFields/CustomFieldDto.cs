namespace ContactManagement.API.Application.Features.CustomFields.Queries.GetAllCustomFields;

public class CustomFieldDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
}
