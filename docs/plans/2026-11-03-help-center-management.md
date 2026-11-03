# Help Center Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Admins manage help-center categories and articles (FAQ-style question + plain-text answer, ordered, active/inactive); learners and instructors — and any anonymous visitor — can read the published content, browse it by category and search it.

**Architecture:** Clean Architecture with MediatR CQRS handlers, FluentValidation validators (every request rule, including DB uniqueness and FK existence, lives there), EF Core `IEntityTypeConfiguration` auto-discovery, `BaseEntity<Guid>` audit + soft delete, versioned ASP.NET Core controllers returning `Result<T>` → `ApiResponse<T>` through `BaseController.FromResult`.

**Tech Stack:** .NET 8 (net8.0), ASP.NET Core, MediatR, FluentValidation, EF Core 8 + SQL Server, JSON-based localization.

---

## Analysis: what already exists

**Nothing help-center-shaped exists.** No entity, DbSet, table, migration, controller, route, feature folder, enum, localization key or seeder matches Help / Article / Category / Faq / Support / Ticket / Contact / Knowledge / Topic / Tutorial / Announcement / Notification. Full domain surface today (11 entities): ApplicationUser, Country, Instructor, InstructorApplication, InstructorAvailabilitySlot, InstructorCertificate, Learner, OnboardingAnswer, OnboardingOption, OnboardingQuestion, UserRefreshToken.

Four things from the repo are directly reusable as precedent:

1. **The UI shape is already designed.** `LMS.API/wwwroot/pages/pages.css` L284–L463 styles a FAQ accordion (`#faq-list`, `.faq-item`, `.faq-question`, `.faq-answer`, `.faq-item.open`, plus RTL variants at L463) with no markup or endpoint behind it. So: a flat list of question + collapsible answer, grouped by category, is what the front end expects.
2. **Admin-managed parent/child content** = `OnboardingQuestion` + `OnboardingOption` with `Key`/`Title`/`Type`/`Order`, `admin/questions/{id}/options` routes, and `OnboardingAdminController` (7 endpoints). This is the closest structural template for `HelpCategory` → `HelpArticle`.
3. **Simple admin-managed list** = `Country` + `CountriesController`: class-level `[Authorize][RoleAuthorize(UserType.Admin)]` with **per-action `[AllowAnonymous]`** on the read actions (L30, L64), and a `CannotDeleteCountryInUse` guard key — exactly the behaviour wanted for deleting a category that still has articles.
4. **Search convention** = `GetUsersPagginatedQuery.Search` + EF-translated `Contains` + `SearchTooLong` key (used in only one place today; `EF.Functions.Like` appears nowhere).

Also verified: `[RoleAuthorize(UserType.Learner, UserType.Instructor)]` is a genuine OR — `RoleAuthorizeAttribute` loops the roles and matches either `user.IsInRole(...)` (the JWT carries one `role` claim per Identity role) or the `UserTypes` bitmask from `JwtTokenService.GenerateAccessToken`. **Not needed for this feature** (reads are anonymous) but worth knowing: with `UserType { Learner = 0, Instructor = 1, Admin = 2 }` and no `[Flags]`, the mask is `0` for a pure learner, `1` for instructor, `2` for admin — so an `Admin` token would get 403 from a Learner+Instructor gate. Anonymous reads avoid that whole trap.

## Scope decisions (confirmed 2026-11-03)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Structure | **Two tables**: `HelpCategory` + `HelpArticle` |
| 2 | Read access | **`[AllowAnonymous]`** on every public read (same trick as `CountriesController`); learners and instructors read it without any special handling, and the login/signup page can too |
| 3 | Audience targeting | **None.** Every published article is visible to everyone; admins control exposure with `IsActive` and delete |
| 4 | Answer format | **Plain text**, newlines preserved, rendered by the front end. No HTML storage (no sanitizer dependency, no stored-XSS surface, and the CSP `default-src 'self'` would block foreign markup assets anyway) |
| 5 | Slug / key | **No `Key`/`Slug` column** — `OnboardingQuestion.Key` exists only because answers reference questions by key. Front end uses `Id`. Add a slug later only if URLs must be human-readable |
| 6 | Names | **Single English field** (`Name`, `Question`, `Answer`) — the platform is English-only, matching `Country.CountryEnName` and `OnboardingQuestion.Title` (no En/Ar pairs) |
| 7 | Images / attachments | **Out of scope.** The accordion CSS has no media slots; the existing `AttachmentsController` upload is available later if articles need screenshots |

