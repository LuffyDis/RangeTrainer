# RangeTrainer — Architecture

> Adapted from the PokerTrainer BMAD architecture artifact.
> **Changes vs. source:** project renamed `PokerTrainer` → `RangeTrainer`; authentication
> is **standard** (ASP.NET Identity + JWT, register + login only) — the anonymous/trial
> account model and the "upgrade anonymous account" flow are removed. All endpoints
> require authentication except register/login.

---

## 1. Context

- **Primary domain:** Full-stack web — Blazor WASM PWA + ASP.NET Core modular monolith.
- **Complexity:** Medium (non-trivial domain logic, no regulatory constraints).
- **Components:** SharedKernel + 3 backend modules (Identity, RangeContext, Study) + Blazor client + Aspire AppHost.
- **Users at launch:** 1 (personal tool), scaling to a small community if validated.

### Technical Constraints

| Constraint | Impact |
|-----------|--------|
| .NET 10 / C# 14 | Determines all tooling, packages, patterns |
| Blazor WebAssembly | Client-side rendering only; WASM bundle-size constraint |
| PostgreSQL | EF Core 10 with schema isolation per module |
| Wolverine | Convention-based handlers, transactional outbox, auto-SaveChanges |
| FSRS.Core | Must also run client-side for offline drilling |
| Single developer | Architecture must be simple to build, test, and deploy solo |

### Cross-Cutting Concerns

1. **Offline data strategy** — full library cached in IndexedDB (reads); offline drill results queued and synced.
2. **Cross-module communication** — RangeContext → Study via events (RangeCreatedEvent) and integration queries, all over Wolverine.
3. **FSRS scheduling location** — server authoritative + client cache for offline.
4. **SVG grid as shared component** — used interactively (range building) and read-only (drill feedback).

---

## 2. Starter Template & Scaffold

**Selected approach:** `aspire` (empty Aspire) + manual module setup. (`aspire-starter`
generates Blazor *Server*, not WASM PWA; community modulith templates conflict with the
Wolverine patterns below.)

> **Scaffold is incremental (see `sdlc.md §7`).** The command list below is the *target*
> full structure. It is **not** created up front: Story 0.1 stood up only the **spine**
> — `AppHost`, `ServiceDefaults`, `Host`, `Client`, `SharedKernel`, one test project,
> `Directory.Packages.props` (CPM) and a `.slnx` solution. The **3 modules** and the
> **other test projects** are born with the feature story that first needs them.
>
> **Reality corrections (verified at scaffold time, .NET 10 / Aspire 13.x):**
> - Template short names: `aspire-empty` → **`aspire`**, `aspire-test-xunit` → **`aspire-xunit`**.
> - The Aspire templates ship with the SDK (no workload install) but target `net9.0` /
>   Aspire `9.3.1`; `AppHost` + `ServiceDefaults` are retargeted to `net10.0` + Aspire `13.4.6`.
> - The repo already *is* `RangeTrainer/`, so projects are created in place under `src/` —
>   no subfolder + `mv` step.
> - Authoritative pinned package versions live in the repo's **`Directory.Packages.props`**,
>   not in the illustrative list in `tech-stack.md`.

