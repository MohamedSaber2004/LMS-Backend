# Teacher App Phase 3: Sessions, Rescheduling & 24h Cutoff Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Implement Phase 3 of the Teacher App Backend: 24h cutoff enforcement for cancellations and rescheduling, single-pending reschedule requests, request withdrawal, session participants roster, and vacation conflict checking.

**Architecture:** ASP.NET Core Web API with Clean Architecture / CQRS using MediatR, FluentValidation, EF Core with SQL Server, ASP.NET Identity, and localized response messaging.

**Tech Stack:** .NET 8, C#, EF Core, MediatR, FluentValidation, Asp.Versioning.

---

## File Structure Map

### LMS.Domain
- Modify: `LMS.Domain/Enums/RescheduleRequestStatus.cs` - Add `Withdrawn = 3`.
- Modify: `LMS.Domain/Entities/RescheduleRequest.cs` - Add `Withdraw(Guid actorUserId)` domain method.

### LMS.Application
- Modify: `LMS.Application/Localization/LocalizationKeys.cs` - Add localization keys for withdrawal, participants, and vacation conflicts.
- Modify: `LMS.Application/Localization/Messages.resx` & `Messages.ar.resx` - Add localized messages in EN and AR.
- Create: `LMS.Application/Features/Sessions/DTOs/SessionParticipantDto.cs` - DTO for session roster participants.
- Create: `LMS.Application/Features/Sessions/DTOs/TeacherRescheduleRequestDto.cs` - DTO for teacher reschedule requests list.
- Create: `LMS.Application/Features/Availability/DTOs/VacationConflictResultDto.cs` - DTO for vacation conflict check results.
- Create: `LMS.Application/Features/Sessions/Commands/WithdrawReschedule/WithdrawRescheduleRequestCommand.cs` - MediatR Command.
- Create: `LMS.Application/Features/Sessions/Commands/WithdrawReschedule/WithdrawRescheduleRequestCommandValidator.cs` - Validator.
- Create: `LMS.Application/Features/Sessions/Commands/WithdrawReschedule/WithdrawRescheduleRequestCommandHandler.cs` - Handler.
- Create: `LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/GetTeacherRescheduleRequestsQuery.cs` - MediatR Query.
- Create: `LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/GetTeacherRescheduleRequestsQueryValidator.cs` - Validator.
- Create: `LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/GetTeacherRescheduleRequestsQueryHandler.cs` - Handler.
- Create: `LMS.Application/Features/Sessions/Queries/GetSessionParticipants/GetSessionParticipantsQuery.cs` - MediatR Query.
- Create: `LMS.Application/Features/Sessions/Queries/GetSessionParticipants/GetSessionParticipantsQueryValidator.cs` - Validator.
- Create: `LMS.Application/Features/Sessions/Queries/GetSessionParticipants/GetSessionParticipantsQueryHandler.cs` - Handler.
- Create: `LMS.Application/Features/Availability/Queries/CheckVacationConflicts/CheckVacationConflictsQuery.cs` - MediatR Query.
- Create: `LMS.Application/Features/Availability/Queries/CheckVacationConflicts/CheckVacationConflictsQueryValidator.cs` - Validator.
- Create: `LMS.Application/Features/Availability/Queries/CheckVacationConflicts/CheckVacationConflictsQueryHandler.cs` - Handler.

### LMS.API
- Modify: `LMS.API/Routes/V1/ApiRoutes.cs` - Add `Teacher` routes for sessions, reschedule requests, and availability.
- Create: `LMS.API/Controllers/V1/TeacherSessionsController.cs` - Dedicated controller for `/api/v1/teacher/sessions`.
- Create: `LMS.API/Controllers/V1/TeacherRescheduleRequestsController.cs` - Dedicated controller for `/api/v1/teacher/reschedule-requests`.
- Create: `LMS.API/Controllers/V1/TeacherAvailabilityController.cs` - Dedicated controller for `/api/v1/teacher/availability`.

---

## Tasks

### Task 1: Domain & Model Enhancements (`RescheduleRequestStatus.Withdrawn` & `Withdraw` method)

**Files:**
- Modify: `LMS.Domain/Enums/RescheduleRequestStatus.cs`
- Modify: `LMS.Domain/Entities/RescheduleRequest.cs:40-60`

- [x] **Step 1: Add `Withdrawn` status to enum**

