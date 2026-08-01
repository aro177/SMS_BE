# Repository Guidelines

## Project Structure & Module Organization

This repository contains a single .NET 9 ASP.NET Core Web API project. `Program.cs` configures dependency injection, JWT authentication, CORS, PostgreSQL/EF Core, Supabase, and the HTTP pipeline. API endpoints live in `Controllers/`; business logic belongs in `Services/` with contracts in `Services/Interfaces/`; persistence code follows the same pattern under `Repositories/`. Domain entities and `AppDbContext` are in `Models/`, while request and response types are grouped by feature in `Dtos/`. Shared helpers live in `Common/`, external-service clients in `Integrations/`, and application setup types in `Configs/`. Keep generated `bin/` and `obj/` content out of reviews.

## Build, Test, and Development Commands

- `dotnet restore` downloads NuGet dependencies.
- `dotnet build "Student Management System.csproj"` compiles the API and reports analyzer/compiler warnings.
- `dotnet run --project "Student Management System.csproj"` starts the local server using the configured launch profile.
- `dotnet watch run --project "Student Management System.csproj"` runs with automatic reload during development.
- `dotnet test` runs all discovered test projects. No test project is currently included; add one before relying on this command for validation.

Use `Student Management System.http` for repeatable local endpoint requests. Configure required connection strings and Supabase/JWT values through user secrets or environment variables, not committed JSON files.

## Coding Style & Naming Conventions

Use four-space indentation and standard C# conventions: PascalCase for public types and members, camelCase for parameters and locals, and `_camelCase` for private fields. Keep file-scoped namespaces and nullable annotations. Name interfaces with an `I` prefix (`IStudentService`) and implementations without it (`StudentService`). Keep controllers thin, put business rules in services, and isolate database access in repositories. Run `dotnet format` before submitting broad formatting changes.

## Testing Guidelines

Create a sibling xUnit test project (for example, `StudentManagementSystem.Tests/`) and name files after the subject, such as `StudentServiceTests.cs`. Use method names that state behavior and conditions, such as `CreateAsync_WhenEmailExists_Throws`. Prioritize service rules, authorization paths, repository queries, and controller status codes. Run `dotnet test` before opening a pull request.

## Commit & Pull Request Guidelines

Git history is not available from this project directory, so use short, imperative commit subjects such as `Add attendance validation`. Keep each commit focused. Pull requests should explain the behavior change, list configuration or schema impacts, link the relevant issue, and include request/response examples for API changes. Confirm builds and tests pass; include screenshots only when rendered UI or API documentation changes.
