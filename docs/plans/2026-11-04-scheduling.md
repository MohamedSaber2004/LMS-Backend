# Scheduling Implementation Plan (§1.2 — Availability · Sessions · Booking)

**Goal:** Turn the recurring weekly availability of an approved instructor into dated, bookable windows, and put a session/booking/reschedule lifecycle on top of them.

**Architecture:** Clean Architecture with MediatR CQRS, FluentValidation validators holding **all** request and DB-state checks, EF Core 8 with auto-discovered `IEntityTypeConfiguration`, soft delete + audit from `BaseEntity<Guid>`, `Result<T>` → `ApiResponse<T>` through `BaseController.FromResult`. Background work runs on Hangfire recurring jobs that are **off by default**.

**Tech Stack:** .NET 8, ASP.NET Core, MediatR, FluentValidation, EF Core 8 + SQL Server (databaseasp.net), Hangfire 1.8.25 (`SqlServerStorage`, custom `HangFire` schema), JSON localization.

**Status:** Code complete, `dotnet build LMS.slnx -p:OutputPath=bin/verify` → **0 warnings / 0 errors**. Keys file and resource file are 1:1 (303 = 303). Not applied to any database — see "Hand-offs the owner applies".

Supersedes decisions 1, 2 and 6 of [2026-11-03-instructor-availability.md](2026-11-03-instructor-availability.md): there is now a dated table, the backend does resolve the zone, and Hangfire is wired.

---

## Scope decisions

| # | Decision | Choice |
|---|----------|--------|
| 1 | Phases | **Phase 0 + 1 + 2**. Phase 3 (money) stays gated on D1/D2/D3, Phase 4 (live class) on D4 + the `Permissions-Policy` blocker |
| 2 | Booking model | Learner books → `Session.Pending` → **instructor confirms** (`/confirm`, `/decline`). No payment, no auto-confirm |
| 3 | Timezone anchor | **No `Instructor.Timezone` column, no human input.** Derived from `Country.TimezoneId` (IANA): the instructor's profile country first, then the dial code of their phone number as a longest-prefix fallback |
| 4 | Weekly vs dated | `InstructorAvailabilitySlots` stays the **recurring definition**. A Hangfire job expands it into dated rows in `AvailabilityInstances`; bookings take a dated row |
| 5 | Exceptions | `AvailabilityException` was **deleted**. Blocking a day is `IsOpen = false` on the generated rows, which the job never silently re-opens |
| 6 | Regeneration | The job never touches a local date that still holds a live session, and never auto-deletes a row the instructor blocked by hand or that has no weekly parent |
| 7 | `VacationMode` | Belongs to the availability surface, **not** the instructor profile DTO. It shields new bookings only; confirmed sessions keep their slots |
| 8 | Session ↔ instance | Matched by **UTC overlap**, no FK. `Sessions.StartsAtUtc/EndsAtUtc` are indexed with `(StartsAtUtc, EndsAtUtc)`, so the check is a range scan |
| 9 | Group sessions | `SessionType.Group`, `SeatCapacity` and `SeatsBooked` exist in the model, but **nothing creates one** — no prototype screen shows a group-session creator |
| 10 | Session price | `Session.Price = Instructor.HourlyRate` for the whole window. Proration and refund treatment are Phase 3 (D1/D2) |
| 11 | Hangfire safety | `PrepareSchemaIfNecessary = false`, `SchemaName = "HangFire"`, `Hangfire:Enabled` defaults to **false**. Nothing touches the shared production DB until the owner applies the schema and flips the flag |

### Why the dated table at all

A weekly slot is `DayOfWeek` + `TimeSpan` — wall clock, not an instant. A booking needs an instant, so something must combine the pattern with a zone and a date. That is the materialization job's whole job, and it is also why `AvailabilityInstances` carries both `Date` (the instructor's own calendar date) and the two UTC bounds.