Edit `LMS.Domain/Enums/RescheduleRequestStatus.cs`:
```csharp
namespace LMS.Domain.Enums
{
    public enum RescheduleRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Declined = 2,
        Withdrawn = 3
    }
}
```

- [x] **Step 2: Add `Withdraw` method to `RescheduleRequest` entity**

Add `Withdraw` method to `LMS.Domain/Entities/RescheduleRequest.cs`:
```csharp
public void Withdraw(Guid actorUserId)
{
    Status = RescheduleRequestStatus.Withdrawn;
    RespondedByUserId = actorUserId;
    RespondedAt = AppNow;
}
```

- [x] **Step 3: Build `LMS.Domain` to verify compilation**

Run: `dotnet build LMS.Domain/LMS.Domain.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 4: Commit Domain changes**

```bash
git add LMS.Domain/Enums/RescheduleRequestStatus.cs LMS.Domain/Entities/RescheduleRequest.cs
git commit -m "feat(domain): add Withdrawn status and Withdraw method to RescheduleRequest"
```

---

### Task 2: Localization Keys and Resource Strings (EN & AR)

**Files:**
- Modify: `LMS.Application/Localization/LocalizationKeys.cs:250-280`
- Modify: `LMS.Application/Localization/Messages.resx`
- Modify: `LMS.Application/Localization/Messages.ar.resx`

- [x] **Step 1: Add localization keys in `LocalizationKeys.cs`**

Under `LocalizationKeys.Sessions`, add:
```csharp
public const string RescheduleWithdrawnSuccess = "Sessions.RescheduleWithdrawnSuccess";
public const string RescheduleNotWithdrawable = "Sessions.RescheduleNotWithdrawable";
public const string RescheduleNotOwner = "Sessions.RescheduleNotOwner";
public const string RescheduleRequestsRetrieved = "Sessions.RescheduleRequestsRetrieved";
public const string ParticipantsRetrieved = "Sessions.ParticipantsRetrieved";
```

Under `LocalizationKeys.Availability`, add:
```csharp
public const string VacationConflictsChecked = "Availability.VacationConflictsChecked";
```

- [x] **Step 2: Add English and Arabic message entries in `.resx` files**

In `Messages.resx`:
- `Sessions.RescheduleWithdrawnSuccess`: `The reschedule request has been successfully withdrawn.`
- `Sessions.RescheduleNotWithdrawable`: `Only pending reschedule requests can be withdrawn.`
- `Sessions.RescheduleNotOwner`: `You can only withdraw reschedule requests initiated by you.`
- `Sessions.RescheduleRequestsRetrieved`: `Reschedule requests retrieved successfully.`
- `Sessions.ParticipantsRetrieved`: `Session participants retrieved successfully.`
- `Availability.VacationConflictsChecked`: `Vacation conflicts check completed.`

In `Messages.ar.resx`:
- `Sessions.RescheduleWithdrawnSuccess`: `تم سحب طلب إعادة الجدولة بنجاح.`
- `Sessions.RescheduleNotWithdrawable`: `يمكن فقط سحب طلبات إعادة الجدولة المعلقة.`
- `Sessions.RescheduleNotOwner`: `يمكنك فقط سحب طلبات إعادة الجدولة التي قمت بإنشائها.`
- `Sessions.RescheduleRequestsRetrieved`: `تم جلب طلبات إعادة الجدولة بنجاح.`
- `Sessions.ParticipantsRetrieved`: `تم جلب المشاركين في الحصة بنجاح.`
- `Availability.VacationConflictsChecked`: `تم فحص تعارضات الإجازة بنجاح.`

- [x] **Step 3: Build `LMS.Application` to verify keys**

Run: `dotnet build LMS.Application/LMS.Application.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 4: Commit Localization changes**

```bash
git add LMS.Application/Localization/
git commit -m "feat(localization): add localization keys and translations for Phase 3"
```

---

### Task 3: Data Transfer Objects (DTOs) for Phase 3

**Files:**
- Create: `LMS.Application/Features/Sessions/DTOs/SessionParticipantDto.cs`
- Create: `LMS.Application/Features/Sessions/DTOs/TeacherRescheduleRequestDto.cs`
- Create: `LMS.Application/Features/Availability/DTOs/VacationConflictResultDto.cs`

- [x] **Step 1: Create `SessionParticipantDto.cs`**