## Data model

```
HelpCategory 1 ──< HelpArticle
```

`HelpCategory : BaseEntity<Guid>`
- `Name` string, required, max 150, unique (trimmed, case-insensitive)
- `Order` int (list position; ties broken by `CreatedAt`)
- `virtual ICollection<HelpArticle> Articles`
- `IsActive` / `IsDeleted` / audit columns come from `BaseEntity` — do not redeclare

`HelpArticle : BaseEntity<Guid>`
- `CategoryId` Guid, required FK → `HelpCategory` (cascade, matching `InstructorAvailabilitySlot`)
- `Question` string, required, max 300
- `Answer` string, required, `nvarchar(max)`, validator caps length at 20000
- `Order` int
- `virtual HelpCategory Category`

Both configurations add `HasQueryFilter(x => !x.IsDeleted)` plus indexes on `IsDeleted`, `CategoryId`, `(CategoryId, Order)`, and a unique index on `HelpCategory.Name`.

Publishing rule for every public read: `article.IsActive && article.Category.IsActive`. Admin reads ignore `IsActive` (they must see hidden rows), mirroring `GetOnboardingQuestionsQuery` vs `GetActiveOnboardingQuestionsQuery`.

## Endpoints

Public — `HelpCenterController`, `[Route(ApiRoutes.HelpCenter.Base)]`, `[AllowAnonymous]` per action:

| Method | Route | Returns |
|--------|-------|---------|
| GET | `categories` | active categories with `articleCount`, ordered by `Order` |
| GET | `articles?categoryId=&search=&pageNumber=&pageSize=` | `PagginatedResult<HelpArticleDto>` of published articles |
| GET | `articles/{id:guid}` | one published article (404 if missing **or inactive**) |

Admin — `HelpCenterAdminController`, same base, class-level `[Authorize][RoleAuthorize(UserType.Admin)]`, `admin/`-prefixed templates (the `Onboarding` route convention):

| Method | Route | Behaviour |
|--------|-------|-----------|
| GET | `admin/categories` | every category, inactive included, with counts |
| POST | `admin/categories` | create (201) |
| PUT | `admin/categories/{id:guid}` | update name/order/`IsActive` via `SetActiveState` |
| DELETE | `admin/categories/{id:guid}` | soft delete; **400 `CannotDeleteCategoryInUse`** while non-deleted articles exist |
| GET | `admin/articles?categoryId=&search=&isActive=&pageNumber=&pageSize=` | `PagginatedResult<HelpArticleDto>` including hidden rows |
| POST | `admin/articles` | create (201) |
| PUT | `admin/articles/{id:guid}` | update question/answer/order/`IsActive` |
| DELETE | `admin/articles/{id:guid}` | soft delete |

---

### Task 1: Domain entities

**Files:**
- Create: `LMS.Domain/Entities/HelpCategory.cs`
- Create: `LMS.Domain/Entities/HelpArticle.cs`

- [x] `HelpCategory` with `Name`, `Order`, `Articles` collection and a static `Create(name, order)` factory + `Update(name, order)` — same shape as `InstructorAvailabilitySlot` (private setters; no `MarkAsUpdated` call, `LmsDbContext.SaveChangesAsync` stamps Modified entities).
- [x] `HelpArticle` with `CategoryId`, `Question`, `Answer`, `Order`, `Category` navigation, `Create(categoryId, question, answer, order)` + `Update(question, answer, order)`. Changing an article's category is deliberately not supported in v1 (move = delete + create) — flag it before adding.
- [x] Verify: `dotnet build LMS.Domain/LMS.Domain.csproj`

### Task 2: EF Core mapping and DbContext surface

