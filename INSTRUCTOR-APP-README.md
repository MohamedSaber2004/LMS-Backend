# SOPHIA / LMS — Instructor App: Feature Analysis & Implementation Plan

**Date:** 2026-11-04
**Scope:** Everything the *Teacher App* half of the clickable prototype promises, measured against what the backend actually implements today.
**Prototype source:** `SOPHIA – Interactive Prototype last.html` — 23 instructor screens (`twelcome`, `treg`, `tstatus`, `tdash`, `tsched`, `tsess`, `tresch`, `tavail`, `tlive`, `tsum`, `tchats`, `tchat`, `tfb`, `tstud`, `tstu`, `tearn`, `tstats`, `tach`, `tme`, `taff`, `tnotif`, `twarn`, `tset`).
**Backend measured at:** commit `cbe4335`.

---

## Headline

The backend takes an instructor as far as **"registered and approved"** and stops. There is no session, no booking, no money, no live class, no chat, no feedback, no notification, and no dashboard.

| Metric | Value |
|---|---|
| OpenAPI operations reachable by the `Instructor` role | **8 of 51** — was **4 of 47** before the §1.1 self-service build landed 2026-11-04 |
| `DbSet`s in `LmsDbContext` | 13 — of which 4 are instructor-related |
| Instructor flows in the prototype | 17 |
| Fully implemented | **0** |
| Partially implemented | **6** (was 5) |
| Not started | **11** (was 12) |

The only instructor-authored path in the whole API is registration.

> **Blocked, and it is not a code bug:** a `Pending` or `Rejected` applicant **cannot log in at all**, so `my-status` / `resubmit` are unreachable until that changes. `RegisterCommandHandler` sets `IsActive = !isInstructor`, `LoginCommandValidator` refuses an inactive user, `VerifyOtpCommandHandler` withholds activation while the application is still `Pending`, and the `Instructor` Identity role is only granted at approval. The new application endpoints are `[Authorize]` with no role for exactly this reason — but that does not help if login itself fails. **Product decision needed:** either let an applicant log in (learner-visible queries already filter `Status == Approved && IsActive`, so nothing leaks), or give applicants a token-free status/resubmit route.

---

## 1. Coverage matrix

Status legend: ✅ done · 🟡 partial · 🔴 absent

### 1.1 Registration & approval

| # | Feature (screen) | Status | Implemented | Gap |
|---|---|---|---|---|
| 1 | Register — 6-step wizard (`twelcome`, `treg`) | 🟡 80% | `POST /api/v1/authentication/register` with `Role=Instructor`: `Specialization`, `Bio`, `YearsOfExperience`, `Education`, `LinkedInUrl`, `HourlyRate`, `Certificates[]`, `IntroVideo`; creates `Instructor` + `InstructorApplication(Pending)`; uploads files; sends OTP email; `IsActive = false` until approval — `LMS.Application/Features/Authentication/Commands/Register/RegisterCommandHandler.cs:51` | **No draft state** — registration is one-shot multipart, the prototype lets you leave and resume. **`TeachingLanguages` and `ArabicDialects`** (wizard step 3) have no column, no entity, no DTO. **No timezone** — the prototype shows *"Time zone: Africa/Cairo (auto-detected)"* while slots are stored as naive wall-clock `TimeSpan` |
| 2 | Application status (`tstatus`) | ✅ 90% | `GET /api/v1/instructor-applications/my-status` → `{ Status, HourlyRate, IsActive, EmailConfirmed, SubmittedAt, ReviewedAt, ReviewNotes, CertificatesCount, HasIntroVideo }` and `POST /api/v1/instructor-applications/resubmit` (multipart, re-uploads certificates, resets the review snapshot, allowed only from `Rejected`) — both owned by the calling user id from the token, never a route id. `InstructorApplication.Resubmit` clears `ReviewedByUserId` / `ReviewedAt` / `ReviewNotes` | **Blocked by the login gate above.** No **`Draft`** status (explicitly deferred by the user 2026-11-04) |
| 3 | Hidden until approved | ✅ | `IsActive = !isInstructor` at registration; admin approve flips visibility. Since 2026-11-04 `ApproveInstructorApplicationCommandValidator` also refuses to approve an applicant whose email is still unverified — approval force-marks `EmailConfirmed`, which silently blessed unreachable addresses | Side effect of the same `IsActive` rule: an applicant cannot log in (see Headline) |

