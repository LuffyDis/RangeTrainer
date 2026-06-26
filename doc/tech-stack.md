# RangeTrainer — Technical Stack

> Adapted from the PokerTrainer BMAD architecture artifact.
> Authentication is **standard** (ASP.NET Identity + JWT, login + registration only).
> No anonymous/trial account path.

## Summary

RangeTrainer is a **.NET 10 / C# 14 modular monolith**: a Blazor WebAssembly PWA
frontend served by an ASP.NET Core host, orchestrated locally with .NET Aspire, backed
by PostgreSQL via EF Core, with Wolverine for in-process messaging (mediator + outbox).

| Layer | Choice |
|-------|--------|
| **Language & Runtime** | C# 14 / .NET 10 (LTS) |
| **Frontend** | Blazor WebAssembly (standalone PWA) + MudBlazor (Material Design UI) |
| **Backend host** | ASP.NET Core Minimal API (BFF that also serves the WASM static files) |
| **Orchestration** | .NET Aspire AppHost + ServiceDefaults |
| **Messaging / CQRS** | Wolverine 5.19.1 (mediator + transactional outbox + auto-transactions) |
| **ORM** | EF Core 10 with Npgsql (PostgreSQL) |
| **Database** | PostgreSQL (Neon managed in prod), per-module schema isolation |
| **Auth** | ASP.NET Identity + JWT (HTTPS only) |
| **Validation** | FluentValidation (via WolverineFx.FluentValidation) |
| **API docs** | Scalar (OpenAPI 3.1) |
| **Spaced repetition** | FSRS.Core (server-authoritative + client cache for offline) |
| **Offline storage** | IndexedDB (full library cache) + service worker |
| **Testing** | xUnit, FluentAssertions, NSubstitute, Respawn, Aspire test infra |
| **CI/CD** | GitHub Actions |
| **Container** | Single Dockerfile (API serves WASM) |
| **Hosting** | Railway or Azure Container Apps (scale-to-zero) |

## Packages (Central Package Management)

All versions are pinned centrally in `Directory.Packages.props` at the repo root
(`ManagePackageVersionsCentrally = true`); `.csproj` files reference packages without
version numbers.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- Aspire -->
    <PackageVersion Include="Aspire.Hosting" Version="9.*" />
    <PackageVersion Include="Aspire.Hosting.PostgreSQL" Version="9.*" />
    <PackageVersion Include="Aspire.Hosting.Testing" Version="9.*" />

    <!-- Wolverine -->
    <PackageVersion Include="WolverineFx" Version="5.19.1" />
    <PackageVersion Include="WolverineFx.Http" Version="5.19.1" />
    <PackageVersion Include="WolverineFx.EntityFrameworkCore" Version="5.19.1" />
    <PackageVersion Include="WolverineFx.FluentValidation" Version="5.19.1" />

    <!-- EF Core + PostgreSQL -->
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />

    <!-- Authentication -->
    <PackageVersion Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />

    <!-- Validation -->
    <PackageVersion Include="FluentValidation" Version="12.*" />

    <!-- API Documentation -->
    <PackageVersion Include="Scalar.AspNetCore" Version="2.*" />

    <!-- Spaced Repetition -->
    <PackageVersion Include="FSRS.Core" Version="1.*" />

    <!-- UI (Blazor) -->
    <!-- MudBlazor — add the current stable version when scaffolding -->

    <!-- Testing -->
    <PackageVersion Include="xunit" Version="2.*" />
    <PackageVersion Include="FluentAssertions" Version="7.*" />
    <PackageVersion Include="Respawn" Version="6.*" />
    <PackageVersion Include="NSubstitute" Version="5.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
  </ItemGroup>