File: `LMS.Application/Features/Sessions/DTOs/SessionParticipantDto.cs`:
```csharp
using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class SessionParticipantDto
    {
        public Guid BookingId { get; set; }
        public Guid LearnerId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime BookedAt { get; set; }
        public AttendanceStatus? AttendanceStatus { get; set; }
        public DateTime? JoinedAtUtc { get; set; }
        public DateTime? LeftAtUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public string? AttendanceNotes { get; set; }
    }
}
```

- [x] **Step 2: Create `TeacherRescheduleRequestDto.cs`**

File: `LMS.Application/Features/Sessions/DTOs/TeacherRescheduleRequestDto.cs`:
```csharp
using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class TeacherRescheduleRequestDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string? SessionTitle { get; set; }
        public DateTime CurrentStartsAtUtc { get; set; }
        public DateTime CurrentEndsAtUtc { get; set; }
        public DateTime ProposedStartsAtUtc { get; set; }
        public DateTime ProposedEndsAtUtc { get; set; }
        public RescheduleRequestStatus Status { get; set; }
        public UserType RequestedByType { get; set; }
        public bool IsRequestedByMe { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<SessionParticipantSummaryDto> Participants { get; set; } = [];
    }

    public class SessionParticipantSummaryDto
    {
        public Guid LearnerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}
```

- [x] **Step 3: Create `VacationConflictResultDto.cs`**

File: `LMS.Application/Features/Availability/DTOs/VacationConflictResultDto.cs`:
```csharp
using LMS.Domain.Enums;

namespace LMS.Application.Features.Availability.DTOs
{
    public class VacationConflictResultDto
    {
        public bool HasConflicts => ConflictCount > 0;
        public int ConflictCount => ConflictedSessions.Count;
        public bool CanProceed => ConflictCount == 0;
        public IReadOnlyCollection<VacationConflictedSessionDto> ConflictedSessions { get; set; } = [];
    }

    public class VacationConflictedSessionDto
    {
        public Guid SessionId { get; set; }
        public string? Title { get; set; }
        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
        public SessionStatus Status { get; set; }
        public SessionType Type { get; set; }
        public int SeatsBooked { get; set; }
        public int SeatCapacity { get; set; }
        public IReadOnlyCollection<string> LearnerNames { get; set; } = [];
    }
}
```

- [x] **Step 4: Build `LMS.Application` to verify DTOs**

Run: `dotnet build LMS.Application/LMS.Application.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 5: Commit DTOs**

```bash
git add LMS.Application/Features/Sessions/DTOs/ LMS.Application/Features/Availability/DTOs/
git commit -m "feat(application): add DTOs for Phase 3 participants, reschedule requests, and vacation conflicts"
```

---

### Task 4: Reschedule Request Withdrawal Feature (`WithdrawRescheduleRequestCommand`)

**Files:**
- Create: `LMS.Application/Features/Sessions/Commands/WithdrawReschedule/WithdrawRescheduleRequestCommand.cs`
- Create: `LMS.Application/Features/Sessions/Commands/WithdrawReschedule/WithdrawRescheduleRequestCommandValidator.cs`
- Create: `LMS.Application/Features/Sessions/Commands/WithdrawReschedule/WithdrawRescheduleRequestCommandHandler.cs`

- [x] **Step 1: Create `WithdrawRescheduleRequestCommand.cs`**

```csharp
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.WithdrawReschedule
{
    public class WithdrawRescheduleRequestCommand : IRequest<Result<RescheduleRequestDto>>
    {
        public Guid Id { get; set; }
    }
}
```

- [x] **Step 2: Create `WithdrawRescheduleRequestCommandValidator.cs`**

```csharp
using FluentValidation;
using FluentValidation.Validators;
using LMS.API.Services;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.WithdrawReschedule
{
    public class WithdrawRescheduleRequestCommandValidator : AbstractValidator<WithdrawRescheduleRequestCommand>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;

        public WithdrawRescheduleRequestCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;

            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.RescheduleIdRequired].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            WithdrawRescheduleRequestCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .Include(r => r.Session)
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (reschedule is null)
            {
                return;
            }

            if (reschedule.Status != RescheduleRequestStatus.Pending)
            {
                context.AddFailure(
                    nameof(WithdrawRescheduleRequestCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleNotWithdrawable].Value);
                return;
            }

            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUser.UserId, cancellationToken);

            if (reschedule.Session.InstructorId != instructorId ||
                reschedule.RequestedByType != UserType.Instructor ||
                reschedule.RequestedByUserId != _currentUser.UserId)
            {
                context.AddFailure(
                    nameof(WithdrawRescheduleRequestCommand.Id),
                    _localizer[LocalizationKeys.Sessions.RescheduleNotOwner].Value);
            }
        }
    }
}
```

- [x] **Step 3: Create `WithdrawRescheduleRequestCommandHandler.cs`**

```csharp
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.WithdrawReschedule
{
    public class WithdrawRescheduleRequestCommandHandler
        : IRequestHandler<WithdrawRescheduleRequestCommand, Result<RescheduleRequestDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public WithdrawRescheduleRequestCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<RescheduleRequestDto>> Handle(
            WithdrawRescheduleRequestCommand request,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.RescheduleNotFound);

            reschedule.Withdraw(_currentUserService.UserId);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<RescheduleRequestDto>.Success(
                SessionMapper.ToRescheduleDto(reschedule),
                _localizer[LocalizationKeys.Sessions.RescheduleWithdrawnSuccess].Value);
        }
    }
}
```

- [x] **Step 4: Build `LMS.Application` to verify compilation**

Run: `dotnet build LMS.Application/LMS.Application.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 5: Commit Reschedule Withdrawal Feature**