### 1.2 Scheduling

| # | Feature (screen) | Status | Implemented | Gap |
|---|---|---|---|---|
| 4 | Availability (`tavail`) | ✅ 90% | `InstructorAvailabilitySlot(Day, StartTime, EndTime)` stays the recurring definition; a Hangfire job expands it plus `Country.TimezoneId` into dated `AvailabilityInstances` rows that a booking can actually take. Instructor self-service `GET my`, `POST/PUT/DELETE my/weekly-slots[/{id}]`, `PUT my/vacation-mode`, `GET my/calendar?from&to`, `PUT my/calendar/day`. Zone is **derived, never typed**: profile country first, phone dial code as fallback. Conflict guard before editing/closing a slot or blocking a day. Admin controller is `[RoleAuthorize(Admin)]` only — the §2 defect is closed | `Hangfire:Enabled` is still `false` and the schema/timezone SQL is unapplied, so nothing is dated yet in the live environment. Group price-per-seat and the `tavail` slot-tap rule stay open |
| 5 | Sessions (`tsched`, `tsess`, `tresch`) | 🟡 70% | `Session`, `SessionBooking`, `RescheduleRequest` + `SessionStatus`/`BookingStatus`/`SessionType`/`RescheduleRequestStatus`. `GET /sessions?tab=`, `GET /sessions/my-bookings?tab=`, `GET /sessions/bookable-slots`, `POST /sessions` (learner takes a dated row → `Pending`), `GET /sessions/{id}`, `POST /sessions/{id}/confirm` · `/decline` · `/cancel` · `/reschedule`, `POST /reschedule-requests/{id}/approve` · `/decline`. **24 h teacher / 12 h student** lock, double-booking and 15-minute-break checks on both sides, one-Pending-reschedule rule, duration-preserving approval with a full conflict re-check, and a 10-minute job that expires elapsed sessions | **No group-session creator** — `SeatCapacity`/`SeatsBooked` exist but nothing writes `Type = Group`. `NoShow` is never set (Phase 9). No refund/wallet effect (D1). Concurrent bookings of the same dated row are not serialized |
| 6 | Live class (`tlive`, `tsum`) | 🔴 0% | — | No SignalR, no video provider, no join logs, no participant list, no attendance marking, no session summary. `appsettings.*.json` **actively denies** the permissions a live class needs (see §3) |

### 1.3 Teaching relationships

| # | Feature (screen) | Status | Implemented | Gap |
|---|---|---|---|---|
| 7 | My students (`tstud`, `tstu`) | 🔴 0% | — | `Level`, `Completed sessions`, `Upcoming`, `Attendance %`, `Progress` — all derived from a `Session` domain that does not exist |
| 8 | Session feedback (`tfb`) | 🔴 0% | — | No `Feedback` entity. **The pattern already exists and is reusable:** `OnboardingQuestion` / `OnboardingOption` / `OnboardingAnswer` is structurally exactly what the prototype asks for here (*"Questions and choices come from the admin dashboard. Teachers cannot add questions"*) |
| 9 | Chats (`tchats`, `tchat`) | 🔴 0% | — | No `Conversation`, no `Message`, no hub. Prototype rules to encode: `Active` vs `Closed`, closed chat is **read-only** and reopens with a new booking, unread badge, search |

### 1.4 Money

| # | Feature (screen) | Status | Implemented | Gap |
|---|---|---|---|---|
| 10 | Earnings & payouts (`tearn`) | 🔴 0% | Two monetary fields, both new-shape since 2026-11-04: `Instructor.HourlyRate` is the **live** 1-to-1 price (reader queries and `PUT /instructor-profile` use it) and `InstructorApplication.HourlyRate` is the **submitted snapshot** an admin reviews (set at register, refreshed at resubmit) | No `Payment`, `WalletTransaction`, `PayoutRequest`, `Commission`. No payment provider package, no webhook controller. Gross / commission / net / pending settlement / available / request payout — all absent. The prototype is explicit: *"Commission % is configured in the backend, not hardcoded"* |