The same reasoning is why every **request** date in this feature is a `DateOnly` and never a `DateTime`: `calendar?from&to` and `bookable-slots?from&to` bind through MVC's default binder, which would parse a `DateTime` in the *server's* zone and never reach `IsoUtcDateTimeConverter`. A `DateOnly` has no zone to lose. The only `DateTime` inputs are `RequestRescheduleCommand.FromUtc/ToUtc`, which arrive in a JSON body and therefore do go through the UTC converter.

---

## Data model

```
ApplicationUser ──1 Instructor ──< InstructorAvailabilitySlot   (weekly definition)
                       │  └──────< AvailabilityInstance         (dated, generated)
                       │                 ▲ matched by UTC overlap
Country ──TimezoneId───┘                 │
                     Session ──1 ──< SessionBooking >──1 Learner
                       │  └──< RescheduleRequest
```

- `AvailabilityInstance` — `InstructorId`, `WeeklySlotId?` (null for a day the instructor added by hand), `Date` (`date` column), `StartsAtUtc`, `EndsAtUtc`, `IsOpen`. Indexed on `InstructorId`, `(InstructorId, Date)`, `(StartsAtUtc, EndsAtUtc)`. No FK on `WeeklySlotId`: a dated row must survive the deletion of its weekly parent.
- `Session` — `InstructorId`, `Title?`, `Type`, `StartsAtUtc`, `EndsAtUtc`, `Status`, `Price`, `SeatCapacity`, `SeatsBooked`, cancel audit. `Cancel(Guid?, string?)` takes a nullable actor so the **system** can expire rows.
- `SessionBooking` — unique `(SessionId, LearnerId)`, `Status`, cancel audit. `Learner` FK is `Restrict`.
- `RescheduleRequest` — `SessionId`, `RequestedByUserId`, `RequestedByType`, `FromUtc`, `ToUtc`, `Status`, respond audit.
- Every `DateTime` in the model is UTC; the global `ValueConverter` in `LmsDbContext` enforces it. `DateOnly` is not covered by that converter and is stored as a plain `date`.
- `TimezoneHelper` (`LMS.Application/Common/Helpers`) is the **only** wall-clock ↔ UTC bridge. Nothing else calls `TimeZoneInfo` directly.

## Lifecycle

```
 learner books ──► Session.Pending ──┬─ instructor /confirm ──► Confirmed ──┬─ ends ──► Completed (job)
                                     │                                     └─ /cancel (≤24 h blocked)
                                     ├─ instructor /decline ──► Cancelled (bookings Declined)
                                     └─ job expires it ─────► Cancelled (bookings Cancelled, seat released)
```

`NoShow` is written by nobody yet — it belongs to Phase 9 (attendance).

Locks, measured against `StartsAtUtc`: **24 h** for the instructor, **12 h** for a learner. Same keys reject a late `/cancel` and a late `/reschedule`. The frontend pre-checks from data the list already returns; the validator is the real guard.

A reschedule **never** moves a session by itself. Approval is the only mutation, it re-checks that the proposed window still sits inside an open availability row and is free for the instructor **and** for every learner holding a seat, and it preserves the duration — the request validator rejects a proposed window of a different length, so approval cannot quietly change the lesson.

## Endpoints

Instructor self-service (`/api/v1/instructor-availability`, `[Authorize] + [RoleAuthorize(UserType.Instructor)]`) — nothing reads an instructor id from the body:

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `my` | Own weekly slots + derived `timezoneId` + `vacationMode` |
| POST | `my/weekly-slots` | Add a recurring window (201) |
| PUT | `my/weekly-slots/{id}` | Edit one |
| DELETE | `my/weekly-slots/{id}` | Soft delete one |
| PUT | `my/vacation-mode` | `{ "enabled": true }` |
| GET | `my/calendar?from&to` | Dated days with `Available` / `Blocked` / `Booked` windows, max 92 days |
| PUT | `my/calendar/day` | `{ "date": "2026-11-07", "open": false }` — block or open one day |

Admin (`[RoleAuthorize(UserType.Admin)]`, was `Admin, Instructor` — that was the §2 security defect):