```bash
git add LMS.Application/Features/Sessions/Commands/WithdrawReschedule/
git commit -m "feat(application): add withdraw reschedule request command, validator and handler"
```

---

### Task 5: Teacher Reschedule Requests Query (`GetTeacherRescheduleRequestsQuery`)

**Files:**
- Create: `LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/GetTeacherRescheduleRequestsQuery.cs`
- Create: `LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/GetTeacherRescheduleRequestsQueryValidator.cs`
- Create: `LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/GetTeacherRescheduleRequestsQueryHandler.cs`

- [x] **Step 1: Create `GetTeacherRescheduleRequestsQuery.cs`**

```csharp
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests
{
    public class GetTeacherRescheduleRequestsQuery : IRequest<Result<PagginatedResult<TeacherRescheduleRequestDto>>>
    {
        public RescheduleRequestStatus? Status { get; set; } = RescheduleRequestStatus.Pending;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
```

- [x] **Step 2: Create `GetTeacherRescheduleRequestsQueryValidator.cs`**

```csharp
using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests
{
    public class GetTeacherRescheduleRequestsQueryValidator : AbstractValidator<GetTeacherRescheduleRequestsQuery>
    {
        public GetTeacherRescheduleRequestsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.PageNumber)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Sessions.PageNumberInvalid].Value);

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage(localizer[LocalizationKeys.Sessions.PageSizeInvalid].Value);
        }
    }
}
```

- [x] **Step 3: Create `GetTeacherRescheduleRequestsQueryHandler.cs`**

```csharp
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests
{
    public class GetTeacherRescheduleRequestsQueryHandler
        : IRequestHandler<GetTeacherRescheduleRequestsQuery, Result<PagginatedResult<TeacherRescheduleRequestDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetTeacherRescheduleRequestsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<TeacherRescheduleRequestDto>>> Handle(
            GetTeacherRescheduleRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var query = _context.RescheduleRequests
                .AsNoTracking()
                .Include(r => r.Session)
                    .ThenInclude(s => s.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                        .ThenInclude(b => b.Learner)
                            .ThenInclude(l => l.User)
                .Where(r => r.Session.InstructorId == instructorId);

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new TeacherRescheduleRequestDto
                {
                    Id = r.Id,
                    SessionId = r.SessionId,
                    SessionTitle = r.Session.Title,
                    CurrentStartsAtUtc = r.Session.StartsAtUtc,
                    CurrentEndsAtUtc = r.Session.EndsAtUtc,
                    ProposedStartsAtUtc = r.FromUtc,
                    ProposedEndsAtUtc = r.ToUtc,
                    Status = r.Status,
                    RequestedByType = r.RequestedByType,
                    IsRequestedByMe = r.RequestedByUserId == _currentUserService.UserId,
                    CreatedAt = r.CreatedAt,
                    Participants = r.Session.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
                        .Select(b => new SessionParticipantSummaryDto
                        {
                            LearnerId = b.LearnerId,
                            FullName = b.Learner.User.FullName,
                            ProfilePictureUrl = b.Learner.User.ProfilePictureUrl
                        }).ToList()
                })
                .ToListAsync(cancellationToken);

            var paged = new PagginatedResult<TeacherRescheduleRequestDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagginatedResult<TeacherRescheduleRequestDto>>.Success(
                paged,
                _localizer[LocalizationKeys.Sessions.RescheduleRequestsRetrieved].Value);
        }
    }
}
```