```bash
# 1. Create solution with Aspire (generates AppHost + ServiceDefaults)
dotnet new aspire --name RangeTrainer --output RangeTrainer
cd RangeTrainer

# 2. Move AppHost and ServiceDefaults under src/
mv RangeTrainer.AppHost src/RangeTrainer.AppHost
mv RangeTrainer.ServiceDefaults src/RangeTrainer.ServiceDefaults

# 3. Convert .sln to .slnx (XML-based solution file)
dotnet sln migrate

# 4. Create the ASP.NET Core Host (BFF)
dotnet new webapi -o src/RangeTrainer.Host --no-openapi
dotnet sln add src/RangeTrainer.Host/RangeTrainer.Host.csproj

# 5. Create the Blazor WASM PWA Client
dotnet new blazorwasm --pwa -o src/RangeTrainer.Client
dotnet sln add src/RangeTrainer.Client/RangeTrainer.Client.csproj

# 6. Create Shared Kernel
dotnet new classlib -o src/RangeTrainer.SharedKernel
dotnet sln add src/RangeTrainer.SharedKernel/RangeTrainer.SharedKernel.csproj

# 7. Create Modules (Identity, RangeContext, Study)
dotnet new classlib -o src/Modules/RangeTrainer.Modules.Identity
dotnet new classlib -o src/Modules/RangeTrainer.Modules.RangeContext
dotnet new classlib -o src/Modules/RangeTrainer.Modules.Study
dotnet sln add src/Modules/RangeTrainer.Modules.Identity/RangeTrainer.Modules.Identity.csproj
dotnet sln add src/Modules/RangeTrainer.Modules.RangeContext/RangeTrainer.Modules.RangeContext.csproj
dotnet sln add src/Modules/RangeTrainer.Modules.Study/RangeTrainer.Modules.Study.csproj

# 8. Create Test Projects
dotnet new xunit -o tests/RangeTrainer.Modules.RangeContext.Tests
dotnet new xunit -o tests/RangeTrainer.Modules.Study.Tests
dotnet new aspire-xunit -o tests/RangeTrainer.Tests.Shared
dotnet new xunit -o tests/RangeTrainer.SystemTests
dotnet new xunit -o tests/RangeTrainer.Client.Tests
dotnet sln add tests/RangeTrainer.Modules.RangeContext.Tests/RangeTrainer.Modules.RangeContext.Tests.csproj
dotnet sln add tests/RangeTrainer.Modules.Study.Tests/RangeTrainer.Modules.Study.Tests.csproj
dotnet sln add tests/RangeTrainer.Tests.Shared/RangeTrainer.Tests.Shared.csproj
dotnet sln add tests/RangeTrainer.SystemTests/RangeTrainer.SystemTests.csproj
dotnet sln add tests/RangeTrainer.Client.Tests/RangeTrainer.Client.Tests.csproj

# 9. Wire Aspire AppHost
dotnet add src/RangeTrainer.AppHost reference src/RangeTrainer.Host/RangeTrainer.Host.csproj
dotnet add src/RangeTrainer.AppHost reference src/RangeTrainer.Client/RangeTrainer.Client.csproj

# 10. Enable Central Package Management
# Create Directory.Packages.props at root (see tech-stack.md), then reference packages
# without version numbers in .csproj files.
```

> Package list and `Directory.Packages.props` are in **`tech-stack.md`**.

---

## 3. Core Architectural Decisions

**Already decided (from technical research):**
- PostgreSQL + EF Core 10 with per-module schemas
- Wolverine 5.19.1 (mediator + outbox + auto-transactions)
- ASP.NET Identity + JWT (standard register/login)
- REST via Minimal API + Wolverine `IMessageBus`
- Result pattern + typed errors → ProblemDetails
- Hybrid VSA/Clean per module
- CQRS (commands via domain entities, queries via DbContext)
- Always-Valid Aggregate pattern
- .NET Aspire for dev + testing

**Deferred (post-MVP):** push notifications, sharing/permissions (coach/student), freemium paywall.

### Data Architecture

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Database** | PostgreSQL via Neon | Scale-to-zero, JSON support, schema isolation |
| **ORM** | EF Core 10, per-module Write + Read DbContexts | CQRS split: Write (tracking, repos, migrations) + Read (NoTracking projections) |
| **Offline storage** | IndexedDB — full library cached | Range data is tiny; full offline reads eliminate read-sync complexity |
| **FSRS location** | Dual: server authoritative + client cache | Client caches FSRS cards for offline; on reconnect, server recalculates from pushed results |
| **Sync strategy** | Client pushes queued drill results on reconnect; server processes sequentially | Single-user → no merge conflicts |
| **Validation** | Value objects + FluentValidation + CheckInvariants | Invalid state unrepresentable; input shape checked; aggregate consistency guaranteed |
| **Migrations** | Per-module EF Core migrations; auto-apply dev, idempotent scripts prod | Each module owns its migration history |

### Authentication & Security

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Authentication** | ASP.NET Identity + JWT (HTTPS only) | Standard, well-tested, built into .NET |
| **Account model** | Standard register + login (email/password). **No anonymous/trial account.** | Simpler than the dual anonymous→upgrade model; the rest of the app already assumes an authenticated, data-scoped user |
| **Authorization** | Per-user data isolation; roles prepared (`AddRoles`) but not enforced until a later phase | All endpoints filter by authenticated UserId |
| **Anonymous endpoints** | Only `POST /api/auth/register` and `POST /api/auth/login` | Everything else requires a valid JWT (NFR10) |
| **Password storage** | ASP.NET Identity defaults (bcrypt/PBKDF2) | Never store plaintext |

