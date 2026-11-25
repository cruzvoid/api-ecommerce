# 🤖 CLAUDE.md — api-ecommerce

> Context for Claude Code. Read it before working on this project.

## 📋 Description

Backend API RESTful using .NET9

## 🗂️ Category

learning

## 🛠️ Stack

<!-- Describe the technologies used by the project here -->
- .NET 9 (ASP.NET Core Web API, controllers)
- Entity Framework Core 9 (SQL Server provider, code-first migrations)
- AutoMapper 14 (entity ↔ DTO mapping)
- BCrypt.Net-Next (password hashing) + `System.IdentityModel.Tokens.Jwt` (token creation)
- OpenAPI (`Microsoft.AspNetCore.OpenApi`) + Swagger UI (`Swashbuckle.AspNetCore.SwaggerUI`, UI only)
- SQL Server 2022 (Developer edition)
- Dev Containers (Docker Compose)

## 📁 Structure

```
api-ecommerce/
├── .devcontainer/
│   ├── devcontainer.json
│   ├── docker-compose.yml
│   └── .env.example
├── scripts/
│   └── seed-computer-shop.sql   # sample categories/products (run via sqlcmd, see header)
├── src/
│   └── ApiEcommerce.Api/
│       ├── Controllers/         # Categories, Products, Users
│       ├── Data/                # ApplicationDbContext
│       ├── Mapping/             # AutoMapper profiles
│       ├── Migrations/          # EF Core migrations
│       ├── Models/              # Entities (Category, Product, User)
│       │   └── Dtos/            # Request/response DTOs
│       ├── Repository/          # Repository implementations
│       │   └── IRepository/     # Repository interfaces
│       ├── Program.cs           # DI registration and pipeline
│       └── ApiEcommerce.Api.http
├── ApiEcommerce.sln
├── CLAUDE.md
├── README.md
├── .editorconfig
└── .gitignore
```

## 🏗️ Architecture

- Request flow: Controller → repository interface (`Repository/IRepository`) → `ApplicationDbContext`. Controllers never touch the DbContext directly; they map entities to DTOs with `IMapper` and return DTOs.
- Each new repository needs an `AddScoped<IXRepository, XRepository>()` line in `Program.cs`. AutoMapper profiles in `Mapping/` are picked up automatically via `AddAutoMapper(typeof(Program).Assembly)`.
- `Product` has a required FK `CategoryId` → `Category`.
- `UserRepository` owns the auth logic: BCrypt hashing on register, and on login it verifies the password and issues a JWT signed with `ApiSettings:SecretKey` (env var `ApiSettings__SecretKey`). `Program.cs` validates tokens with JwtBearer, and controllers use `[Authorize(Roles = "Admin")]` + `[AllowAnonymous]` (see the JWT task below).
- Schema changes go through EF migrations; after editing an entity, run `dotnet ef migrations add <Name>` and review the generated file before `database update`.
- There is no test project. Verify changes manually via Swagger UI, `curl`, or `ApiEcommerce.Api.http`.

## ▶️ Running

Run these inside the dev container, from `src/ApiEcommerce.Api`:

```bash
dotnet ef migrations add <Name>   # only generates migration files
dotnet ef database update         # applies migrations to the DB
dotnet run --launch-profile http  # http://localhost:5117 (forwarded to the host by VS Code)
dotnet build --no-incremental     # full rebuild (`dotnet rebuild` doesn't exist)
```

- `appsettings.json` has an empty `DefaultConnection`; the real value comes from the container env var, so `dotnet ef` / `dotnet run` must run inside the container.
- There is no route at `/`. In Development, Swagger UI is at `/swagger` and the OpenAPI JSON at `/openapi/v1.json`.
- The spec comes from the built-in `AddOpenApi()`/`MapOpenApi()`; Swashbuckle is only used for the UI (`UseSwaggerUI` points at `/openapi/v1.json`), so don't add `AddSwaggerGen`.
- JWT in Swagger UI: `AddOpenApi(options => options.AddDocumentTransformer(...))` in `Program.cs` adds a `Bearer` security scheme (`SecuritySchemeType.Http`, `bearer`, `JWT`) plus a document-level security requirement, so Swagger UI shows the **Authorize** button. Paste only the token (no `Bearer ` prefix). `EnablePersistAuthorization()` keeps it across refreshes. The requirement is global, so `[AllowAnonymous]` endpoints also show a lock (cosmetic only).