- [x] **Step 4: Build `LMS.Application` to verify compilation**

Run: `dotnet build LMS.Application/LMS.Application.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 5: Commit Teacher Reschedule Requests Query**

```bash
git add LMS.Application/Features/Sessions/Queries/GetTeacherRescheduleRequests/
git commit -m "feat(application): add get teacher reschedule requests query and handler"
```

---

### Task 6: Session Participants Query (`GetSessionParticipantsQuery`)

**Files:**
- Create: `LMS.Application/Features/Sessions/Queries/GetSessionParticipants/GetSessionParticipantsQuery.cs`
- Create: `LMS.Application/Features/Sessions/Queries/GetSessionParticipants/GetSessionParticipantsQueryValidator.cs`
- Create: `LMS.Application/Features/Sessions/Queries/GetSessionParticipants/GetSessionParticipantsQueryHandler.cs`

- [x] **Step 1: Create `GetSessionParticipantsQuery.cs`**

```csharp
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetSessionParticipants
{
    public class GetSessionParticipantsQuery : IRequest<Result<IReadOnlyCollection<SessionParticipantDto>>>
    {
        public Guid SessionId { get; set; }
    }
}
```

- [x] **Step 2: Create `GetSessionParticipantsQueryValidator.cs`**

```csharp
using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetSessionParticipants
{
    public class GetSessionParticipantsQueryValidator : AbstractValidator<GetSessionParticipantsQuery>
    {
        public GetSessionParticipantsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Sessions.SessionIdRequired].Value);
        }
    }
}
```

- [x] **Step 3: Create `GetSessionParticipantsQueryHandler.cs`**

```csharp
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Queries.GetSessionParticipants
{
    public class GetSessionParticipantsQueryHandler
        : IRequestHandler<GetSessionParticipantsQuery, Result<IReadOnlyCollection<SessionParticipantDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetSessionParticipantsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<SessionParticipantDto>>> Handle(
            GetSessionParticipantsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var session = await _context.Sessions
                .AsNoTracking()
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .Include(s => s.Attendances)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            if (session.InstructorId != instructorId)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            var participants = session.Bookings.Select(booking =>
            {
                var attendance = session.Attendances.FirstOrDefault(a => a.LearnerId == booking.LearnerId);
                return new SessionParticipantDto
                {
                    BookingId = booking.Id,
                    LearnerId = booking.LearnerId,
                    UserId = booking.Learner.UserId,
                    FullName = booking.Learner.User.FullName,
                    ProfilePictureUrl = booking.Learner.User.ProfilePictureUrl,
                    BookingStatus = booking.Status,
                    BookedAt = booking.CreatedAt,
                    AttendanceStatus = attendance?.Status,
                    JoinedAtUtc = attendance?.JoinedAtUtc,
                    LeftAtUtc = attendance?.LeftAtUtc,
                    DurationMinutes = attendance?.DurationMinutes,
                    AttendanceNotes = attendance?.Notes
                };
            }).ToList();

            return Result<IReadOnlyCollection<SessionParticipantDto>>.Success(
                participants,
                _localizer[LocalizationKeys.Sessions.ParticipantsRetrieved].Value);
        }
    }
}
```

- [x] **Step 4: Build `LMS.Application` to verify compilation**

Run: `dotnet build LMS.Application/LMS.Application.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 5: Commit Session Participants Feature**

```bash
git add LMS.Application/Features/Sessions/Queries/GetSessionParticipants/
git commit -m "feat(application): add get session participants query and handler"
```

---

### Task 7: Vacation Conflict Check Feature (`CheckVacationConflictsQuery`)

**Files:**
- Create: `LMS.Application/Features/Availability/Queries/CheckVacationConflicts/CheckVacationConflictsQuery.cs`
- Create: `LMS.Application/Features/Availability/Queries/CheckVacationConflicts/CheckVacationConflictsQueryValidator.cs`
- Create: `LMS.Application/Features/Availability/Queries/CheckVacationConflicts/CheckVacationConflictsQueryHandler.cs`

- [x] **Step 1: Create `CheckVacationConflictsQuery.cs`**

```csharp
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Queries.CheckVacationConflicts
{
    public class CheckVacationConflictsQuery : IRequest<Result<VacationConflictResultDto>>
    {
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}
```