### API & Communication

| Decision | Choice |
|----------|--------|
| **API style** | REST via Minimal API route groups |
| **Endpoint wiring** | Minimal API + `IMessageBus` (endpoints and handlers separate) |
| **Error responses** | `Result` → ProblemDetails via ResultExtensions (400/404/409/422) |
| **API documentation** | Scalar (OpenAPI 3.1) |
| **Cross-module sync reads** | Integration queries via `IMessageBus.InvokeAsync`. Consumer owns interface + implementation; producer handles query in `IntegrationHandlers/`; contracts in SharedKernel. |
| **Cross-module async events** | Wolverine cascading messages + transactional outbox |
| **Event isolation** | `MultipleHandlerBehavior.Separated` |
| **Microservice extraction** | `IntegrationHandlers/` become future HTTP endpoints; consumer swaps Wolverine call for HTTP. Zero handler code changes. |

### Frontend

| Decision | Choice |
|----------|--------|
| **Framework** | Blazor WebAssembly standalone PWA |
| **UI kit** | MudBlazor (Material Design), dark theme |
| **State management** | Scoped services + cascading for auth only |
| **Component pattern** | ViewModel pattern (pure C#, xUnit-testable) |
| **Grid rendering** | SVG in `RangeGrid.razor` |
| **Offline** | Service worker + IndexedDB full-library cache |

### Infrastructure & Deployment

| Decision | Choice |
|----------|--------|
| **Local dev** | .NET Aspire AppHost (PostgreSQL + API + Client) |
| **CI/CD** | GitHub Actions |
| **Hosting** | Railway or Azure Container Apps (scale-to-zero) |
| **Container** | Single Dockerfile (API serves WASM static files) |
| **Database hosting** | Neon (managed PostgreSQL) |
| **Monitoring** | Aspire dashboard (dev) + cloud metrics (prod) |

---

## 4. Implementation Sequence

1. **Project scaffold** (Aspire + modules + Blazor WASM PWA) — section 2
2. **SharedKernel** (Result, Error, AggregateRoot, Entity, IDomainEvent, value objects)
3. **Identity module** (ASP.NET Identity + JWT + register/login) — *no trial mode*
4. **RangeContext module** (Situation + Range domain, handlers, endpoints)
5. **Blazor Client** (RangeGrid component, ViewModel pattern, API clients, auth state)
6. **Study module** (FSRS integration, drill modes, question selection)
7. **Offline capability** (IndexedDB, service worker, sync)

**Cross-component dependencies:**
- Study depends on RangeContext via integration queries (Wolverine in-process; future HTTP).
- Study owns `IRangeQueryService`, implements it internally with `IMessageBus`.
- RangeContext handles integration queries in `IntegrationHandlers/` (= future REST endpoints).
- SharedKernel holds integration query/response contracts (`IntegrationQuery`/`IntegrationResponse` suffix).
- FSRS.Core needed server-side (Study) and client-side (Blazor WASM for offline).

---

## 5. Naming Conventions

### Database (PostgreSQL + EF Core)

| Element | Convention | Example |
|---------|-----------|---------|
| Schema names | lowercase, module name | `range`, `study`, `identity` |
| Write DbContext | `{Module}WriteDbContext` | `RangeWriteDbContext` |
| Read DbContext | `{Module}ReadDbContext` | `RangeReadDbContext` |
| Table names | PascalCase, plural | `Situations`, `Ranges`, `Combos` |
| Column names | PascalCase | `HeroPosition`, `CreatedAt` |
| Foreign keys | `{Entity}Id` (no prefix) | `SituationId`, `RangeId` |
| Indexes | `IX_{Table}_{Column}` | `IX_Situations_UserId` |
| Cross-schema refs | Simple ID, no DB-level FK | `SituationId` in `[study].MasteryCards` |

### API

| Element | Convention | Example |
|---------|-----------|---------|
| Base path | `/api/{resource}` lowercase plural | `/api/situations` |
| Nested resources | `/api/{parent}/{id}/{child}` | `/api/situations/{id}/ranges` |
| Route parameters | `{id:guid}` with type constraint | `/api/situations/{id:guid}` |
| Query parameters | camelCase | `?heroPosition=CO&page=1` |
| HTTP methods | Standard REST | GET / POST / PUT / DELETE |

### C# Code

| Element | Convention | Example |
|---------|-----------|---------|
| Namespaces | `RangeTrainer.Modules.{Module}.{Folder}` | `RangeTrainer.Modules.RangeContext.Features.Commands` |
| Classes | PascalCase | `CreateSituationHandler` |
| Interfaces | `I` + PascalCase | `ISituationRepository`, `IRangeQueryService` |
| Methods | PascalCase | `Handle`, `AddRange`, `CheckInvariants` |
| Private fields | `_camelCase` | `_ranges` |
| Constants | PascalCase | `MaxRangesPerSituation` |
| Records | PascalCase | `CreateSituation`, `RangeCreatedEvent` |
| Value objects | PascalCase `readonly record struct` | `SituationId`, `ComboWeight` |

### Blazor Components

| Element | Convention | Example |
|---------|-----------|---------|
| Pages | `{Feature}Page.razor` | `DrillPage.razor` |
| ViewModels | `{Feature}ViewModel.cs` | `DrillViewModel.cs` |
| Shared components | `{Name}.razor` | `RangeGrid.razor`, `ActionBar.razor` |
| CSS isolation | `{Component}.razor.css` | `RangeGrid.razor.css` |
| Services | `{Name}Service.cs` / `{Name}ApiClient.cs` | `DrillApiClient.cs` |

### Wolverine Messages

| Type | Convention | Example |
|------|-----------|---------|
| Commands | PascalCase verb+noun | `CreateSituation`, `CloneRange` |
| Queries | PascalCase verb+noun | `GetSituation`, `FilterSituations` |
| Events | PascalCase past-tense+noun | `RangeCreatedEvent`, `SituationDeletedEvent` |
| Integration queries | …`IntegrationQuery` | `GetRangeForDrillIntegrationQuery` |
| Integration responses | …`IntegrationResponse` | `RangeForDrillIntegrationResponse` |
| Events location | `SharedKernel/Events/` | |
| Integration query contracts | `SharedKernel/IntegrationQueries/` | |
| Integration handlers | `Module/Features/IntegrationHandlers/` | |

---

## 6. Structure Patterns

### Backend module — Producer (RangeContext, Hybrid VSA)

```
RangeTrainer.Modules.RangeContext/
├── Domain/
│   ├── Entities/              # Situation.cs, Range.cs, Combo.cs, Tag.cs
│   ├── ValueObjects/          # SituationId, ComboWeight, HandNotation, ActionSequence, RaiseSizing
│   ├── DomainServices/        # ConflictDetectionService.cs
│   ├── Interfaces/            # ISituationRepository.cs (internal to module)
│   └── Errors/                # DomainErrors.cs
├── Features/
│   ├── Commands/
│   │   └── CreateSituation/
│   │       ├── CreateSituation.cs          # Message (plain record)
│   │       ├── CreateSituationHandler.cs   # Static handler
│   │       └── CreateSituationValidator.cs # FluentValidation
│   ├── Queries/
│   │   └── GetSituation/{GetSituation,GetSituationHandler,GetSituationResponse}.cs
│   └── IntegrationHandlers/                # Cross-module queries (= future endpoints)
│       └── GetRangeForDrillIntegrationQueryHandler.cs
├── Infrastructure/
│   ├── Persistence/{RangeWriteDbContext,RangeReadDbContext}.cs + Configurations/
│   ├── Repositories/                       # Use WriteDbContext only
│   ├── Services/                           # RangeTextParser.cs
│   └── Migrations/                         # Owned by WriteDbContext only
└── RangeContextModule.cs
```

### Backend module — Consumer (Study, Hybrid VSA)

```
RangeTrainer.Modules.Study/
├── Domain/
│   ├── Entities/             # DrillSession.cs, MasteryCard.cs
│   ├── ValueObjects/         # DrillSessionId, MasteryScore, DrillMode
│   └── Interfaces/
│       └── IRangeQueryService.cs          # Study OWNS this interface
├── Features/
│   ├── Commands/             # StartSession, SubmitAnswer, EndSession
│   └── Queries/              # GetNextQuestion, GetSessionSummary, GetMasteryScores
│       # (no IntegrationHandlers — Study is consumer, not producer)
├── Infrastructure/
│   ├── Persistence/{StudyWriteDbContext,StudyReadDbContext}.cs + Configurations/
│   ├── Repositories/
│   ├── Services/             # RangeQueryService (IMessageBus), QuestionSelector, FsrsScheduler
│   ├── EventHandlers/        # WhenRangeCreatedHandler.cs
│   └── Migrations/
└── StudyModule.cs
```

### Backend module — Pure VSA (Identity)

```
RangeTrainer.Modules.Identity/
├── Features/
│   ├── Commands/             # Register/, Login/
│   └── Queries/              # GetGameProfile/
├── Data/{IdentityWriteDbContext,IdentityReadDbContext}.cs + Configurations/ + Migrations/
├── Entities/                 # User.cs, GameProfile.cs
└── IdentityModule.cs
```

### Frontend

```
RangeTrainer.Client/
├── Components/Shared/         # RangeGrid, ActionBar, ComboWeightInput, ConflictDot, SessionSummary
├── Features/
│   ├── Situations/           # Pages + ViewModels + SituationCard
│   ├── Ranges/               # RangeEditorPage, RangeImportDialog (+ ViewModels)
│   ├── Drill/                # DrillPage + 4 drill modes (+ ViewModels)
│   └── Account/              # LoginPage, RegisterPage, GameProfilePage (+ ViewModels)
├── Services/
│   ├── ApiClients/           # Situation, Range, Drill, Auth
│   ├── OfflineStorage/       # IndexedDbService, SyncService
│   └── AuthStateProvider.cs
├── Layout/                   # MainLayout, NavMenu
└── wwwroot/                  # index.html, manifest.webmanifest, service-worker.js, css/, icons/
```

> Note vs. source: the client feature folder is **`Account/`** (Login + Register + GameProfile)
> rather than `Profile/`, reflecting standard auth.

### Tests

```
tests/
├── RangeTrainer.Modules.RangeContext.Tests/   # Domain (unit) + Features (integration) + Builders
├── RangeTrainer.Modules.Study.Tests/          # Domain + Features + Builders
├── RangeTrainer.Client.Tests/                 # ViewModels
├── RangeTrainer.Tests.Shared/                 # AppHostFixture, AspireIntegrationTest, AspireCollection
└── RangeTrainer.SystemTests/                  # Cross-module tests
```

---

## 7. Format & Process Patterns

### API Response Formats

| Scenario | Format | Status |
|----------|--------|--------|
| Success (create) | `{ "value": "guid" }` | 201 |
| Success (read) | Direct DTO (no wrapper) | 200 |
| Success (list) | `{ "items": [...], "totalCount": N, "page": N, "pageSize": N }` | 200 |
| Validation error | ProblemDetails `{ status, title, errors[] }` | 400 |
| Not found | ProblemDetails | 404 |
| Domain failure | ProblemDetails `{ status, title, errors[] }` | 422 |
| Conflict | ProblemDetails | 409 |

- **JSON:** camelCase property names (System.Text.Json default); dates as ISO 8601.
- **Error codes:** `{Entity}.{ErrorName}` — e.g., `Situation.MaxRangesReached`, `ComboWeight.OutOfRange`.

### Error Handling Flow

```
FluentValidation (input shape)     → 400 Bad Request
Value Object construction (value)  → 400 Bad Request
Domain entity + CheckInvariants()  → 422 Unprocessable Entity
Not found                          → 404 Not Found
Conflict (duplicate)               → 409 Conflict
Unexpected exception               → 500 (global middleware)
```

**Rule:** never throw exceptions for expected business failures — always return `Result<T>`.

### Wolverine Handler Convention

```csharp
// ALWAYS static class + static Handle method
// ALWAYS return tuple (Response, Event?) for commands; DTO directly for queries
public static class CreateSituationHandler
{
    public static (Result<SituationId>, SituationCreatedEvent?) Handle(
        CreateSituation command,
        ISituationRepository repository) { ... }
}
```

### ViewModel Convention

```csharp
// Inject deps via primary constructor; expose state as public properties;
// notify the view via StateChanged; async for commands, sync for state reads.
public class DrillViewModel(IDrillApiClient drillApi)
{
    public DrillQuestion? CurrentQuestion { get; private set; }
    public event Action? StateChanged;
    public async Task StartSessionAsync(...) { /* ... */ StateChanged?.Invoke(); }
}
```

---

## 8. Boundaries

### Module communication

| Direction | Mechanism | Contract Location |
|-----------|-----------|-------------------|
| Study → RangeContext (sync read) | `IMessageBus.InvokeAsync` → IntegrationHandler | `SharedKernel/IntegrationQueries/` |
| RangeContext → Study (async event) | Wolverine cascading message → EventHandler | `SharedKernel/Events/` |
| Any module → own DB | Direct DbContext | `Module/Infrastructure/Persistence/` |
| Client → any module | HTTP REST | Module endpoints via Host |

### Data boundaries

| Schema | Owner | Accessed By |
|--------|-------|-------------|
| `[identity]` | Identity module | Identity only |
| `[range]` | RangeContext module | RangeContext directly; Study via integration queries |
| `[study]` | Study module | Study only (stores SituationId/RangeId as GUIDs) |

### Microservice extraction guide

| When splitting… | What changes | What stays |
|-----------------|-------------|------------|
| RangeContext becomes a service | `IntegrationHandlers/` → REST endpoints | Handler code unchanged |
| Study connects to remote RangeContext | `RangeQueryService` swaps `IMessageBus` for `HttpClient` | `IRangeQueryService` + handler code unchanged |
| Events go cross-service | Wolverine transport config → RabbitMQ | Event contracts + handlers unchanged |

### Requirements → structure mapping

| FR Category | Backend | Frontend |
|------------|---------|----------|
| FR1–FR8 (Situations) | RangeContext/Features/Commands + Domain/Entities/Situation.cs | Features/Situations/ |
| FR9–FR18 (Ranges) | RangeContext/Features/Commands + Domain/Entities/Range.cs | Features/Ranges/ + Components/Shared/RangeGrid.razor |
| FR19–FR25 (Import/Export) | RangeContext/Infrastructure/Services/RangeTextParser.cs | Features/Ranges/RangeImportDialog.razor |
| FR26–FR35 (Drilling) | Study/Features/ | Features/Drill/ (4 drill ViewModels) |
| FR36–FR40 (Mastery) | Study/Infrastructure/Services/FsrsScheduler.cs | Features/Drill/DrillViewModel.cs |
| FR41, FR43 (Identity) | Identity/Features/ | Features/Account/ |
| FR44–FR47 (Offline) | N/A (client-side) | Services/OfflineStorage/ |

---

## 9. Enforcement Rules (for all AI agents / contributors)

1. Follow naming conventions exactly — no "consistency later" exceptions.
2. Place files in the correct folder per the structure patterns.
3. Use static Wolverine handlers with tuple returns for commands.
4. Use the ViewModel pattern for all pages and complex components.
5. Return `Result<T>` from domain methods — never throw for business rules.
6. Use `sealed record` for messages, DTOs, events, and value objects.
7. Use `readonly record struct` for strongly-typed IDs.
8. Use primary constructors for handlers, ViewModels, and services.
9. Write FluentValidation validators for all commands.
10. Include `CancellationToken` in all async handler methods.
11. Place cross-module query contracts in `SharedKernel/IntegrationQueries/` with the `IntegrationQuery`/`IntegrationResponse` suffix.
12. Place integration-query handlers in the module's `Features/IntegrationHandlers/` folder.
13. Consumer modules own their interfaces and implement them internally using `IMessageBus`.
14. Never reference another module's internal types — only SharedKernel contracts.
15. All endpoints require authentication except `register` and `login`; always filter data by authenticated UserId.

---

## 10. Open Items

1. Entity-relationship diagram — generate from the EF Core model once implemented.
2. IndexedDB library choice — decide during the first offline story.
3. FSRS.Core WASM compatibility — verify early that it runs in Blazor WASM.