### Fresh database setup

After creating or resetting the DB (`docker volume rm api-ecommerce-db-data`), inside the container from `src/ApiEcommerce.Api`:

1. `dotnet tool install --global dotnet-ef --version 9.0.21` (if `dotnet ef` is missing)
2. `dotnet ef migrations list`: shows applied migrations and `(Pending)` ones
3. `dotnet ef database update`: applies all pending migrations in order
4. Optional sample data: run `scripts/seed-computer-shop.sql` (see its header)
5. Register a user (`POST /api/users`) and set its `Role` to `Admin` in the DB

- If `database update` fails with `PendingModelChangesWarning`, the entities changed without a migration: run `dotnet ef migrations add <Name>`, review it, then update again. Don't suppress the warning.

## 🐳 Docker

The environment runs as a Dev Container defined in `.devcontainer/`:

| Service / container  | Image                                         | Role                          |
| -------------------- | --------------------------------------------- | ----------------------------- |
| `api-ecommerce-app`  | `mcr.microsoft.com/devcontainers/dotnet:2-9.0` | .NET 9 dev workspace          |
| `api-ecommerce-db`   | `mcr.microsoft.com/mssql/server:2022-latest`   | SQL Server, `127.0.0.1:1433`  |

Setup:

```bash
cp .devcontainer/.env.example .devcontainer/.env   # then set MSSQL_SA_PASSWORD and JWT_SECRET_KEY
```

Then in VS Code: **Dev Containers: Reopen in Container**. To manage it manually:

```bash
# Start
docker compose -f .devcontainer/docker-compose.yml up -d

# View logs
docker compose -f .devcontainer/docker-compose.yml logs -f

# Stop
docker compose -f .devcontainer/docker-compose.yml down
```

- From inside the app container the DB host is `api-ecommerce-db` (not `localhost`); from the host machine use `localhost,1433`.
- The connection string is injected as `ConnectionStrings__DefaultConnection` → read it with `GetConnectionString("DefaultConnection")`.
- DB data persists in the named volume `api-ecommerce-db-data` (`docker volume rm api-ecommerce-db-data` to reset the DB).
- The app container waits for the DB healthcheck (`sqlcmd SELECT 1`) before starting.

### Troubleshooting

- **"An error occurred setting up the container"**: run `docker start api-ecommerce-app` to see the real error. If it's `error mounting "/run/user/1000/wayland-N" ... not a directory` (stale Wayland socket after re-login), run `docker rm api-ecommerce-app && rmdir /run/user/1000/wayland-N`, then **Dev Containers: Rebuild Container**. The DB and its volume are unaffected.
- **`dotnet ef` not found** (after recreating the container): `dotnet tool install --global dotnet-ef --version 9.0.21`. NuGet packages aren't lost; check them with `dotnet list package`.

## 🔧 Conventions

- File names: kebab-case, except C# files, which use PascalCase matching the type name
- Interfaces exposed through public constructors (e.g. repositories) must be declared `public`
- Indentation: tabs of 4 (see `.editorconfig`; YAML and Markdown use 2 spaces)
- Commits: in English, descriptive
- Branches: `feature/name`, `fix/name`, `hotfix/name`, `docs/name`, `refactor/name`, `chore/name`, `style/name`

## ⚠️ Important notes

<!-- Add things Claude should know here: test credentials, limitations, etc. -->
- Never commit `.devcontainer/.env` (ignored by the `.env` rule); add new variables to `.devcontainer/.env.example`
- AutoMapper 14.0.0 triggers NU1903 (GHSA-rvv3-g6hj-g44x, DoS on deeply nested graphs; fixed in 15.1.1+, which requires a license key). Pending decision: keep, upgrade, or replace with manual mapping.