</Project>
```

> **Note:** versions are inherited from the PokerTrainer baseline (early 2026). Verify the
> latest stable releases when scaffolding RangeTrainer.

## Why these choices

| Constraint / driver | Decision |
|---------------------|----------|
| .NET 10 / C# 14 required | Determines all tooling, packages, patterns |
| PWA + offline | Blazor WASM (client-side, offline-capable, single language) |
| Tiny dataset, single-user first | Full library cached in IndexedDB → no read-sync complexity |
| Spaced repetition must work offline | FSRS.Core runs **both** server-side (authoritative) and client-side (cached) |
| Reliable cross-module events | Wolverine transactional outbox in the same DB transaction |
| Simple solo build/test/deploy | Aspire (one F5 to run everything), single container, scale-to-zero hosting |
| Schema isolation per module | PostgreSQL schemas `[identity]`, `[range]`, `[study]` via per-module EF Core contexts |

## Data Architecture

| Concern | Choice |
|---------|--------|
| **Database** | PostgreSQL (Neon: scale-to-zero, JSON support, DB branching for migration testing) |
| **ORM** | EF Core 10, per-module **Write + Read** DbContexts (CQRS split: Write = change tracking/repos/migrations; Read = NoTracking projections) |
| **Offline storage** | IndexedDB — full library cached (range data is tiny, ~100KB / 100 situations) |
| **FSRS location** | Dual: server stores authoritative mastery; client caches FSRS card data for offline drills; on reconnect client pushes results, server recalculates |
| **Sync strategy** | Client queues offline drill answers; server processes sequentially on reconnect (single-user → no merge conflicts) |
| **Validation** | Three layers — value objects (construction) + FluentValidation (input shape) + CheckInvariants (aggregate consistency) |
| **Migrations** | Per-module EF Core migrations; auto-apply in dev, idempotent scripts in prod |

## Authentication & Security

> **Changed from source:** standard auth only — no anonymous/trial account, no upgrade flow.

| Concern | Choice |
|---------|--------|
| **Authentication** | ASP.NET Identity + JWT (HTTPS only) |
| **Account model** | Standard registration (email + password) and login. No anonymous accounts. |
| **Authorization** | Per-user data isolation — all endpoints filter by authenticated UserId. Roles enabled (`AddRoles`) but not enforced until a later phase. |
| **Anonymous endpoints** | Only `POST /api/auth/register` and `POST /api/auth/login`. Everything else requires a valid JWT. |
| **Password storage** | ASP.NET Identity defaults (bcrypt/PBKDF2) — never plaintext |

## API & Communication

| Concern | Choice |
|---------|--------|
| **API style** | REST via Minimal API route groups |
| **Endpoint wiring** | Minimal API + Wolverine `IMessageBus` (endpoints and handlers kept separate) |
| **Error responses** | `Result` → ProblemDetails mapping (400/404/409/422) |
| **API documentation** | Scalar (OpenAPI 3.1), available at `/scalar/v1` |
| **Cross-module sync reads** | Integration queries via `IMessageBus.InvokeAsync` (contracts in SharedKernel) |
| **Cross-module async events** | Wolverine cascading messages + transactional outbox |

## Frontend

| Concern | Choice |
|---------|--------|
| **Framework** | Blazor WebAssembly standalone PWA |
| **UI kit** | MudBlazor (Material Design); dark mode theme; Plus Jakarta Sans (body) + JetBrains Mono (data/combos) |
| **State management** | Scoped services + cascading for auth only (no Fluxor) |
| **Component pattern** | ViewModel pattern — pure C# ViewModels, testable with xUnit (no bUnit needed for logic) |
| **Grid rendering** | SVG in `RangeGrid.razor` (scales cleanly, color overlay, accessible) |
| **Offline** | Service worker (from PWA template) + IndexedDB full-library cache |

## Infrastructure & Deployment

| Concern | Choice |
|---------|--------|
| **Local dev** | .NET Aspire AppHost (PostgreSQL + API + Client), built-in observability dashboard |
| **CI/CD** | GitHub Actions (build + test on push) |
| **Container** | Single Dockerfile — API serves WASM static files |
| **Hosting** | Railway or Azure Container Apps (scale-to-zero) |
| **Database hosting** | Neon (managed PostgreSQL, free tier, DB branching) |
| **Monitoring** | Aspire dashboard (dev) + cloud provider metrics (prod) |

## Performance Budgets (from NFRs)

| Budget | Target |
|--------|--------|
| Grid paint interaction | < 16ms (60fps) |
| Drill question presentation | < 200ms |
| Situation creation API | < 500ms |
| PWA cached page load | < 1s |
| First visit (4G) | < 3s |
| WASM bundle | < 5MB compressed |