**Files:**
- Create: `LMS.Persistence/Configurations/HelpCategoryConfiguration.cs`
- Create: `LMS.Persistence/Configurations/HelpArticleConfiguration.cs`
- Modify: `LMS.Persistence/LmsDbContext.cs` (L17–L27 DbSet block)
- Modify: `LMS.Application/Common/Interfaces/ILmsDbContext.cs`

- [x] `HelpCategoryConfiguration`: `ToTable("HelpCategories")`, `Name` max 150 required, unique index on `Name`, `Version` rowversion, `IsDeleted` index, soft-delete query filter.
- [x] `HelpArticleConfiguration`: `ToTable("HelpArticles")`, `Question` max 300 required, `Answer` left as `nvarchar(max)`, `CategoryId` FK `WithMany(category => category.Articles)` with cascade, indexes on `CategoryId`, `(CategoryId, Order)`, `IsDeleted`, soft-delete query filter.
- [x] Add both DbSets to `LmsDbContext` **and** `ILmsDbContext` (handlers only see the interface).
- [x] Verify: `dotnet build LMS.Persistence/LMS.Persistence.csproj`

### Task 3: Localization

**Files:**
- Modify: `LMS.Application/Localization/LocalizationKeys.cs` (new `HelpCenter` class after `Availability`)
- Modify: `LMS.Application/Localization/Resources/messages.en.json` (new `"HelpCenter"` object)

- [x] Keys: `CategoryNameRequired`, `CategoryNameTooLong`, `CategoryNameAlreadyExists`, `CategoryNotFound`, `CategoryInUse`, `ArticleNotFound`, `ArticleIdRequired`, `QuestionRequired`, `QuestionTooLong`, `AnswerRequired`, `AnswerTooLong`, `OrderMustBeNonNegative`, `SearchTooLong`, `PageNumberInvalid`, `PageSizeInvalid`, `CategoryCreatedSuccess`, `CategoryUpdatedSuccess`, `CategoryDeletedSuccess`, `CategoriesRetrieved`, `ArticleCreatedSuccess`, `ArticleUpdatedSuccess`, `ArticleDeletedSuccess`, `ArticlesRetrieved`, `ArticleRetrieved`.
- [x] English values only — `messages.ar.json` does not exist and must not be created (English-only platform).
- [x] Verify JSON parses (the file has failed silently before) and build `LMS.Application`.

### Task 4: DTOs

**Files:**
- Create: `LMS.Application/Features/HelpCenter/DTOs/HelpCategoryDto.cs` — `Id`, `Name`, `Order`, `IsActive`, `ArticleCount`
- Create: `LMS.Application/Features/HelpCenter/DTOs/HelpArticleDto.cs` — `Id`, `CategoryId`, `CategoryName`, `Question`, `Answer`, `Order`, `IsActive`, `CreatedAt`, `UpdatedAt`
- Create: `LMS.Application/Features/HelpCenter/HelpCenterMapper.cs`

- [x] Mutable classes with parameterless constructors and object-initializer mapping, like `InstructorCardDto` / `OnboardingMapper`.
- [x] The public payload reuses `HelpArticleDto`; `IsActive` stays in it (harmless, avoids a second DTO) but `CreatedBy`/`UpdatedBy` must never be mapped.

### Task 5: Admin category commands

**Files:** `Features/HelpCenter/Commands/Admin/{AddHelpCategory,UpdateHelpCategory,DeleteHelpCategory}/` (command + handler + validator) + `LMS.Application/DependencyInjection.cs`