## 📌 Current status

<!-- Update this when you pause work -->
- Done and merged to `main`: Categories CRUD at `/api/categories` (PR #1), Swagger UI at `/swagger`, Products CRUD + search + `buyProduct` at `/api/products` (PR #2)
- Also merged: Users at `/api/users` (GET all, GET `{id:int}`, POST register, POST `Login`) (PR #3), global CORS policy (PR #4)
- In progress on `feature/jwt-authorization`: JWT validation + role-based authorization (committed `d05de62`), required `Username` migration (`d5f1610`), Swagger UI Bearer support (uncommitted, 2026-11-25)
- Next steps:
  - Decide on AutoMapper (see note above)
  - Minor cleanups in `CategoriesController`: duplicate `Name` assignment in Update, redundant `CategoryExists` check in Delete, typos in error messages, missing `204NoContent` response types
  - Consider `PUT` instead of `PATCH` for the full-replace update

### ⏸️ Paused experiment: JWT validation (#5), branch `feature/jwt-authorization`

Postponed until the user finishes the course module. Teach it step by step (the user writes the code).

- Already done and committed: password hash hidden from responses, failed login returns 401, Register forces `Role = "User"` (`095b43c`), JWT key moved to the env var `ApiSettings__SecretKey`, fed by `JWT_SECRET_KEY` in `.devcontainer/.env` (`3d16a43`)
- Problem: the API creates tokens but never validates them; every endpoint is public
- **Part A: authentication**
  1. `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.18` (stay on 9.0.x)
  2. `Program.cs`: read `ApiSettings:SecretKey` (throw at startup if empty), then `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` with `ValidateIssuerSigningKey = true`, `IssuerSigningKey = new SymmetricSecurityKey(UTF8 bytes of the key)`, `ValidateIssuer = false`, `ValidateAudience = false`
  3. `app.UseAuthentication()` right before `app.UseAuthorization()`
- Status (2026-11-16, committed in `d05de62`): Part A done (`ValidateAudience = false`, `UseAuthentication()` added). Part B done: every controller has `[Authorize(Roles = "Admin")]` at class level, with `[AllowAnonymous]` on Categories GETs, Products GETs/searches, and Users Register/Login. DB admin row updated to `Role = "Admin"`.
- **Part B: authorization**
  - `[Authorize]` on controllers, `[AllowAnonymous]` on Register/Login, `[Authorize(Roles = "Admin")]` on `GetUsers`
  - Role checks are case-sensitive: casing is now `Admin`/`User` in code and DB
  - Test with `curl`, the `.http` file (`Authorization: Bearer <token>` header), or Swagger UI's **Authorize** button (added 2026-11-25, see Running)
- **Optional exercise: per-operation lock in Swagger**: replace the global security requirement with `options.AddOperationTransformer(...)` that adds it only to endpoints whose metadata has `[Authorize]` and not `[AllowAnonymous]`
- **Next lesson: `BuyProduct`**: the reading and searching endpoints are open and the editing endpoints are Admin-only. One question for later is `BuyProduct`. Right now only admins can buy. In a store you'd probably want any logged-in user to buy. Teach it next, because it shows a new idea about how attributes combine (class-level and action-level `[Authorize]` stack as AND, so an action-level `[Authorize]` alone can't loosen the class's Admin requirement).
- Smaller leftovers from the review: mixed Spanish/English error messages, `_db` is `public` in `UserRepository`, `UserRegisterDto.ID` is a `string` (the DTO is now only referenced in `UserProfile`), unused usings (e.g. `Microsoft.EntityFrameworkCore.Storage` and `Microsoft.Extensions.Options` in `Program.cs`), add `401` to `ProducesResponseType` on Login, add `System.IdentityModel.Tokens.Jwt` explicitly to the `.csproj`, no unique index on `Username`
