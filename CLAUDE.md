# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Commands

**Database Setup:**
```bash
docker-compose up -d                    # Start SQL Server
```

**Development:**
```bash
cd ContactManagement.API
dotnet restore                          # Restore packages
dotnet build                            # Build project
dotnet run                              # Run API (http://localhost:5084)
                                        # Migrations apply automatically on startup
```

**Migrations:**
```bash
cd ContactManagement.API
dotnet ef migrations add MigrationName  # Create new migration (auto-applied on next run)
dotnet ef database update               # Manually apply migrations (optional)
dotnet ef migrations remove             # Remove last migration
```

**Database Access:**
```bash
# SQL Server credentials: sa / YourStrong@Passw0rd
docker exec -it contactmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd
```

## Architecture

### Clean Architecture with CQRS

The API follows a **folder-based clean architecture** using the **CQRS pattern** with **MediatR**:

```
ContactManagement.API/
├── Domain/                           # Pure business entities
│   └── Entities/
│       ├── Contact.cs
│       └── CustomField.cs
├── Application/                      # Use cases and business logic
│   ├── Features/
│   │   ├── Contacts/
│   │   │   ├── Commands/            # Write operations (Create, Update, Delete)
│   │   │   └── Queries/             # Read operations (GetAll, GetById)
│   │   └── CustomFields/
│   │       ├── Commands/
│   │       └── Queries/
│   ├── Common/
│   │   ├── Models/                  # Shared DTOs (PagedResult)
│   │   └── Interfaces/              # Application contracts (IContactManagementDbContext)
│   └── DependencyInjection.cs       # MediatR registration
├── Infrastructure/                   # External concerns
│   ├── Data/
│   │   ├── ContactManagementDbContext.cs
│   │   └── ContactManagementDbContextFactory.cs
│   ├── Migrations/
│   └── DependencyInjection.cs       # DbContext registration
├── Controllers/                      # Presentation layer (thin mediators)
│   ├── ContactsController.cs
│   └── CustomFieldsController.cs
└── Program.cs                        # Startup configuration
```

### Logical Dependency Flow
```
Controllers → Application → Domain
Controllers → Infrastructure → Domain
Infrastructure → Application (via IContactManagementDbContext)
```

### Request Flow (CQRS with MediatR)
```
HTTP Request → Controller → MediatR Command/Query → Handler → DbContext → Database
                    ↓                                     ↓
              Validation                         Business Logic
                                              + Transactions
                                              + Entity↔DTO Mapping
```

### Dependency Injection

Layered services are registered in `Program.cs`:
```csharp
builder.Services.AddApplication();          // MediatR handlers
builder.Services.AddInfrastructure(config); // DbContext + Interface
```

Each layer has its own `DependencyInjection.cs`:
- **Application**: Registers MediatR with auto-discovery of handlers
- **Infrastructure**: Registers DbContext and IContactManagementDbContext interface

## Key Patterns

### CQRS with MediatR

**Commands** (write operations) and **Queries** (read operations) are separated:
- Commands: `CreateContact`, `UpdateContact`, `DeleteContact`, `BulkMergeContacts`, `AssignCustomFieldValue`
- Queries: `GetAllContacts`, `GetContactById`, `GetAllCustomFields`

Each use case has:
- **Request class**: `CreateContactCommand`, `GetContactByIdQuery`
- **Handler class**: `CreateContactCommandHandler`, `GetContactByIdQueryHandler`
- **DTOs**: Colocated in feature folders (not shared globally)

**Example handler:**
```csharp
public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, CreateContactResult>
{
    private readonly IContactManagementDbContext _context;

    public async Task<CreateContactResult> Handle(CreateContactCommand request, CancellationToken ct)
    {
        // Business logic here
    }
}
```