| Method | Route |
|--------|-------|
| GET | `/api/v1/instructor-availability/pagginated` |
| POST | `/api/v1/instructor-availability/instructors/{instructorId}/weekly-slots` |
| PUT/DELETE | `/api/v1/instructor-availability/weekly-slots/{id}` |

Sessions (`[Authorize] + [RoleAuthorize(Instructor, Learner)]`, ownership re-checked per call):

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/v1/sessions?tab=&pageNumber=&pageSize=` | The caller's **teaching** list |
| GET | `/api/v1/sessions/my-bookings?tab=&…` | The caller's **bookings** |
| GET | `/api/v1/sessions/bookable-slots?instructorId=&from=&to=` | Open dated rows with nothing live inside them |
| POST | `/api/v1/sessions` | Learner books one dated row (201) |
| GET | `/api/v1/sessions/{id}` | Detail — roster only for the teacher |
| POST | `/api/v1/sessions/{id}/confirm` · `/decline` | Teacher answers a Pending booking |
| POST | `/api/v1/sessions/{id}/cancel` | Teacher 24 h / learner 12 h; a learner gives up the seat, and the session goes only if nobody holds one |
| POST | `/api/v1/sessions/{id}/reschedule` | `{ fromUtc, toUtc }` → creates a request (201) |
| POST | `/api/v1/reschedule-requests/{id}/approve` · `/decline` | Only the side that did not ask |

`GET /sessions?tab=` and `GET /sessions/my-bookings` are two routes, not one branching route: the README lists a single `?tab=` endpoint, but one handler that silently changes ownership rules by role is exactly the class of bug §2 describes.

## Guards, and where each one lives

All of these are validator rules — the handlers orchestrate.

| Rule | Key |
|------|-----|
| Weekly window must not overlap another window of the same day | `Availability.OverlappingSlot` |
| A weekly slot cannot be edited or deleted while sessions still sit in its dated rows | `Availability.SlotHasSessions` |
| A day cannot be blocked while a session is still scheduled on it | `Availability.DayHasSessions` |
| The date must be today or later **in the instructor's zone** | `Availability.DateInPast` |
| Only an approved, active, non-vacationing instructor can be booked | `Sessions.InstructorNotBookable` |
| The chosen row must exist, belong to that instructor, be open, and not have started | `Sessions.SlotNotOpen` · `Sessions.SlotElapsed` |
| No double-booking for the instructor, and the same for the learner | `Sessions.InstructorBusy` · `Sessions.LearnerBusy` |
| 15-minute break on both sides of every session | `Sessions.InstructorBreakTooShort` · `Sessions.LearnerBreakTooShort` |
| A learner cannot hold two seats on one session | unique `(SessionId, LearnerId)` + `Sessions.SessionNotPending` |
| Only one Pending reschedule per session | `Sessions.RescheduleAlreadyPending` |
| Proposed window must be the same length as the session | `Sessions.RescheduleDurationChanged` |
| Approval re-checks the window is still inside availability and still free | `Sessions.RescheduleConflicted` |
| Any write from a non-owner | `ForbiddenException` → 403, never a field error |

## Background jobs

| Job | Cron (UTC) | What it does |
|-----|-----------|--------------|
| `scheduling-materialize-availability` | `0 2 * * *` | For every approved instructor with slots: resolve the zone, expand the weekly pattern over `DaysAhead` local days, create missing rows, re-time drifted rows, soft-delete rows whose pattern went away |
| `scheduling-expire-sessions` | `*/10 * * * *` | Elapsed `Pending` → `Cancelled` (+ bookings + seat released); elapsed `Confirmed` → `Completed` |

- Batching: instructors in groups of `InstructorBatchSize`, sessions in groups of 500.
- A job whose instructor zone cannot be resolved returns `-1` and is logged, never guessed at. `LmsDbContext.SaveChangesAsync` stamps audit with `"System"` when there is no HTTP user.
- Job methods are parameterless (`CancellationToken.None` inside) because Hangfire serializes the arguments.
- **The nightly job is not the only path.** `Add`/`Edit`/`Delete` of a weekly slot call `MaterializeInstructorAsync` synchronously, so a slot added today is bookable today even with `Hangfire:Enabled: false`. Blocking or opening a day needs no job at all: it writes `IsOpen` straight onto the dated rows.

## Configuration

```json
"Hangfire": { "Enabled": false },
"Scheduling": { "DaysAhead": 30, "MaxDaysAhead": 365, "InstructorBatchSize": 50,
                "MinimumBreakMinutes": 15, "InstructorLockHours": 24, "LearnerLockHours": 12 }