- [x] **Step 2: Create `CheckVacationConflictsQueryValidator.cs`**

```csharp
using FluentValidation;
using LMS.Application.Localization;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.CheckVacationConflicts
{
    public class CheckVacationConflictsQueryValidator : AbstractValidator<CheckVacationConflictsQuery>
    {
        public CheckVacationConflictsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(query => query)
                .Must(q => !q.FromUtc.HasValue || !q.ToUtc.HasValue || q.FromUtc <= q.ToUtc)
                .WithMessage(localizer[LocalizationKeys.Availability.DateRangeInvalid].Value);
        }
    }
}
```

- [x] **Step 3: Create `CheckVacationConflictsQueryHandler.cs`**

```csharp
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Availability.Queries.CheckVacationConflicts
{
    public class CheckVacationConflictsQueryHandler
        : IRequestHandler<CheckVacationConflictsQuery, Result<VacationConflictResultDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CheckVacationConflictsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<VacationConflictResultDto>> Handle(
            CheckVacationConflictsQuery request,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var startThreshold = request.FromUtc ?? DateTime.UtcNow;

            var query = _context.Sessions
                .AsNoTracking()
                .Include(s => s.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .Where(s => s.InstructorId == instructorId
                    && (s.Status == SessionStatus.Confirmed || s.Status == SessionStatus.Pending)
                    && s.EndsAtUtc > startThreshold);

            if (request.ToUtc.HasValue)
            {
                query = query.Where(s => s.StartsAtUtc < request.ToUtc.Value);
            }

            var conflictedSessions = await query
                .OrderBy(s => s.StartsAtUtc)
                .Select(s => new VacationConflictedSessionDto
                {
                    SessionId = s.Id,
                    Title = s.Title,
                    StartsAtUtc = s.StartsAtUtc,
                    EndsAtUtc = s.EndsAtUtc,
                    Status = s.Status,
                    Type = s.Type,
                    SeatsBooked = s.SeatsBooked,
                    SeatCapacity = s.SeatCapacity,
                    LearnerNames = s.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
                        .Select(b => b.Learner.User.FullName)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var result = new VacationConflictResultDto
            {
                ConflictedSessions = conflictedSessions
            };

            return Result<VacationConflictResultDto>.Success(
                result,
                _localizer[LocalizationKeys.Availability.VacationConflictsChecked].Value);
        }
    }
}
```

- [x] **Step 4: Build `LMS.Application` to verify compilation**

Run: `dotnet build LMS.Application/LMS.Application.csproj`
Expected: Build succeeded with 0 warnings and 0 errors.

- [x] **Step 5: Commit Vacation Conflict Feature**

```bash
git add LMS.Application/Features/Availability/Queries/CheckVacationConflicts/
git commit -m "feat(application): add vacation conflicts check query and handler"
```

---

### Task 8: API Routes and Teacher Controllers

**Files:**
- Modify: `LMS.API/Routes/V1/ApiRoutes.cs`
- Create: `LMS.API/Controllers/V1/TeacherSessionsController.cs`
- Create: `LMS.API/Controllers/V1/TeacherRescheduleRequestsController.cs`
- Create: `LMS.API/Controllers/V1/TeacherAvailabilityController.cs`

- [x] **Step 1: Define `ApiRoutes.Teacher` in `ApiRoutes.cs`**

Edit `LMS.API/Routes/V1/ApiRoutes.cs`:
```csharp
        public static class Teacher
        {
            public const string Base = BaseRoutes.Base + "/teacher";

            public static class Sessions
            {
                public const string Base = Teacher.Base + "/sessions";
                public const string Cancel = "{id:guid}/cancel";
                public const string RescheduleRequests = "{id:guid}/reschedule-requests";
                public const string Participants = "{id:guid}/participants";
            }

            public static class RescheduleRequests
            {
                public const string Base = Teacher.Base + "/reschedule-requests";
                public const string GetAll = "";
                public const string Withdraw = "{id:guid}/withdraw";
            }

            public static class Availability
            {
                public const string Base = Teacher.Base + "/availability";
                public const string CheckVacationConflicts = "vacation/check-conflicts";
            }
        }
```

- [x] **Step 2: Create `TeacherSessionsController.cs`**