- [x] `AddHelpCategoryCommand : IRequest<Result<HelpCategoryDto>>` — `Name`, `Order`. Validator: required, max 150, `MustAsync` trimmed case-insensitive uniqueness → `CategoryNameAlreadyExists` (copy `AddOnboardingQuestionCommandValidator.BeUniqueKeyAsync`), `Order >= 0`.
- [x] `UpdateHelpCategoryCommand` — `Id` (route-bound), `Name`, `Order`, `IsActive`; uniqueness check excludes `Id`; handler calls `Update(...)` and `SetActiveState(request.IsActive, _currentUserService.UserId.ToString())`, returns 200.
- [x] `DeleteHelpCategoryCommand` — handler loads or throws `NotFoundException(HelpCenter.CategoryNotFound)`, then refuses while `category.Articles.Any()` remains (the soft-delete query filter already hides deleted articles) → `BadRequestException(LocalizationKeys.HelpCenter.CategoryInUse)`, else `MarkAsDeleted(currentUser)`.
- [x] Register all three validators in `DependencyInjection.cs` (MediatR handlers are assembly-scanned; validators are listed one by one).

### Task 6: Admin article commands

**Files:** `Features/HelpCenter/Commands/Admin/{AddHelpArticle,UpdateHelpArticle,DeleteHelpArticle}/` + `DependencyInjection.cs`

- [x] `AddHelpArticleCommand` — `CategoryId` from the body (`POST admin/articles` has no category segment), `Question`, `Answer`, `Order`. Validator: `Question` required/max 300, `Answer` required/max 20000, `Order >= 0`, and `CategoryId` must exist **as a non-deleted category** (`MustAsync` → `CategoryNotFound`) — matching how the availability validators verify the parent.
- [x] `UpdateHelpArticleCommand` — `Id`, `Question`, `Answer`, `Order`, `IsActive`; same content rules; handler throws `NotFoundException(HelpCenter.ArticleNotFound)`.
- [x] `DeleteHelpArticleCommand` — soft delete, `Result<string>.Success`.
- [x] Register validators.

### Task 7: Admin reads

**Files:** `Queries/GetHelpCategories/`, `Queries/GetHelpArticlesPagginated/` (query + handler + validator each)

- [x] `GetHelpCategoriesQuery : IRequest<Result<IReadOnlyCollection<HelpCategoryDto>>>` — every category ordered by `Order, CreatedAt`, `ArticleCount = category.Articles.Count(a => a.IsActive)` (EF translates to a subquery; the query filter already removes deleted rows).
- [x] `GetHelpArticlesPagginatedQuery` — `Guid? CategoryId`, `string? Search`, `bool? IsActive`, `PageNumber`, `PageSize` (defaults from `PagginatedResult<T>`). `Search` applied as `question.Contains(term) || answer.Contains(term)` after trim, exactly like `GetUsersPagginatedQueryHandler` L34–L41; ordering `Category.Name, Order, CreatedAt`; `PagginatedResult<HelpArticleDto>.Create`.
- [x] Validators: `Search` max 150 (`SearchTooLong`), `PageNumber > 0`, `PageSize` within `1..MaxPageSize` (50).
- [x] `Include(article => article.Category)` for `CategoryName` (AsNoTracking).

### Task 8: Public reads

**Files:** `Queries/GetActiveHelpCategories/`, `Queries/GetActiveHelpArticlesPagginated/`, `Queries/GetActiveHelpArticleById/`

- [x] `GetActiveHelpCategoriesQuery` — same projection as Task 7 but `Where(category => category.IsActive)`. Naming mirrors `GetActiveOnboardingQuestionsQuery`.
- [x] `GetActiveHelpArticlesPagginatedQuery` — `Where(article => article.IsActive && article.Category.IsActive)` plus the optional `categoryId` / `search` filters and paging.
- [x] `GetActiveHelpArticleByIdQuery` — returns one published article or throws `NotFoundException(HelpCenter.ArticleNotFound)`; an inactive article is a 404, not a 403, so hidden content is not enumerable.
- [x] No `ICurrentUserService` dependency in any public handler — anonymous access must work with no user context.

### Task 9: Routes and controllers

**Files:**
- Modify: `LMS.API/Routes/V1/ApiRoutes.cs` (new `HelpCenter` block after `InstructorAvailability`)
- Create: `LMS.API/Controllers/V1/HelpCenterController.cs`
- Create: `LMS.API/Controllers/V1/HelpCenterAdminController.cs`

