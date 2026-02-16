Contact Management API – Technical Design



Overview

A RESTful ASP.NET Core Web API (.NET 8+) for managing contacts with flexible custom fields. Supports CRUD, bulk merge, and custom field operations. Uses SQL Server and Entity Framework Core.



Architecture

Framework: ASP.NET Core Web API (.NET 8+)

ORM: Entity Framework Core

Database: SQL Server

Documentation: Swagger/OpenAPI



Data Model



Contact

Id (Guid)

Name (string)

Email (string, unique)

Phone (string)

CreatedAt (DateTime)

UpdatedAt (DateTime)

CustomFields (JSON string, flexible key-value)



CustomField

Id (Guid)

Name (string)

DataType (string: string/int/bool)



Endpoints



Contacts

POST /contacts — Create

GET /contacts — List (pagination, sort, filter)

GET /contacts/{id} — Retrieve

PUT /contacts/{id} — Update

DELETE /contacts/{id} — Delete

POST /contacts/bulk-merge — Bulk merge by email



Custom Fields

GET /custom-fields — List

POST /custom-fields — Create

DELETE /custom-fields/{id} — Delete

POST /contacts/{id}/custom-fields/{fieldId} — Assign value



Bulk Merge Logic

Input: Collection of contacts

Matches by email, updates existing, creates new, removes absent contacts in DB

Runs as a single atomic transaction



Database Optimization

Unique index on contact Email

Index on CreatedAt for sorting



Testing

Integration tests for all endpoints

Test bulk merge and custom field scenarios



Design Rationale

Chose JSON for custom fields (fast prototyping/flexibility)

EF Core: Rapid development, migrations

Indexed key fields for performance