### 1.5 Instructor self-service surface

| # | Feature (screen) | Status | Implemented | Gap |
|---|---|---|---|---|
| 11 | Profile / "Me" (`tme`) | 🟡 60% | `GET` / `PUT /api/v1/instructor-profile` (new 2026-11-04): `Specialization`, `Bio`, `YearsOfExperience`, `Education`, `LinkedInUrl` and the **live** `HourlyRate`, gated to `RoleAuthorize(Instructor)` and only once the application is `Approved`. `PUT /api/v1/authentication/update-user-information` still owns the 4 account fields: `FullName`, `CountryId`, `Language`, `ProfilePictureUrl` | **Group price-per-seat** and the **trial-lesson flag** have no column — deliberately not invented, they land with Phase 1. `Approved`-only edit rule means a `Rejected` instructor must use `resubmit`, which is the only path that refreshes the review snapshot |
| 12 | Settings (`tset`) | 🟡 25% | Change password ✅ · Help Center ✅ (public read + admin CRUD) | Bank / payment details, notification preferences, "Show profile publicly", support tickets |
| 13 | Dashboard (`tdash`) | 🔴 0% | — | `Today`, `Net this month`, `Students`, `Hours` — every tile is an aggregate over missing domains |
| 14 | Performance insights (`tstats`) | 🔴 0% | — | 18 metrics × 3 periods (Week/Month/Quarter) with "vs previous" deltas |
| 15 | Achievements & rank (`tach`) | 🔴 0% | — | 10 achievements, points, 4 tiers (Bronze/Silver/Gold/Platinum), leaderboard |
| 16 | Notifications (`tnotif`) | 🔴 0% | — | No `Notification` entity, no provider, no read/unread state |
| 17 | Attendance & warnings (`twarn`) | 🔴 0% | — | Consecutive-miss counter, escalation at 1/2/3 → warning / final warning / **account closure**, plus the technical-failure review queue that must gate a miss before it counts |

---

## 2. Security defect — fix before any new feature

`LMS.API/Controllers/V1/InstructorAvailabilityAdminController.cs:17`

```csharp
[RoleAuthorize(UserType.Admin, UserType.Instructor)]
...
[HttpPost(ApiRoutes.InstructorAvailability.Slots)]
public async Task<IActionResult> AddSlot([FromRoute] Guid instructorId, ...)
```

`AddAvailabilitySlotCommandHandler.cs:31` passes `request.InstructorId` straight into `InstructorAvailabilitySlot.Create(...)`. `EditAvailabilitySlotCommandHandler` resolves the slot by route `id` alone. **Neither handler injects `ICurrentUserService`, and neither checks ownership.**

> **Impact:** any authenticated instructor can add, edit, or delete **another instructor's** availability slots. `DeleteAvailabilitySlotHandler` uses `ICurrentUserService` only to stamp `DeletedBy`.

