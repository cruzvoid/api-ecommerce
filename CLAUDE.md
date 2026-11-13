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
├── src/
│   └── ApiEcommerce.Api/
│       ├── Controllers/         # API controllers (CategoriesController)
│       ├── Data/                # ApplicationDbContext
│       ├── Mapping/             # AutoMapper profiles
│       ├── Migrations/          # EF Core migrations
│       ├── Models/              # Entities (Category)
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

## 🐳 Docker

The environment runs as a Dev Container defined in `.devcontainer/`:

| Service / container  | Image                                         | Role                          |
| -------------------- | --------------------------------------------- | ----------------------------- |
| `api-ecommerce-app`  | `mcr.microsoft.com/devcontainers/dotnet:2-9.0` | .NET 9 dev workspace          |
| `api-ecommerce-db`   | `mcr.microsoft.com/mssql/server:2022-latest`   | SQL Server, `127.0.0.1:1433`  |

Setup:

```bash
cp .devcontainer/.env.example .devcontainer/.env   # then set MSSQL_SA_PASSWORD
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
- Done: Categories CRUD at `/api/categories` (GET all, GET `{id:int}`, POST, PATCH `{id:int}`, DELETE `{id:int}`), merged to `main` (PR #1)
- Last task: Swagger UI at `/swagger` (Development only), verified working; not committed yet
- Next steps:
  - Commit the Swagger UI change
  - Decide on AutoMapper (see note above)
  - Minor cleanups in `CategoriesController`: duplicate `Name` assignment in Update, redundant `CategoryExists` check in Delete, typos in error messages, missing `204NoContent` response types
  - Consider `PUT` instead of `PATCH` for the full-replace update

### ⏸️ Paused experiment: JWT validation (#5), branch `feature/user-authentication-jwt`

Postponed until the user finishes the course module. Teach it step by step (the user writes the code).

- Already done and committed: password hash hidden from responses, failed login returns 401, Register forces `Role = "User"` (`095b43c`), JWT key moved to the env var `ApiSettings__SecretKey`, fed by `JWT_SECRET_KEY` in `.devcontainer/.env` (`3d16a43`)
- Problem: the API creates tokens but never validates them; every endpoint is public
- **Part A: authentication**
  1. `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.18` (stay on 9.0.x)
  2. `Program.cs`: read `ApiSettings:SecretKey` (throw at startup if empty), then `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` with `ValidateIssuerSigningKey = true`, `IssuerSigningKey = new SymmetricSecurityKey(UTF8 bytes of the key)`, `ValidateIssuer = false`, `ValidateAudience = false`
  3. `app.UseAuthentication()` right before `app.UseAuthorization()`
- **Part B: authorization**
  - `[Authorize]` on controllers, `[AllowAnonymous]` on Register/Login, `[Authorize(Roles = "Admin")]` on `GetUsers`
  - Role checks are case-sensitive: the DB admin row has `role = "admin"`, but code uses `"User"`. Pick one casing (e.g. `Admin`/`User`) and update the row
  - Test with `curl` or the `.http` file using an `Authorization: Bearer <token>` header (Swagger UI has no Bearer button without extra OpenAPI config)
- Smaller leftovers from the review: mixed Spanish/English error messages, `_db` is `public` in `UserRepository`, `UserRegisterDto.ID` is a `string` (the DTO is now only referenced in `UserProfile`), unused usings, add `401` to `ProducesResponseType` on Login, add `System.IdentityModel.Tokens.Jwt` explicitly to the `.csproj`, pending migration (`UserName` nullable → `Username` required) and no unique index on `Username`