File: `LMS.API/Controllers/V1/TeacherSessionsController.cs`
Endpoints:
- `POST /api/v1/teacher/sessions/{id}/cancel` -> sends `CancelSessionCommand`
- `POST /api/v1/teacher/sessions/{id}/reschedule-requests` -> sends `RequestRescheduleCommand`
- `GET /api/v1/teacher/sessions/{id}/participants` -> sends `GetSessionParticipantsQuery`

```csharp
using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.CancelSession;
using LMS.Application.Features.Sessions.Commands.RequestReschedule;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetSessionParticipants;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Sessions.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherSessionsController : BaseController
    {
        [HttpPost(ApiRoutes.Teacher.Sessions.Cancel)]
        [ProducesResponseType(typeof(ApiResponse<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelSessionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        [HttpPost(ApiRoutes.Teacher.Sessions.RescheduleRequests)]
        [ProducesResponseType(typeof(ApiResponse<RescheduleRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestReschedule(
            [FromRoute] Guid id,
            [FromBody] RequestRescheduleCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        [HttpGet(ApiRoutes.Teacher.Sessions.Participants)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SessionParticipantDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetParticipants(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSessionParticipantsQuery { SessionId = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
```

- [x] **Step 3: Create `TeacherRescheduleRequestsController.cs`**

File: `LMS.API/Controllers/V1/TeacherRescheduleRequestsController.cs`
Endpoints:
- `GET /api/v1/teacher/reschedule-requests?status=pending` -> sends `GetTeacherRescheduleRequestsQuery`
- `POST /api/v1/teacher/reschedule-requests/{id}/withdraw` -> sends `WithdrawRescheduleRequestCommand`

```csharp
using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.Commands.WithdrawReschedule;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.RescheduleRequests.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherRescheduleRequestsController : BaseController
    {
        [HttpGet(ApiRoutes.Teacher.RescheduleRequests.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<TeacherRescheduleRequestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetTeacherRescheduleRequestsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        [HttpPost(ApiRoutes.Teacher.RescheduleRequests.Withdraw)]
        [ProducesResponseType(typeof(ApiResponse<RescheduleRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Withdraw(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new WithdrawRescheduleRequestCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
```

- [x] **Step 4: Create `TeacherAvailabilityController.cs`**

File: `LMS.API/Controllers/V1/TeacherAvailabilityController.cs`
Endpoint:
- `POST /api/v1/teacher/availability/vacation/check-conflicts` -> sends `CheckVacationConflictsQuery`

```csharp
using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using LMS.Application.Features.Availability.Queries.CheckVacationConflicts;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Availability.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherAvailabilityController : BaseController
    {
        [HttpPost(ApiRoutes.Teacher.Availability.CheckVacationConflicts)]
        [ProducesResponseType(typeof(ApiResponse<VacationConflictResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CheckVacationConflicts(
            [FromBody] CheckVacationConflictsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }
    }
}
```

- [x] **Step 5: Build solution to verify full integration**

Run: `dotnet build`
Expected: 0 Warning(s), 0 Error(s).

- [x] **Step 6: Commit Controllers and Routes**

```bash
git add LMS.API/Routes/V1/ApiRoutes.cs LMS.API/Controllers/V1/Teacher*
git commit -m "feat(api): add teacher controllers and endpoints for Phase 3"
```

---

### Task 9: Solution Build, Verification & Documentation Update

**Files:**
- Modify: `docs/TEACHER-APP-BACKEND-IMPLEMENTATION-PLAN.md` (check off Phase 3 status or mark completed)

- [x] **Step 1: Run full clean and build**

Run: `dotnet build LMS.slnx`
Expected: Build succeeded with 0 errors and 0 warnings.

- [x] **Step 2: Verify all 6 Phase 3 Endpoints in Swagger / Routes**

Verify routes registered:
1. `POST /api/v1/teacher/sessions/{id}/cancel`
2. `POST /api/v1/teacher/sessions/{id}/reschedule-requests`
3. `GET /api/v1/teacher/reschedule-requests?status=pending`
4. `POST /api/v1/teacher/reschedule-requests/{id}/withdraw`
5. `GET /api/v1/teacher/sessions/{id}/participants`
6. `POST /api/v1/teacher/availability/vacation/check-conflicts`

- [x] **Step 3: Commit Documentation and Final State**

```bash
git add docs/TEACHER-APP-BACKEND-IMPLEMENTATION-PLAN.md
git commit -m "docs: update teacher backend implementation plan with Phase 3 completion status"
```

