# Instructor Availability Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let an admin manage each instructor's recurring weekly availability and let a learner read the availability of a selected instructor, plus filter the instructor list by a day/time window.

**Architecture:** Clean Architecture with MediatR CQRS handlers, FluentValidation validators (all request validation lives there, including DB checks), EF Core with `IEntityTypeConfiguration` auto-discovery, soft-delete + audit via `BaseEntity<Guid>`, versioned ASP.NET Core controllers using `Result<T>` → `ApiResponse<T>` through `BaseController.FromResult`.

**Tech Stack:** .NET 8 (net8.0), ASP.NET Core, MediatR, FluentValidation, EF Core 8 + SQL Server (databaseasp.net), JSON-based localization.

**Status:** Code complete and building (0 warnings, 0 errors). Blocked on the schema being applied to the shared database — see "Apply the schema".

---

## Scope decisions

> **Superseded 2026-11-04** by [2026-11-04-scheduling.md](2026-11-04-scheduling.md): decisions 1, 2 and 6 below no longer hold. There is a dated `AvailabilityInstances` table, the backend resolves the instructor's zone from `Country.TimezoneId`, and Hangfire is wired in (off by default). The weekly table and the endpoints below are unchanged.

| # | Decision | Choice |
|---|----------|--------|
| 1 | Model | **One table only**: recurring weekly windows (`InstructorAvailabilitySlots`). The dated exception table was cut on 2026-11-03 at the user's request |
| 2 | Time anchoring | Backend stores/returns **naive local wall-clock values** (`TimeSpan` → SQL `time`). No `TimeZoneId`, no UTC conversion. The frontend resolves the instructor's timezone from the country it already knows (phone `Code`) and does the offsetting |
| 3 | Learner reads | Per-instructor availability detail **and** a day/time filter on the existing learner instructor list |
| 4 | Who writes | **Admin only** (`[RoleAuthorize(UserType.Admin)]`). Instructor self-service is out of scope |
| 5 | Delete | Soft delete via `MarkAsDeleted`, consistent with the rest of the codebase |
| 6 | Hangfire | **Deferred.** Nothing in the availability feature needs a background job today; revisit with booking/sessions. Not added to the solution |

Because of decision 2 the learner payload **must** carry the instructor's country anchor (`CountryId`, `CountryCode`, `CountryName`) — otherwise the returned times are un-interpretable. `InstructorCardDto` exposes only `CountryName`, so the availability DTO adds the other two.

## Data model

```
Instructor 1 ──< InstructorAvailabilitySlot (Day, StartTime, EndTime)
```

- `Day` is BCL `DayOfWeek` stored as `int` (0 = Sunday … 6 = Saturday).
- Overnight windows are not supported: `EndTime > StartTime` is enforced by the validators.
- The entity inherits `BaseEntity<Guid>` (Id, CreatedAt/By, UpdatedAt/By, IsDeleted, IsActive, `Version` rowversion) and the configuration adds `HasQueryFilter(slot => !slot.IsDeleted)`, so overlap checks and reads never see soft-deleted rows.
- The global UTC `DateTime` value-converter loop in `LmsDbContext.OnModelCreating` only touches `DateTime`/`DateTime?`, so `TimeSpan` columns stay naive as intended.

## Endpoints

Admin (`/api/v1/instructor-availability`, `[RoleAuthorize(UserType.Admin)]`):

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `pagginated?instructorId=&day=&pageNumber=&pageSize=` | Slots across instructors, with instructor name/specialization |
| POST | `instructors/{instructorId:guid}/weekly-slots` | Add a window (201) |
| PUT | `weekly-slots/{id:guid}` | Edit a window (instructor is unchanged) |
| DELETE | `weekly-slots/{id:guid}` | Soft delete |