**Controllers are thin** - they only mediate to MediatR:
```csharp
[HttpPost]
public async Task<ActionResult<CreateContactResult>> Create([FromBody] CreateContactCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

### DbContext Abstraction

Handlers access the database via **IContactManagementDbContext** interface (defined in Application layer):
```csharp
public interface IContactManagementDbContext
{
    DbSet<Contact> Contacts { get; }
    DbSet<CustomField> CustomFields { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

Infrastructure's `ContactManagementDbContext` implements this interface.

### Custom Fields as JSON

Custom fields are stored as JSON strings in the `Contact.CustomFields` column:
- **Entity level**: `string CustomFields = "{}"`
- **DTO level**: `Dictionary<string, object>? CustomFields`
- **Handler logic**: Serializes/deserializes with `System.Text.Json`

When modifying custom field logic:
1. Update the entity's JSON string in the database
2. Deserialize for DTOs in handlers
3. Validate custom field definitions exist when assigning values

### Bulk Merge Transaction Pattern

The `BulkMergeContactsCommandHandler` implements an atomic transaction:
1. Match incoming contacts by email
2. Update existing contacts
3. Create new contacts
4. **Remove contacts not in the input list** (full sync)
5. Commit or rollback as a unit

**Implementation:**
```csharp
public class BulkMergeContactsCommandHandler : IRequestHandler<BulkMergeContactsCommand>
{
    private readonly ContactManagementDbContext _context; // Direct DbContext for transactions

    public async Task Handle(BulkMergeContactsCommand request, CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // Merge logic
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
```

**Note**: BulkMerge handler uses `ContactManagementDbContext` directly (not `IContactManagementDbContext`) to access `Database.BeginTransactionAsync()`.

### Entity Framework Configuration

All EF Core configuration is in `Infrastructure/Data/ContactManagementDbContext.OnModelCreating()`:
- Email has a unique index for constraint enforcement
- CreatedAt has an index for sorting performance
- MaxLength constraints match DTO validation attributes

When adding new entities, configure them in `OnModelCreating()` to maintain consistency.

## Database

**Connection String:** `appsettings.json` → `ConnectionStrings:DefaultConnection`

**Auto-Migration:** Migrations are automatically applied on application startup via `dbContext.Database.Migrate()` in `Program.cs`.

**Critical Indexes:**
- `Contact.Email` - Unique index (prevents duplicate emails)
- `Contact.CreatedAt` - Index (optimizes sorting)

**Migration Location:** `ContactManagement.API/Infrastructure/Migrations/`

**Design-Time DbContext:** `ContactManagementDbContextFactory` enables migrations without running the full app.

## API Conventions

**Route Pattern:** All controllers use `[Route("api/[controller]")]`
- Contacts: `/api/contacts`
- Custom Fields: `/api/custom-fields`

**Pagination:** Default page=1, pageSize=10, max pageSize=100

**Sorting Options:** name, email, createdat, createdat_desc (default: createdat_desc)

**Error Handling:** Controllers return appropriate HTTP status codes:
- 200 OK - Success
- 201 Created - Resource created
- 204 No Content - Success with no response body
- 400 Bad Request - Validation error or exception
- 404 Not Found - Resource not found

## Adding New Features

### 1. Create a Command or Query

**Command example** (in `Application/Features/Contacts/Commands/NewFeature/`):
```csharp
public record NewFeatureCommand(Guid Id, string Data) : IRequest<bool>;

public class NewFeatureCommandHandler : IRequestHandler<NewFeatureCommand, bool>
{
    private readonly IContactManagementDbContext _context;

    public NewFeatureCommandHandler(IContactManagementDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(NewFeatureCommand request, CancellationToken ct)
    {
        // Business logic
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
```

**Query example** (in `Application/Features/Contacts/Queries/NewQuery/`):
```csharp
public record NewQuery(Guid Id) : IRequest<ResultDto>;

public class NewQueryHandler : IRequestHandler<NewQuery, ResultDto>
{
    private readonly IContactManagementDbContext _context;

    public async Task<ResultDto> Handle(NewQuery request, CancellationToken ct)
    {
        // Query logic
        return result;
    }
}

public class ResultDto { /* properties */ }
```

### 2. Add Controller Endpoint

```csharp
[HttpPost("new-feature")]
public async Task<IActionResult> NewFeature([FromBody] NewFeatureCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

MediatR automatically discovers and registers handlers - **no manual registration needed**.

## Testing

**Swagger UI:** Available in Development mode (check console output for URL)

The API has no automated tests currently. When adding tests, consider:
- Integration tests for bulk merge transaction rollback
- Custom field JSON serialization edge cases
- Email uniqueness constraint violations
- MediatR handler unit tests with mocked DbContext

## Package Dependencies

**MediatR:**
- `MediatR` (12.2.0) - CQRS pattern implementation

**Entity Framework Core:**
- `Microsoft.EntityFrameworkCore.SqlServer` (9.0.3)
- `Microsoft.EntityFrameworkCore.Design` (9.0.3)
- `Microsoft.EntityFrameworkCore.Tools` (9.0.3)

**API:**
- `Swashbuckle.AspNetCore` (7.2.0) - Swagger/OpenAPI

## Notes

- **No repository pattern**: Handlers access DbContext directly via interface abstraction
- **No FluentValidation**: Model validation uses DataAnnotations
- **Folder-based layers**: Single project with logical separation (not separate assemblies)
- **Feature folders**: DTOs colocated with their handlers, not shared in a Models folder
- **MediatR auto-discovery**: Handlers are automatically registered from the executing assembly