```

Only `LMS.API/appsettings.json` carries them; the defaults are in `SchedulingOptions` so a missing section is still correct. Remember the deployment caveat: the live environment is pinned to `Test` and `appsettings` are excluded from the FTP deploy, so these values change only through the workflow patch step.

---

## Hand-offs the owner applies

`dotnet ef database update` was **never** run. Three scripts, in order:

1. **`docs/plans/2026-11-04-scheduling.sql`** — the idempotent form of migration `20261104102902_AddSchedulingTables`: `Instructors.VacationMode`, `Countries.TimezoneId`, `AvailabilityInstances`, `Sessions`, `SessionBookings`, `RescheduleRequests`.
2. **`docs/plans/2026-11-04-country-timezones.sql`** — fills `Countries.TimezoneId` for ~190 dial codes. Without it **no instructor has a zone, so nothing is bookable**; it is not optional. Ambiguous dial codes are left empty on purpose (see the file).
3. **`docs/plans/2026-11-04-hangfire-schema.sql`** — the official Hangfire installer, verbatim, schema `HangFire`. Apply it **before** setting `Hangfire:Enabled: true`; with `PrepareSchemaIfNecessary = false` the app will not create it.

The existing `2026-11-04-instructor-self-service.sql` still has to be applied for `Instructor.HourlyRate`.

## Open gaps

| # | Gap | Why it is still open |
|---|-----|----------------------|
| G1 | Refund / wallet effect of a cancellation | Blocked on **D1**. `CancelSessionCommand.Reason` is stored; nothing is credited |
| G2 | Group sessions have no creator | No prototype screen shows one, so no endpoint was invented. `SeatCapacity`/`SeatsBooked` are shaped for it |
| G3 | Two learners booking the same dated row concurrently | The guards read then write; the real fix is a unique index or a keyed lock, not a validator. `Version` protects an existing row, not the creation of a new one |
| G4 | Ambiguous dial codes (+1, +7, +52, +55, +61, +62, +590/599, +689/691/692) | One code, many zones. The profile `CountryId` wins when set; the review query at the bottom of the timezone script lists the rows still empty |
| G5 | `NoShow` is never written | Phase 9 (attendance) owns it, and it must gate a miss behind the technical-failure review queue |
| G6 | `Session.Title` is never set by anything | `tsched` does not show a title field. Nullable, so no column was invented for it |
| G7 | Admin session dashboard | Phase 8 — a derived aggregate; building it now would only produce fake numbers |
| G8 | The learner-facing `InstructorAvailabilityDto` gained `TimezoneId` | Additive, but the frontend lives outside this workspace, so the change is **unverified** against the actual client |

## Decisions worth re-visiting

- **`Price = HourlyRate` for the whole window.** Once Phase 3 needs per-seat group pricing or a trial lesson, this becomes the place where the rule is applied.
- **Overlap matching instead of an FK from `Session` to `AvailabilityInstance`.** Chosen to avoid inventing a column the prototype never asks for. If a session must always point at the row it came from, that is a one-column migration plus a back-fill by overlap.
- **`SessionBooking.Learner` FK is `Restrict`** while `Session → Instructor` cascades, so deleting an instructor hard-deletes their sessions only if nothing blocks on a learner row. Untested against real data.