Learner (`[RoleAuthorize(UserType.Learner)]`):

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/v1/instructor-availability/instructors/{instructorId:guid}` | Country anchor + `weeklySlots` for one approved instructor (404 otherwise) |
| GET | `/api/v1/onboarding/instructors?day=&after=&before=` | Existing list, now filterable by recurring availability |

The list filter requires one slot on that day to **cover** the requested window; omitted bounds widen to the whole day.

---

### Task 1: Domain entity — done

**Files:**
- Create: `LMS.Domain/Entities/InstructorAvailabilitySlot.cs`
- Modify: `LMS.Domain/Entities/Instructor.cs`

- [x] `InstructorAvailabilitySlot : BaseEntity<Guid>` with private setters, static `Create(instructorId, day, startTime, endTime)` and `Update(day, startTime, endTime)`. `Update` does not call `MarkAsUpdated` — `LmsDbContext.SaveChangesAsync` already stamps Modified entities.
- [x] `Instructor` gained `ICollection<InstructorAvailabilitySlot> AvailabilitySlots` and `AddAvailabilitySlot`.
- [x] Exception entity removed from scope (file deleted before it reached the model).

### Task 2: EF Core mapping and DbContext surface — done

- [x] `LMS.Persistence/Configurations/InstructorAvailabilitySlotConfiguration.cs`: table `InstructorAvailabilitySlots`, `Day` as int, `StartTime`/`EndTime` as `time`, `CreatedBy` required, `Version` rowversion, cascade FK to `Instructors`, indexes on `InstructorId`, `(InstructorId, Day, StartTime)` and `IsDeleted`, soft-delete query filter.
- [x] `DbSet<InstructorAvailabilitySlot> InstructorAvailabilitySlots` added to **both** `LmsDbContext` and `ILmsDbContext`.

### Task 3: Migration — generated, NOT applied

- [x] `LMS.Persistence/Migrations/20261103092630_AddInstructorAvailability.cs` (+ Designer + snapshot).
  Command: `dotnet ef migrations add AddInstructorAvailability --project LMS.Persistence --startup-project LMS.API --context LmsDbContext`
  (`--context` is required: the tool reports more than one DbContext otherwise.)
- [x] Upgrade script written to `docs/plans/2026-11-03-instructor-availability.sql`.
- [ ] **Apply the schema** — the user runs the script. `appsettings.Development.json` points at the **live** server (`db75937.public.databaseasp.net`) and nothing in the repo or CI applies migrations, so `dotnet ef database update` must not be run from a dev box. The script also inserts a row into `__EFMigrationsHistory`; if the production database is not migration-tracked, run only the `CREATE TABLE`/`CREATE INDEX` statements.

### Task 4: Localization — done

- [x] `LocalizationKeys.Availability` block: `InstructorIdRequired`, `SlotIdRequired`, `SlotNotFound`, `InstructorNotFound`, `InstructorNotApproved`, `DayInvalid`, `DayRequiredForTimeFilter`, `FilterTimeRangeInvalid`, `EndTimeAfterStartTime`, `OverlappingSlot`, `SlotCreatedSuccess`, `SlotUpdatedSuccess`, `SlotDeletedSuccess`, `SlotsRetrieved`, `AvailabilityRetrieved`, `PageNumberInvalid`, `PageSizeInvalid`.
- [x] Matching values in `messages.en.json` under a new `"Availability"` object. English-only: no `messages.ar.json` exists (the `Localization file not found: messages.ar.json` warning during `dotnet ef` predates this change).

### Task 5: DTOs — done

- [x] `DTOs/AvailabilitySlotDto.cs` — `Day` (int) **and** `DayName` (string) so the frontend needs no enum; `TimeSpan` start/end.
- [x] `DTOs/InstructorAvailabilityDto.cs` — instructor id/user id/full name, `CountryId`, `CountryCode`, `CountryName`, `HourlyRate`, `WeeklySlots`.
- [x] `DTOs/InstructorAvailabilityRowDto.cs` — admin paginated row (adds instructor full name + specialization).
- [x] `AvailabilityMapper.cs` — `ToSlotDto`, `ToRowDto`.

### Task 6: Admin write commands — done

`Features/Availability/Commands/Admin/{AddAvailabilitySlot,EditAvailabilitySlot,DeleteAvailabilitySlot}` (command + handler + validator each).

- [x] Shared rules in `Features/Availability/AvailabilityValidation.cs` (`internal static`): `HasNoOverlapAsync`, `InstructorExistsAsync`, `InstructorIsApprovedAsync`.
- [x] **Add** validator: instructor required/exists/approved, `Day` in enum, `EndTime > StartTime`, no overlapping window on the same day (`slot.StartTime < end && slot.EndTime > start`). Handler creates through the factory, one `SaveChangesAsync`, returns 201.
- [x] **Edit** validator: reads the stored row's `InstructorId` (the slot never moves instructors) and re-runs the overlap check excluding `command.Id`. Handler throws `NotFoundException(Availability.SlotNotFound)` → 404.
- [x] **Delete**: `MarkAsDeleted(_currentUserService.UserId.ToString())`, `Result<string>.Success`.
- [x] The misspelled empty scaffold folders (`AddAviabilityForInstructor`, `EditAviabilityForInstructor`, `DeleteAviabilityForInstructor`, `GetTeacherAvialabilityPagginated`) were deleted; `Pagginated` spelling kept to match `PagginatedResult`.

### Task 7: Exception dates — dropped (see scope decision 1)

### Task 8: Admin paginated read — done

- [x] `Queries/GetInstructorAvailabilityPagginated/{Query,QueryHandler,QueryValidator}` — filters by `InstructorId`/`Day`, `Include(Instructor).ThenInclude(User)` for the name, `PagginatedResult.Create`, ordering `InstructorId, Day, StartTime`, clamped to `MaxPageSize` (50). Admin sees slots of non-approved instructors too.

### Task 9: Learner read for the selected instructor — done

- [x] `Queries/GetInstructorAvailabilityForLearner/{Query,QueryHandler,QueryValidator}`. Validator only checks `InstructorId` is present; the handler applies the same approved/active predicate as `GetInstructorsForLearnerQueryHandler` and throws `NotFoundException(Availability.InstructorNotFound)` → 404, so an unapproved instructor is not distinguishable from a missing one.
- [x] Response carries `countryId/countryCode/countryName` as the timezone anchor; times are returned raw (no `Z`, no shift).

### Task 10: Availability filter on the learner instructor list — done

- [x] `GetInstructorsForLearnerQuery` gained `DayOfWeek? Day`, `TimeSpan? After`, `TimeSpan? Before`.
- [x] Handler filters with a single `Any(...)` (translates to `EXISTS`, so instructors are never duplicated). Missing bounds become end-of-day / midnight defaults so one predicate covers all cases.
- [x] Validator: `Day` must be a defined value, a time bound without a `Day` is rejected, `Before > After` when both are supplied.

### Task 11: Routes, controllers and DI — done

- [x] `ApiRoutes.InstructorAvailability` with `Base`, `GetAllPagginated`, `ForInstructor`, `Slots`, `SlotById`.
- [x] `Controllers/V1/InstructorAvailabilityAdminController.cs` (4 actions) and `Controllers/V1/InstructorAvailabilityController.cs` (1 action), both `: BaseController` with `ProducesResponseType` sets and `FromResult`.
- [x] Five validators registered in `LMS.Application/DependencyInjection.cs` (MediatR handlers are assembly-scanned; validators are listed one by one).

### Task 12: Verification

- [x] `dotnet build` → 0 warnings, 0 errors.
- [x] `messages.en.json` parses as valid JSON.
- [ ] Apply the SQL (user) then exercise with `curl`, admin token first:
  - `POST .../instructors/{id}/weekly-slots` body `{"day":1,"startTime":"14:00:00","endTime":"18:00:00"}` → 201.
  - Same window again → 400 `Availability.OverlappingSlot`; `endTime` before `startTime` → 400.
  - `GET .../pagginated?instructorId={id}` → 200 with `instructorFullName`.
  - `PUT .../weekly-slots/{slotId}` moving it onto a window owned by another slot → 400; unknown id → 404.
  - `DELETE .../weekly-slots/{slotId}` → 200, and the slot disappears from the learner payload (soft delete).
  - As a learner: `GET /api/v1/instructor-availability/instructors/{id}` → 200 with raw `startTime`/`endTime`; non-approved instructor → 404; instructor role → 403.
  - `GET /api/v1/onboarding/instructors?day=1&after=14:00:00&before=16:00:00` → only instructors whose Monday window covers 14:00–16:00.
- [ ] Regenerate the contracts: `pwsh scripts/Update-OpenApi.ps1` then `pwsh scripts/Update-PostmanCollection.ps1`. **Note:** that script starts the API with the `http` profile, which runs `BaseSeeder.SeedAllAsync` against the shared production database — do it deliberately, or let the `openapi-postman-sync.yml` workflow do it on push.
- [ ] No commit or push without asking the user.

---

## Out of scope / follow-ups

- Dated exception days (holidays, single-day blocks) — cut from this round; would be a second table plus an `IsAvailable` override.
- Hangfire background jobs — nothing in this feature needs one yet. If added later, note that `Hangfire.SqlServer` creates its own tables on startup, which would hit the shared production database.
- Instructor self-service for their own availability (admin writes only).
- Booking/sessions: no session or booking entity exists, so slots are display data until booking lands.
- A bulk "save the whole week" `PUT instructors/{id}/availability` replacing every window in one call, if the admin UI needs it.
