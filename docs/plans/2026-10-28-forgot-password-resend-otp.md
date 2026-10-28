# Forgot Password & Resend OTP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Forgot Password and Resend OTP commands, handlers, validators, and API endpoints, enforcing a strict 30-second per-user rate limit restriction on Resend OTP.

**Architecture:** Clean Architecture with MediatR CQRS handlers, FluentValidation, MailKit for SMTP delivery, IMemoryCache for per-user rate limiting, and ASP.NET Core API versioned controllers.

**Tech Stack:** .NET 8, ASP.NET Core, MediatR, FluentValidation, MailKit/MimeKit, Microsoft.Extensions.Caching.Memory, Microsoft.AspNetCore.Identity.

---

### Task 1: Domain & Localization Updates

**Files:**
- Modify: `LMS.Domain/Entities/ApplicationUser.cs`
- Modify: `LMS.Application/Localization/LocalizationKeys.cs`
- Modify: `LMS.Application/Localization/Resources/messages.en.json`

- [ ] **Step 1: Add domain methods to ApplicationUser**
  Add `SetVerificationCode(string code, DateTime expiry)` and `ClearVerificationCode()` to `ApplicationUser.cs`.
- [ ] **Step 2: Add localization keys**
  Add `OtpSentSuccess`, `OtpResentSuccess`, `OtpRateLimited`, and `UserNotFound` constants to `LocalizationKeys.cs`.
- [ ] **Step 3: Add English translations**
  Add corresponding messages to `messages.en.json`.
- [ ] **Step 4: Verify build**
  Run: `dotnet build LMS.Domain/LMS.Domain.csproj; dotnet build LMS.Application/LMS.Application.csproj`

---

### Task 2: Email Service & OTP Rate Limiter Service

**Files:**
- Create: `LMS.Application/Common/Interfaces/IEmailService.cs`
- Create: `LMS.Infrastructure/Services/Email/EmailService.cs`
- Create: `LMS.Application/Common/Interfaces/IOtpRateLimiter.cs`
- Create: `LMS.Infrastructure/Services/Authentication/OtpRateLimiter.cs`
- Modify: `LMS.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Create IEmailService interface**
  Define `Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);`
- [ ] **Step 2: Implement EmailService with MailKit**
  Connect to SMTP host from `EmailSettings`, authenticate, send `MimeMessage` with HTML body.
- [ ] **Step 3: Create IOtpRateLimiter interface**
  Define `(bool IsAllowed, int RemainingSeconds) CheckAndRecord(string email, TimeSpan cooldown);`
- [ ] **Step 4: Implement OtpRateLimiter with IMemoryCache**
  Key cache by normalized email, calculate remaining seconds if cooldown is active, record new timestamp when allowed.
- [ ] **Step 5: Register services in LMS.Infrastructure/DependencyInjection.cs**
  Register `IEmailService` as `Scoped` and `IOtpRateLimiter` as `Singleton`.
- [ ] **Step 6: Verify build**
  Run: `dotnet build LMS.Infrastructure/LMS.Infrastructure.csproj`

---

### Task 3: ForgotPassword Feature

**Files:**
- Modify: `LMS.Application/Features/Authentication/Commands/ForgetPassword/ForgetPasswordCommand.cs`
- Modify: `LMS.Application/Features/Authentication/Commands/ForgetPassword/ForgetPasswordCommandValidator.cs`
- Modify: `LMS.Application/Features/Authentication/Commands/ForgetPassword/ForgetPasswordCommandHandler.cs`
- Modify: `LMS.Application/DependencyInjection.cs`

- [ ] **Step 1: Implement ForgetPasswordCommand**
  Define `Email` property and implement `IRequest<Result<string>>`.
- [ ] **Step 2: Implement ForgetPasswordCommandValidator**
  Validate non-empty email and valid email address syntax.
- [ ] **Step 3: Implement ForgetPasswordCommandHandler**
  Query user by email; if active and found, generate 6-digit OTP, update verification code, send email via `IEmailService`, record in rate limiter, return success Result.
- [ ] **Step 4: Register validator in LMS.Application/DependencyInjection.cs**
  Add `services.AddTransient<IValidator<ForgetPasswordCommand>, ForgetPasswordCommandValidator>();`
- [ ] **Step 5: Verify build**
  Run: `dotnet build LMS.Application/LMS.Application.csproj`

---

### Task 4: ResendOtp Feature with 30-Second Rate Limit

**Files:**
- Modify: `LMS.Application/Features/Authentication/Commands/ResendOtp/ResendOtpCommand.cs`
- Modify: `LMS.Application/Features/Authentication/Commands/ResendOtp/ResendOtpCommandValidator.cs`
- Modify: `LMS.Application/Features/Authentication/Commands/ResendOtp/ResendOtpCommandHandler.cs`
- Modify: `LMS.Application/DependencyInjection.cs`

- [ ] **Step 1: Implement ResendOtpCommand**
  Define `Email` property and implement `IRequest<Result<string>>`.
- [ ] **Step 2: Implement ResendOtpCommandValidator**
  Validate non-empty email and valid email address syntax.
- [ ] **Step 3: Implement ResendOtpCommandHandler with 30-second restriction**
  Check `IOtpRateLimiter`. If not allowed, throw `TooManyRequestsException` with remaining seconds. If allowed, find user, generate 6-digit OTP, update verification code, send email, return success Result.
- [ ] **Step 4: Register validator in LMS.Application/DependencyInjection.cs**
  Add `services.AddTransient<IValidator<ResendOtpCommand>, ResendOtpCommandValidator>();`
- [ ] **Step 5: Verify build**
  Run: `dotnet build LMS.Application/LMS.Application.csproj`

---

### Task 5: API Endpoints & Routes

**Files:**
- Modify: `LMS.API/Routes/V1/ApiRoutes.cs`
- Modify: `LMS.API/Controllers/V1/AuthenticationController.cs`
- Modify: `LMS.API/appsettings.Development.json` (add endpoint rate limit rules if needed)

- [ ] **Step 1: Add route constants to ApiRoutes.cs**
  Add `ForgotPassword = "forgot-password"` and `ResendOtp = "resend-otp"`.
- [ ] **Step 2: Add action methods to AuthenticationController.cs**
  Add `ForgotPassword` and `ResendOtp` actions returning `FromResult(result)`.
- [ ] **Step 3: Configure endpoint rate limiting in appsettings.Development.json**
  Add rules for forgot-password and resend-otp endpoints.
- [ ] **Step 4: Verify complete solution build**
  Run: `dotnet build`

---

### Task 6: End-to-End Verification

- [ ] **Step 1: Build the complete solution**
- [ ] **Step 2: Verify git status and changes**
