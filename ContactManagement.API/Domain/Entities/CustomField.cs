namespace ContactManagement.API.Domain.Entities;

public class CustomField
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "string"; // string, int, bool
}