**Root cause:** the design doc scoped writes as *"Admin only — instructor self-service is out of scope"* (scope decision #4). The `Instructor` role was later added to the class-level gate without the handlers being brought in line.

**Fix (Phase 0):** derive `InstructorId` from `ICurrentUserService` for instructor callers, and keep a separate admin-only route for acting on behalf of another instructor.

> **Closed 2026-11-04.** `InstructorAvailabilityAdminController` is `[RoleAuthorize(UserType.Admin)]`, and self-service lives on `MyAvailabilityController` (`[Authorize] + [RoleAuthorize(UserType.Instructor)]`) where no route or body carries an instructor id. `ActorOwnership` (`LMS.Application/Common/Helpers/ActorOwnership.cs`) is now the single place that decides *write target* (`ResolveWriteTargetAsync`: an admin must name an instructor, anyone else is pinned to their own row), *read target* (`ResolveInstructorIdAsync` / `ResolveLearnerIdAsync`) and *cross-owner attempt* (`EnsureCanWriteForAsync` → `ForbiddenException` = 403, never a field error). Every session write re-resolves the caller against the session's own `InstructorId`/bookings in `SessionGuards`.

---

## 3. Environment blocker for the live-class phase

All four `appsettings` files deny the browser permissions a live class requires:

```
"PermissionsPolicy": "camera=(), microphone=(), geolocation=(), payment=(), usb=(), ..."
```

`LMS.API/appsettings.Development.json:86`, `appsettings.Production.json:55`, `appsettings.Test.json:55`, `appsettings.json:26`

Any WebRTC feature will fail at the permission prompt before a line of application code runs. Note also that `payment=()` is denied — relevant to Phase 3.

> **Deployment caveat:** the live environment is pinned to `Test` and `appsettings` are excluded from the FTP deploy, so only the workflow patch step reaches `/wwwroot`. Changing the repo file alone will **not** change production behaviour.

---

## 4. Product decisions required before coding

These are not technical unknowns — the prototype itself flags them.

| # | Decision | Prototype evidence | Blocks |
|---|---|---|---|
| D1 | **Refund treatment when the teacher cancels** | `tsess`: *"Refund treatment for teacher cancellation is a pending product rule"* | Phase 3 wallet shape |
| D2 | **Commission model** — flat? per instructor? per tier? | `tearn`: *"Commission % is configured in the backend, not hardcoded"* | Phase 3 |
| D3 | **Payment provider** — nothing is installed today | `hand` → *"Needs real backend: Payments"* | Phase 3 |
| D4 | **Video provider** — Zoom / Meet / Jitsi / Daily | `hand` → *"Needs real backend: video/live service"* | Phase 4 |
| D5 | **Are `TeachingLanguages` / `Dialects` actually in the product?** | `treg` step 3 collects them | Phase 0 data model |
| D6 | **Scope of the "Proposed, needs business approval" set** — affiliate, packages, referral rewards, trial lesson, recommendations, compare tutors, payouts, streaks | `hand` → explicit list | Phases 10–11 |

---

## 5. Implementation plan

Phases are ordered by **dependency**, not by UI prominence. Later phases are almost all derived aggregates of earlier ones — building the dashboard first would only produce fake numbers.

Repo conventions that apply to every phase:
- Clean Architecture + MediatR CQRS; `Result<T>` → `ApiResponse<T>` via `BaseController.FromResult`.
- **All** request validation — including DB-state checks — lives in the FluentValidation `AbstractValidator`, never in the handler. Handlers orchestrate only.
- **No hardcoded strings anywhere user-facing** (user rule, 2026-11-04). Every message and every exception carries a `LocalizationKeys.*` constant that has an entry in `LMS.Application/Localization/Resources/messages.en.json`. This is not cosmetic: `JsonLocalizationProvider.GetLocalizedString` **returns the key itself when the lookup misses** (`LMS.Application/Localization/JsonLocalizationProvider.cs:129`), and `CustomExceptionHandlerMiddleware` treats an exception's first string argument as a *key*, never as text — so a sentence passed there is echoed to the client untranslated, verbatim. The keys file and the resource file are currently 1:1 (248 = 248, no missing entry, no orphan, no blank); re-check both directions when adding a phase, and `throw new NotFoundException(typeof(T).Name, id)` (the entity/key overload) rather than the `(localizationKey, args)` one when there is no dedicated key.
- Every entity gets an `IEntityTypeConfiguration` class in `LMS.Persistence/Configurations` (auto-discovered); soft delete + audit come from `BaseEntity<Guid>`.
- **Never run `dotnet ef database update`** against the shared Development/Production database. Each phase ships a migration **plus** a hand-off `.sql` script in `docs/plans/` for the owner to apply.
- Build with `-p:OutputPath=bin/verify` to avoid `MSB3027/3021` from the VS debugger locking `bin/Debug`.
- **Every `DateTime` in the API is UTC** (user rule, 2026-11-04). Construct with `DateTime.UtcNow` only; `BaseEntity.AppNow` already does. Values are serialized by the globally registered `IsoUtcDateTimeConverter` → `2026-11-04T09:02:17Z`, matching the `format: "date-time"` (RFC 3339) the OpenAPI document already declares. Two traps to keep out: SQL Server `datetime2` comes back with `Kind = Unspecified`, and `ToUniversalTime()` on `Unspecified` assumes *local* — it shifted every stored timestamp by 2 h on this UTC+2 machine while looking correct on a UTC-pinned host; and a recurring weekly slot is `DayOfWeek` + `TimeSpan`, i.e. wall-clock, **not** an instant — it can only become a bookable `Session.StartsAt` once an instructor timezone exists (D2/D5).
  - **That converter only covers JSON bodies.** Verified 2026-11-04: no request in the API carries a `DateTime` today — every one in `LMS.Application/Features` is a response-DTO property or internal `DateTime.UtcNow` math, and `LMS.API/Controllers` has zero `DateTime` parameters. When Phase 1/2 adds `calendar?from&to` or `sessions?date=`, those bind through MVC's default binder — parsed in the **server's** local zone, never reaching `IsoUtcDateTimeConverter`. Decide the rule then: accept ISO 8601 with an explicit offset only, and normalize/assert `Kind` inside the validator like every other request rule.
    - **Resolved 2026-11-04 by avoiding the problem.** Every query/route date in the scheduling surface is a `DateOnly` (`calendar?from&to`, `bookable-slots?from&to`, `my/calendar/day`) — a calendar date has no zone to lose, and the server's zone never enters the picture. The instants stay inside JSON bodies, where the converter does run: `RequestRescheduleCommand.FromUtc`/`ToUtc` are the only `DateTime` inputs added, and they are stored as UTC without re-conversion.
- No invented columns: a field exists only if a prototype screen or an existing endpoint actually collects it.
- UI eligibility pre-checks (e.g. the 12 h / 24 h lock) are computed in the frontend from data the GET already returns; the validator remains the real guard.

---

### Phase 0 — Security hardening + instructor self-service

*Smallest phase, unblocks everything, and closes a live vulnerability. Start here.*

**Entities / model**
- [ ] ~~`Instructor.Timezone` (string, IANA id)~~ — **rejected 2026-11-04**: the zone is derived from `Country.TimezoneId` via the instructor's profile country, falling back to the dial code of their phone number. No column, no human input. See `LMS.Application/Common/Helpers/CountryTimezoneResolver.cs`
- [ ] `Instructor.TeachingLanguages` / `Instructor.Dialects` as join entities (`InstructorLanguage`, `InstructorDialect`) — **not** CSV columns — *pending D5*
- [ ] `InstructorApplication` gains a `Draft` status (extend `InstructorApplicationStatus`) — *deferred by the user 2026-11-04*
- [x] `Instructor.HourlyRate` (live price) + `Instructor.UpdateProfile(...)`, and `InstructorApplication.Resubmit(actorUserId, hourlyRate)` which clears the stale review outcome

**Endpoints**
- [x] `GET  /api/v1/instructor-availability/my`
- [x] `POST /api/v1/instructor-availability/my/weekly-slots`
- [x] `PUT  /api/v1/instructor-availability/my/weekly-slots/{id}`
- [x] `DELETE /api/v1/instructor-availability/my/weekly-slots/{id}`
- [x] `GET  /api/v1/instructor-applications/my-status` → `{ Status, HourlyRate, ReviewNotes, ReviewedAt, IsActive, EmailConfirmed, CertificatesCount, HasIntroVideo, SubmittedAt }`
- [x] `POST /api/v1/instructor-applications/resubmit` — allowed only from `Rejected`
- [ ] `POST /api/v1/instructor-applications/draft` — save-and-continue-later
- [x] `GET  /api/v1/instructor-profile` and `PUT /api/v1/instructor-profile` — ships with specialization, about, years, education, LinkedIn and the 1-to-1 rate; **group price-per-seat and trial-lesson flag still missing** (no column invented ahead of Phase 1)
- [x] Split the existing admin gate: `InstructorAvailabilityAdminController` keeps `[RoleAuthorize(Admin)]`; the new `my/*` routes are `[Authorize] + [RoleAuthorize(Instructor)]`
- [x] The application self-service routes carry **`[Authorize]` with no role** — an applicant does not hold the `Instructor` role yet, so a role gate there would 403 the only users the feature exists for. Ownership is enforced by the caller's user id inside the query/validator

**Validation / tests**
- [x] Ownership assertion in every slot handler; a cross-instructor request must return 403
- [x] Validator: resubmit only from `Rejected` (`UpdateInstructorProfile` mirrors it: edits only from `Approved`, so the live rate cannot drift away from what the admin reviewed). Draft rule still pending the `Draft` status
- [x] Validator: `HourlyRate > 0` and `<= 10000`, bio floor 30 characters — copied from the register rules. `GroupRate` has no field yet
- [x] Validator: admin `approve` refuses an application whose email is not confirmed

- [x] Migration `20261104085131_AddInstructorSelfService` + `docs/plans/2026-11-04-instructor-self-service.sql` — **section [2] is not optional**: EF leaves every existing `Instructor.HourlyRate` at `0`, which ranks those instructors as the cheapest cards. The script copies each submitted snapshot into the live column; skip it only if you prefer those instructors to re-enter their rate. (He applies the SQL himself.)

---

### Phase 1 — Sessions & Booking

*The keystone. Nine later phases are derived from this one.*

**Entities**
- [x] `Session` — `InstructorId`, `StartsAtUtc`, `EndsAtUtc`, `Type` (`OneToOne` | `Group`), `Status` (`Pending` | `Confirmed` | `Completed` | `Cancelled` | `NoShow`), `Price`, `CancelledByUserId`, `CancelReason`, `SeatCapacity`, `SeatsBooked`. The learner side is the separate `SessionBooking` row (`SessionId`, `LearnerId`, `Status`, unique `(SessionId, LearnerId)`), not a `Session.LearnerId` column — a group needs many
- [x] `RescheduleRequest` — `SessionId`, `RequestedByUserId` + `RequestedByType`, `From`, `To`, `Status` (`Pending` | `Approved` | `Declined`), respond audit
- [x] `AvailabilityInstance` — dated rows generated from the weekly pattern; `AvailabilityException` was cut. This is the schema bridge between Phases 0 and 1

**Endpoints**
- [x] `GET  /api/v1/sessions?tab=upcoming|completed|cancelled` — the **teaching** list. The learner list is a second route, `GET /sessions/my-bookings?tab=`, because one route whose ownership rule changes with the caller's role is the bug in §2
- [x] `GET  /api/v1/sessions/{id}`
- [x] `POST /api/v1/sessions/{id}/cancel`
- [x] `POST /api/v1/sessions/{id}/confirm` · `/decline` — added with the approved `Pending → instructor confirms` model
- [x] `POST /api/v1/sessions/{id}/reschedule` → creates a `RescheduleRequest`, never mutates the session directly
- [x] `POST /api/v1/reschedule-requests/{id}/approve` · `.../decline`
- [x] `POST /api/v1/sessions` (learner booking side) with capacity + conflict guards — takes one `AvailabilityInstance`, so there is no duration to choose
- [x] `GET /api/v1/sessions/bookable-slots?instructorId=&from=&to=` — the dated picker the booking side needs

**Business rules to encode in validators**
- [x] **24 h teacher / 12 h student** cancel & reschedule lock, measured against `StartsAt` — both numbers live in `SchedulingOptions`, not in the validators
- [x] No double-booking: same `InstructorId` + overlapping time. Wider than the rule as written: `Pending` holds the time too, otherwise the slot a learner just requested stays sellable until the instructor answers
- [x] Blocking an availability day that has a confirmed session → rejected with *"reschedule it first"* (same rule for editing or deleting the weekly slot behind it)
- [ ] Group `SeatsBooked <= SeatCapacity`, never over-subscribed — the invariant is written (`HoldsSeat` counting, `ReserveSeat`/`ReleaseSeat`), but nothing can create a `Group` session yet, so it has no path to fail
- [x] A reschedule request that is already `Pending` for the same session → rejected
- [x] Approval preserves the duration and re-checks every conflict — the request validator rejects a proposed window of a different length, so approval cannot quietly resize the lesson
- [ ] Cancelling credits the refund exactly once (idempotency guard) — *shape pending D1*

- [x] Migration `20261104102902_AddSchedulingTables` + `docs/plans/2026-11-04-scheduling.sql` (and `2026-11-04-country-timezones.sql`, which is **not optional** — without a zone on the country rows nothing is ever materialized or bookable)

---

### Phase 2 — Availability v2

- [x] `VacationMode` toggle on `Instructor` — `PUT /instructor-availability/my/vacation-mode`. Hides from new bookings, **confirmed sessions stay** and follow the 24 h rule. Deliberately **not** on the instructor profile DTO
- [x] `BlockedDay` (dated) — no separate table: `AvailabilityInstance.IsOpen = false`, which the materializer never silently re-opens. `PUT /instructor-availability/my/calendar/day`
- [x] `GET /api/v1/instructor-availability/my/calendar?from&to` — week/month view (`Available` / `Blocked` / `Booked` per day, capped at 92 days)
- [x] Conflict check against `Session` before closing a slot — and before blocking a day
- [x] 15-minute break enforcement between sessions — for the instructor **and** for the learner, on booking and again on reschedule approval
- [x] Timezone resolution — **the backend anchors**, because the frontend cannot be trusted to map dialling codes: `Country.TimezoneId` resolved from the profile country, falling back to the phone dial code. The learner payload now carries `TimezoneId` so wall-clock slots can still be turned into instants client-side

---

### Phase 3 — Money: Payment · Wallet · Earnings · Payout

*Gated on D1, D2, D3.*

- [ ] `Payment`, `WalletTransaction`, `PayoutRequest`, `PlatformSetting(CommissionPercent)`
- [ ] Provider integration + webhook controller + signature verification
- [ ] `GET  /api/v1/instructor/earnings/summary` → gross / commission / net
- [ ] `GET  /api/v1/instructor/earnings/weekly` → the 4-bar trend
- [ ] `GET  /api/v1/instructor/wallet` → pending settlement / available
- [ ] `POST /api/v1/instructor/payout-requests`
- [ ] `PUT  /api/v1/instructor/payout-method` (the bank details shown in `tset`)
- [ ] Ledger invariant: every booking, refund and commission posts a balanced `WalletTransaction`

---

### Phase 4 — Live class

*Gated on D4 and on the `Permissions-Policy` change in §3.*

- [ ] SignalR hub + `Microsoft.AspNetCore.SignalR` package (currently zero references)
- [ ] `SessionParticipant` join logs — **Phase 6 attendance reads these**
- [ ] Provider integration (token/mint + webhook for recording)
- [ ] Private vs group mode, participant list, connection state
- [ ] In-session state marking: student no-show / network drop / teacher missed
- [ ] `GET /api/v1/sessions/{id}/summary` — duration, attendance, status
- [ ] Relax `camera=()` and `microphone=()` in the live environment

---

### Phase 5 — Feedback & reviews

- [ ] `FeedbackTemplate` + `FeedbackOption` — **clone the `OnboardingQuestion`/`Option` shape**, admin-managed, instructors cannot author questions
- [ ] `SessionFeedback` — answers per session, unique on (`InstructorId`, `StudentId`, `SessionDate`)
- [ ] `Review` — stars + text, permitted only for a learner with a `Completed` session; feeds the `Avg rating` metric
- [ ] Admin CRUD for templates + public read for both sides

---

### Phase 6 — Chat

- [ ] `Conversation`, `Message` + chat hub
- [ ] Status derived from the booking relationship: `Active` while an open booking exists, `Closed` otherwise
- [ ] **Closed conversation is read-only** — send rejected at the validator, reopened by a new booking
- [ ] Unread counters, search, privacy rules on what a student profile exposes

---

### Phase 7 — Notifications

- [ ] `Notification` + `NotificationPreference`
- [ ] Triggers: new confirmed booking, reschedule requested/approved/declined, session cancelled, feedback available, earnings updated
- [ ] `GET /api/v1/instructor/notifications` + `POST .../{id}/read`
- [ ] Email/push provider
- [ ] Per-type toggles surfaced in `tset`

---

### Phase 8 — Dashboard & insights

*Deliberately late — every metric is a derived aggregate of Phases 1–7.*

- [ ] `GET /api/v1/instructor/dashboard` → today's count, net this month, active students, hours taught
- [ ] `GET /api/v1/instructor/insights?period=week|month|quarter` → the 18 metrics from `REP`: completed sessions, teaching hours, net earnings, net per hour, completion rate, retention, new students, avg rating, reviews, bookings/week, cancellation rate, reschedule requests, on-time joins, group fill rate, feedback completion, repeat booking, chat response time, peak demand — each with a "vs previous" delta

---

### Phase 9 — Attendance & warnings

- [ ] `AttendanceRecord` derived from Phase 4 join logs
- [ ] `ConsecutiveMisses` counter with escalation 1 → warning, 2 → final warning, 3 → **account closure**
- [ ] Technical-failure queue: a miss is **not counted** until an admin confirms it; "clear (connection error)" resets without penalty
- [ ] `GET /api/v1/instructor/attendance` + history

---

### Phase 10 — Achievements & rank

- [ ] `AchievementDefinition` (admin-configurable points + thresholds), `InstructorAchievementProgress`, `Tier`
- [ ] The 10 achievements in `ACH`: login streak, no teacher reschedules 30 days, zero missed sessions, 10 reviews ≥ 4.8, 30 sessions completed, feedback within 24 h, reply within 1 h, 20 on-time joins, 3 group sessions hosted, 25 affiliate signups
- [ ] Tiers: Bronze 0 · Silver 100 · Gold 200 · Platinum 300
- [ ] Leaderboard (`Top tutors`) — verified platform data only

---

### Phase 11 — Affiliate + settings completion

*Gated on D6 — the prototype labels affiliate as "Proposed feature" with undefined reward terms.*

- [ ] `ReferralLink`, click / signup / booking tracking
- [ ] Remaining `tset` items: notification preferences, "Show profile publicly", support tickets

---

## 6. Dependency graph

```
Phase 0 (security + self-service)
   └─► Phase 1 (Sessions & Booking)  ◄── the keystone
          ├─► Phase 2 (Availability v2)
          ├─► Phase 3 (Money)          ◄── D1 D2 D3
          ├─► Phase 4 (Live class)     ◄── D4 + Permissions-Policy
          │      └─► Phase 9 (Attendance) ─► Phase 10 (Achievements)
          ├─► Phase 5 (Feedback & Reviews) ─► Phase 10
          ├─► Phase 6 (Chat) ─► Phase 10
          └─► Phase 7 (Notifications)
                 └─► Phase 8 (Dashboard & Insights) ◄── needs 1–7
                        Phase 11 (Affiliate)        ◄── D6
```

**Recommended sequencing:** Phase 0 immediately (it is a vulnerability, not a feature) → Phase 1 → then Phases 2/3/5 in parallel → Phase 4 → Phases 6–9 → Phases 10–11 last.

---

## 7. What is genuinely reusable today

Nothing needs to be thrown away. Three existing assets carry directly into the plan:

| Asset | Reuse in |
|---|---|
| `OnboardingQuestion` / `OnboardingOption` / `OnboardingAnswer` (admin-managed EAV) | Phase 5 `FeedbackTemplate` — structurally identical |
| `GetInstructorsForLearnerQuery` filters (`MinPrice`, `MaxPrice`, `Specialization`, `CountryId`, `Day`, `After`, `Before`) | Phase 1 booking search + Phase 2 calendar filtering |
| `HelpCenterSeeder` "For Instructors" category | Documents the *current* product contract — must be rewritten once Phase 1 lands, because it currently promises only registration, approval, rate and weekly windows |

---

## 8. Documentation note

`docs/plans/` and `docs/specs/` are empty in the working tree. Six design documents — including `2026-11-03-instructor-availability.md`, which carries the availability scope decisions this plan inherits — were deleted in commit `5c237f2`. They are recoverable with `git show 5c237f2^:docs/plans/<file>`. Restore them before Phase 1, since scope decision #2 (naive wall-clock times, frontend resolves the timezone) directly constrains the `Session` schema.
