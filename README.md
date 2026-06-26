# ScranHub API

> The backend for [ScranHub](https://github.com/garethspencer/ScranHubClient) — plan and rate your scranning adventures with your friends!

This repository contains the **REST API and supporting services** that power ScranHub. It serves the [ScranHub web client](https://github.com/garethspencer/ScranHubClient), handling all functionality around friends, groups, venues, ratings, and the associated configurations that drive the app.

## Features

- **RESTful API** for users, friends, groups, group venues, options, ratings, and lookup options.
- **JWT authentication** via [Auth0](https://auth0.com/) — every endpoint requires an authenticated user by default unless explicitly marked anonymous.
- **API versioning** using URL segments (e.g. `/api/v1/...`), defaulting to `v1`.
- **Interactive API docs** in development through both [Swagger UI](https://swagger.io/tools/swagger-ui/) and [Scalar](https://scalar.com/), each wired up for Auth0 login so you can try authenticated endpoints.
- **Background processing** via an Azure Functions app (a daily job that removes long-term inactive users).
- **Health checks** at `/health`, including SQL Server connectivity.
- **Structured logging** with [Serilog](https://serilog.net/).
- **Secrets management** through [Azure Key Vault](https://azure.microsoft.com/products/key-vault) in non-development environments.
- **Full Unit Testing** of the API controllers and Utilities logic and validations.
- **Full Integration Testing** of the Service Layer orchestration including repository-level EF Core mappings and database changes.

## Tech stack

| Area | Technology |
| --- | --- |
| Runtime | [.NET 10](https://dotnet.microsoft.com/) / ASP.NET Core |
| Language | C# |
| Data access | [Entity Framework Core](https://learn.microsoft.com/ef/core/) + SQL Server |
| Auth | [Auth0](https://auth0.com/) (JWT Bearer) |
| API versioning | [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) |
| API docs | [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) + [Scalar](https://scalar.com/) |
| Logging | [Serilog](https://serilog.net/) |
| Background jobs | [Azure Functions](https://azure.microsoft.com/products/functions) (isolated worker) |
| Hosting | [Azure App Service](https://azure.microsoft.com/products/app-service) + [Azure Key Vault](https://azure.microsoft.com/products/key-vault) |

## Architecture

The solution is organised into layers, each as its own project, so that responsibilities are separated and dependencies flow in a single direction:

```
WebApi            HTTP API — controllers, auth, versioning, middleware, API docs
  └─ ServiceLayer   Business logic and orchestration
      └─ RepositoryLayer   Data access abstractions (unit of work, repositories)
          └─ DAL             EF Core DbContext, entities, and migrations
Utilities         Cross-cutting helpers and shared models (used by all layers)
Functions         Azure Functions app for scheduled background work
```

Test projects accompany the codebase: `WebApi.UnitTests`, `Utilities.UnitTests`, and `ServiceLayer.IntegrationTests`.

## Database schema

The diagram below shows the database tables and their relationships. The source is an SVG file generated from [draw.io](https://www.drawio.com/).

![ScranHub database schema](docs/ScranHub-DatabaseDesign.svg)

The main tables are:

| Table | Description |
| --- | --- |
| `Users` | Application users, linked to an Auth0 identity. |
| `UserFriends` | Friendship links between two users (with request status). |
| `Groups` | Groups that users can create and join. |
| `UserGroups` | Membership join between users and groups. |
| `GroupVenues` | Venues added to a group, optionally tagged with a food type and venue type. May also store Google Places details for a selected real-world location. |
| `FoodTypeOptions` | Configurable food types used to categorise venues. |
| `VenueTypeOptions` | Configurable venue types used to categorise venues. |
| `QualityOptions` | Configurable quality rating scale (per group). |
| `CostOptions` | Configurable cost rating scale (per group). |
| `QualityRatings` | A user's quality rating for a group venue, referencing a quality option. |
| `CostRatings` | A user's cost rating for a group venue, referencing a cost option. |

All tables share a common auditable base (created/updated by and timestamps).

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A SQL Server instance (LocalDB, a local container, or a remote database)
- An [Auth0](https://auth0.com/) API + application configured for authentication
- (Optional) [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) to run the `Functions` project locally

### Clone & restore

```bash
git clone https://github.com/garethspencer/ScranHub.git
cd ScranHub
dotnet restore
```

### Configure

Application configuration lives in `WebApi/appsettings.json`, with development overrides in `appsettings.Development.json`. The key settings are:

| Setting | Description |
| --- | --- |
| `ConnectionStrings:Default` | Connection string for the SQL Server database. |
| `Auth0:Authority` | Your Auth0 tenant authority URL (e.g. `https://your-tenant.auth0.com/`). |
| `Auth0:Audience` | The API identifier/audience registered in Auth0. |
| `Auth0:ClientId` | The Auth0 application Client ID used by the Swagger/Scalar login flow. |
| `AZURE_KEY_VAULT_URL` | (Non-development only) URL of the Key Vault to load secrets from. |

> **Note:** keep real secrets — especially the database connection string — out of source control. In development use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or `appsettings.Development.json` (git-ignored); in production they are loaded from Azure Key Vault.

### Database migrations

The database schema is managed with EF Core migrations in the `DAL` project.
Pending migrations are **applied automatically on application startup**
(`app.ApplyMigrations()` in `WebApi`), so simply running the API brings the
database up to date — locally and on deploy. There is no need to run
`dotnet ef database update` by hand.

When you change an entity, create the migration (this still has to be done
manually, as it requires the design-time tooling):

```bash
dotnet ef migrations add <MigrationName> --project DAL --startup-project WebApi
```

It is then applied the next time the API starts.

### Seed the initial admin user

Once the schema is in place, insert the initial admin user so there is an account with admin privileges to work with. Run the following against your database:

```sql
INSERT INTO Users ([UserId], [AuthId], [DisplayName], [Active], [Admin], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn], [Email])
VALUES ('00000000-0000-0000-0000-000000000001', NULL, 'Admin User', 1, 1, '00000000-0000-0000-0000-000000000001', GETUTCDATE(), NULL, NULL, 'test-email@scranhub.com')
```

The `AuthId` is left `NULL` and is linked to an Auth0 identity the first time that user signs in. Update the `DisplayName` and `Email` to fit your needs.

### Run

```bash
dotnet run --project WebApi
```

By default the API listens on:

- `https://localhost:7079`
- `http://localhost:5234`

## API documentation

In **development**, two interactive docs UIs are served, both pre-configured for Auth0 (Authorization Code + PKCE) so you can sign in and call protected endpoints:

| Tool | URL |
| --- | --- |
| Scalar | <https://localhost:7079/scalar/v1> |
| Swagger UI | <https://localhost:7079/swagger> |

To call a protected endpoint, use the **Authorize** / login option in either UI to authenticate with Auth0, then send your request — the access token is attached automatically.

> Swagger and Scalar are only enabled when running in the Development environment. They are not exposed in production.

### Calling the production API

The deployed API is consumed by the hosted ScranHub web client. Requests must include a valid Auth0 **Bearer token** in the `Authorization` header:

```http
GET /api/v1/...
Authorization: Bearer <your-auth0-access-token>
```

Tokens must be issued by the configured Auth0 authority for the configured audience. CORS in production is restricted to the ScranHub web client's origin.

## Background jobs

The `Functions` project is an Azure Functions app for scheduled work that shouldn't run inside the request pipeline. Run it locally with:

```bash
cd Functions
func start
```

(Requires the Azure Functions Core Tools and a `local.settings.json` with the relevant configuration.)

## Testing

```bash
dotnet test
```

This runs the unit and integration test projects across the solution.

## Deployment

Deployment is handled by the GitHub Actions workflow in [`.github/workflows`](.github/workflows):

- The **WebApi** is published to **Azure App Service** (`app-scranhub-api`).
- The **Functions** app is published to **Azure Functions** (`scranhubfunctions`).

Tests run as part of the pipeline before publish. In hosted environments, secrets are pulled from Azure Key Vault rather than configuration files.

## Related repositories

- **Frontend client:** [garethspencer/ScranHubClient](https://github.com/garethspencer/ScranHubClient)

## License

[MIT](LICENSE.txt)