- [x] Routes: `Base = BaseRoutes.Base + "/help-center"`, `Categories = "categories"`, `Articles = "articles"`, `ArticleById = "articles/{id:guid}"`, `AdminCategories = "admin/categories"`, `AdminCategoryById = "admin/categories/{id:guid}"`, `AdminArticles = "admin/articles"`, `AdminArticleById = "admin/articles/{id:guid}"`.
- [x] Public controller: **no class-level `[Authorize]`** (the feature is anonymous), `[AllowAnonymous]` on each action following `CountriesController` L29–L36; doc comments state that only published content is returned.
- [x] Admin controller: `[ApiVersion("1.0")] [Route(ApiRoutes.HelpCenter.Base)] [Authorize] [RoleAuthorize(UserType.Admin)]`, route-bound ids assigned into the command before `Mediator.Send` (`command.Id = id`) like `OnboardingAdminController`.
- [x] Every action gets XML `<summary>`, `[ProducesResponseType]` for 200/201 plus 400/401/403/404 as applicable, and `return FromResult(result)`.
- [x] Verify: `dotnet build`

### Task 10: Migration, verification and contracts

- [x] `dotnet ef migrations add AddHelpCenter --project LMS.Persistence --startup-project LMS.API --context LmsDbContext` (`--context` is required or the tool aborts with "More than one DbContext was found").
- [x] **Do not run `database update`** — `appsettings.Development.json` points at the live server (`db75937.public.databaseasp.net`) and nothing in the repo or CI applies migrations. Produce a script instead:
  `dotnet ef migrations script <lastApplied> AddHelpCenter --project LMS.Persistence --startup-project LMS.API --context LmsDbContext -o docs/plans/2026-11-03-help-center.sql`
  Note `20261103092630_AddInstructorAvailability` is **still unapplied** as of this writing — if the help-center work ships before that SQL runs, script from `20261102112145_SeparateLearnerAndInstructorProfiles` so one file carries both tables, and tell the user.
- [x] Build clean (`0 Warning(s), 0 Error(s)`) and re-validate `messages.en.json`.
- [ ] After the SQL is applied, curl matrix:
  - `GET /api/v1/help-center/categories` and `.../articles` **with no Authorization header** → 200 (this is the check that anonymous really works).
  - Create category → 201; duplicate name (`" Billing "` vs `"billing"`) → 400 `CategoryNameAlreadyExists`.
  - `DELETE /admin/categories/{id}` with live articles → 400 `CategoryInUse`; after deleting its articles → 200.
  - Create article with `answer` of 20001 chars → 400 `AnswerTooLong`; with an unknown `categoryId` → 400 `CategoryNotFound`.
  - Admin `PUT .../articles/{id}` with `isActive: false` → row still in `admin/articles` but **absent** from `help-center/articles` and its detail GET → 404.
  - `GET /api/v1/help-center/articles?search=refund` matches question and answer.
  - `POST /admin/categories` with a learner token → 403; with an admin token → 201.
  - Paging: `pageSize=51` → 400; `pageSize` clamped path returns `PagginatedResult` envelope fields (`totalCount`, `totalPages`, `hasNextPage`).
- [ ] Regenerate contracts with `pwsh scripts/Update-OpenApi.ps1` then `pwsh scripts/Update-PostmanCollection.ps1` — **the script boots the API on the `http` profile, which runs `BaseSeeder.SeedAllAsync` against the shared production database** (Program.cs L514), so let the user run it deliberately or leave it to the `openapi-postman-sync.yml` workflow on push.
- [x] No commit or push without asking the user.

---

## Out of scope / follow-ups

- Slug or human-readable URL segment per article/category.
- Images, screenshots or file attachments inside answers (the `AttachmentsController` upload + `UploadPaths` plumbing already exists for it).
- Rich text / HTML bodies (would require a sanitizer package and a stored-XSS review).
- Per-article audience targeting (Learner vs Instructor) — explicitly rejected for this round.
- "Contact support" ticketing or a live-chat entry point in the help center.
- View counters / "was this helpful?" feedback on articles.
- Search relevance ranking or full-text indexing; v1 is `Contains` like the rest of the codebase.
