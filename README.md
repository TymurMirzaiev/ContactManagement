# Contact Management API

A RESTful ASP.NET Core Web API (.NET 10) for managing contacts with flexible custom fields. Supports CRUD operations, bulk merge, and custom field management with SQL Server and Entity Framework Core.

## Features

- Full CRUD operations for contacts
- Pagination, sorting, and filtering
- Bulk merge contacts by email (atomic transaction)
- Custom field definitions and value assignment
- SQL Server database with optimized indexes
- Swagger UI for API documentation and testing

## Architecture

- **Framework**: ASP.NET Core Web API (.NET 10)
- **ORM**: Entity Framework Core 9.0
- **Database**: SQL Server 2022 (Docker)
- **Documentation**: Swagger/Swashbuckle

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### 1. Start SQL Server Database

```bash
docker-compose up -d
```

This starts SQL Server 2022 in a Docker container on port 1433.

**Database credentials:**
- Server: `localhost,1433`
- Database: `ContactManagementDb`
- Username: `sa`
- Password: `YourStrong@Passw0rd`

### 2. Run the API

```bash
cd ContactManagement.API
dotnet run
```

**Note:** Database migrations are applied automatically on startup, so no manual migration step is needed.

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

## API Endpoints

### Contacts

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/contacts` | List all contacts (with pagination, sorting, filtering) |
| GET | `/api/contacts/{id}` | Get a contact by ID |
| POST | `/api/contacts` | Create a new contact |
| PUT | `/api/contacts/{id}` | Update an existing contact |
| DELETE | `/api/contacts/{id}` | Delete a contact |
| POST | `/api/contacts/bulk-merge` | Bulk merge contacts by email |
| POST | `/api/contacts/{id}/custom-fields/{fieldId}` | Assign custom field value |

### Custom Fields

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/custom-fields` | List all custom field definitions |
| POST | `/api/custom-fields` | Create a new custom field definition |
| DELETE | `/api/custom-fields/{id}` | Delete a custom field definition |

## Usage Examples

### Create a Contact

```bash
POST /api/contacts
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "+1234567890",
  "customFields": {
    "company": "Acme Corp",
    "position": "Developer"
  }
}
```

### List Contacts with Pagination and Filtering

```bash
GET /api/contacts?page=1&pageSize=10&sortBy=name&filterEmail=example.com
```

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `sortBy`: Sort field (name, email, createdat, createdat_desc)
- `filterEmail`: Filter by email (partial match)

### Bulk Merge Contacts

```bash
POST /api/contacts/bulk-merge
Content-Type: application/json

{
  "contacts": [
    {
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "+1234567890"
    },
    {
      "name": "Jane Smith",
      "email": "jane@example.com",
      "phone": "+0987654321"
    }
  ]
}
```

**Bulk Merge Logic:**
- Matches contacts by email
- Updates existing contacts
- Creates new contacts
- Removes contacts not in the provided list
- Runs as a single atomic transaction

### Create Custom Field Definition

```bash
POST /api/custom-fields
Content-Type: application/json

{
  "name": "salary",
  "dataType": "int"
}
```

**Supported Data Types:** `string`, `int`, `bool`

### Assign Custom Field Value to Contact

```bash
POST /api/contacts/{contactId}/custom-fields/{fieldId}
Content-Type: application/json

{
  "value": 75000
}
```

## Data Model

### Contact

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Contact name (max 200 chars) |
| Email | string | Email address (max 200 chars, unique) |
| Phone | string | Phone number (max 50 chars) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last update timestamp |
| CustomFields | JSON | Flexible key-value pairs |

### CustomField

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Field name (max 100 chars) |
| DataType | string | Data type (string/int/bool) |

## Database Optimization

- **Unique index** on Contact.Email for fast lookups and constraint enforcement
- **Index** on Contact.CreatedAt for efficient sorting
- **JSON column** for CustomFields providing flexibility without schema changes

## Development

### Create a New Migration

```bash
cd ContactManagement.API
dotnet ef migrations add MigrationName
# Migration will be applied automatically on next app startup
# Or manually apply with: dotnet ef database update
```

### Remove Last Migration

```bash
dotnet ef migrations remove
```

### View Database in Docker

```bash
docker exec -it contactmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd
```

## Testing

Use Swagger UI at `https://localhost:7001/swagger` for interactive API testing.

Or use curl/Postman/HTTPie:

```bash
# Create a contact
curl -X POST https://localhost:7001/api/contacts \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","phone":"123456"}'

# List contacts
curl https://localhost:7001/api/contacts

# Get contact by ID
curl https://localhost:7001/api/contacts/{id}
```

## Stopping the Database

```bash
docker-compose down
```

To also remove the database volume:

```bash
docker-compose down -v
```

## Design Decisions

- **JSON for custom fields**: Provides maximum flexibility for prototyping while maintaining type safety at the service layer
- **EF Core**: Enables rapid development with automatic migrations and LINQ queries
- **Indexed key fields**: Email uniqueness and CreatedAt indexing ensure good query performance
- **Atomic bulk merge**: Transaction-based bulk operations maintain data consistency
- **Service layer**: Separates business logic from controllers for better testability and maintainability

## License

MIT
