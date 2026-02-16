# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Commands

**Database Setup:**
```bash
docker-compose up -d                    # Start SQL Server
cd ContactManagement.API
dotnet ef database update               # Apply migrations
```

**Development:**
```bash
cd ContactManagement.API
dotnet restore                          # Restore packages
dotnet build                            # Build project
dotnet run                              # Run API (https://localhost:7001)
```

**Migrations:**
```bash
cd ContactManagement.API
dotnet ef migrations add MigrationName  # Create new migration
dotnet ef database update               # Apply migrations
dotnet ef migrations remove             # Remove last migration
```

**Database Access:**
```bash
# SQL Server credentials: sa / YourStrong@Passw0rd
docker exec -it contactmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd
```

## Architecture

### Layered Structure
The API follows a clean layered architecture:
- **Controllers** (`Controllers/`) - HTTP endpoints, request validation, route to services
- **Services** (`Services/`) - Business logic, transactions, entity-DTO mapping
- **Entities** (`Entities/`) - EF Core database entities
- **Models** (`Models/`) - DTOs for API requests/responses
- **Data** (`Data/`) - DbContext and EF Core configuration

### Request Flow
```
HTTP Request → Controller → Service → DbContext → Database
                    ↓           ↓
              Validation    Business Logic
                         + Transactions
                         + Entity↔DTO Mapping
```

### Dependency Injection
Services are registered in `Program.cs`:
```csharp
builder.Services.AddDbContext<ContactManagementDbContext>(...)
builder.Services.AddScoped<IContactService, ContactService>()
builder.Services.AddScoped<ICustomFieldService, CustomFieldService>()
```

## Key Patterns

### Custom Fields as JSON
Custom fields are stored as JSON strings in the `Contact.CustomFields` column, providing schema flexibility:
- **Entity level**: `string CustomFields = "{}"`
- **DTO level**: `Dictionary<string, object>? CustomFields`
- **Service layer**: Handles JSON serialization/deserialization with `System.Text.Json`

When modifying custom field logic, remember to:
1. Update the entity's JSON string in the database
2. Deserialize for DTOs
3. Validate custom field definitions exist when assigning values

### Bulk Merge Transaction Pattern
The bulk merge endpoint (`POST /api/contacts/bulk-merge`) implements an atomic transaction:
1. Match incoming contacts by email
2. Update existing contacts
3. Create new contacts
4. **Remove contacts not in the input list** (full sync)
5. Commit or rollback as a unit

This is implemented in `ContactService.BulkMergeAsync()` using `BeginTransactionAsync()`.

### Entity Framework Configuration
All EF Core configuration is in `ContactManagementDbContext.OnModelCreating()`:
- Email has a unique index for constraint enforcement
- CreatedAt has an index for sorting performance
- MaxLength constraints match DTO validation attributes

When adding new entities, configure them in `OnModelCreating()` to maintain consistency.

## Database

**Connection String:** `appsettings.json` → `ConnectionStrings:DefaultConnection`

**Critical Indexes:**
- `Contact.Email` - Unique index (prevents duplicate emails)
- `Contact.CreatedAt` - Index (optimizes sorting)

**Migration Location:** `ContactManagement.API/Migrations/`

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

## Testing

**Swagger UI:** `https://localhost:7001/swagger` (only in Development)

The API has no automated tests currently. When adding tests, consider:
- Integration tests for bulk merge transaction rollback
- Custom field JSON serialization edge cases
- Email uniqueness constraint violations
