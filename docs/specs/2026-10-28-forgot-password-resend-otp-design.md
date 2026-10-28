# Forgot Password & Resend OTP Specification

## 1. Overview
This specification details the design and implementation of the **Forgot Password** and **Resend OTP** features in the LMS Backend API. It includes rate-limiting restrictions guaranteeing that a user can only request an OTP resend once every 30 seconds.

---

## 2. Endpoints & Routing

### 2.1 Routing Constants (`ApiRoutes.cs`)
In `LMS.API.Routes.V1.ApiRoutes.Authentication`:
- `ForgotPassword`: `"forgot-password"`
- `ResendOtp`: `"resend-otp"`

Base route: `/api/v1/authentication`

### 2.2 Endpoints (`AuthenticationController.cs`)
1. **Forgot Password**:
   - HTTP Verb: `POST`
   - Path: `/api/v1/authentication/forgot-password`
   - Request Body: `ForgotPasswordCommand` `{ "email": "string" }`
   - Responses:
     - `200 OK`: `ApiResponse<string>` with success message
     - `400 Bad Request`: Validation errors or invalid email
2. **Resend OTP**:
   - HTTP Verb: `POST`
   - Path: `/api/v1/authentication/resend-otp`
   - Request Body: `ResendOtpCommand` `{ "email": "string" }`
   - Responses:
     - `200 OK`: `ApiResponse<string>` with success message
     - `400 Bad Request`: Validation errors or invalid email
     - `429 Too Many Requests`: When requested within 30 seconds of previous request for the user

---

## 3. Data & Domain Changes

### 3.1 Domain Entity (`ApplicationUser.cs`)
Add domain helper methods to manage `VerificationCode` and `VerificationCodeExpiry`:
- `public void SetVerificationCode(string code, DateTime expiry)`:
  Updates `VerificationCode = code; VerificationCodeExpiry = expiry; UpdatedAt = DateTime.UtcNow;`
- `public void ClearVerificationCode()`:
  Updates `VerificationCode = null; VerificationCodeExpiry = null; UpdatedAt = DateTime.UtcNow;`

---

## 4. Rate Limiting Mechanism

### 4.1 Requirement
Users may request an OTP resend at most once every 30 seconds for each user (identified by normalized email).

### 4.2 In-Memory Cache Throttling (`IOtpRateLimiter` / `IMemoryCache`)
- Cache Key: `$"otp_resend_ratelimit_{normalizedEmail}"`
- TTL / Expiration: 30 seconds.
- Flow:
  1. Look up cache key for normalized email.
  2. If found with timestamp `sentAt` and `DateTime.UtcNow - sentAt < TimeSpan.FromSeconds(30)`:
     - Calculate remaining seconds: `Math.Ceiling((30 - (DateTime.UtcNow - sentAt).TotalSeconds))`
     - Throw `TooManyRequestsException` with localized message: `"Please wait {0} seconds before requesting another code."`
  3. If not found or expired:
     - Allow request, save `DateTime.UtcNow` to cache key with 30s sliding/absolute expiration.

---

## 5. Email & OTP Service

### 5.1 OTP Generation
- 6-digit cryptographically secure numeric string (`RandomNumberGenerator.GetInt32(100000, 1000000).ToString()`).
- Expiration: configured via `EmailSettings.VerificationCodeExpiryMinutes` (default: 10 minutes).

### 5.2 Email Service (`IEmailService`)
- Interface: `IEmailService.SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)`
- Implementation: `EmailService` using MailKit `SmtpClient` and `MimeMessage`, configured via `EmailSettings`.
- Template: Professional HTML email containing the LMS branding, 6-digit OTP code, and expiry note.

---

## 6. Commands & Handlers

### 6.1 ForgotPassword
- `ForgotPasswordCommand`: `IRequest<Result<string>>`, properties: `Email`.
- `ForgotPasswordCommandValidator`: validates `Email` not empty, valid email format, user exists and is active/not deleted.
- `ForgotPasswordCommandHandler`:
  - Generates 6-digit OTP.
  - Updates user via `SetVerificationCode(code, expiry)`.
  - Saves user changes.
  - Records rate limit entry in cache.
  - Sends email with OTP.
  - Returns `Result<string>.Success(..., localizer[LocalizationKeys.Authentication.OtpSentSuccess].Value)`.

### 6.2 ResendOtp
- `ResendOtpCommand`: `IRequest<Result<string>>`, properties: `Email`.
- `ResendOtpCommandValidator`: validates `Email` not empty, valid email format.
- `ResendOtpCommandHandler`:
  - Enforces 30-second rate limiter (throws `TooManyRequestsException` if rate limited).
  - Finds active user by email.
  - Generates new 6-digit OTP.
  - Updates user via `SetVerificationCode(code, expiry)`.
  - Saves changes to DB.
  - Records rate limit entry in cache for 30s.
  - Sends email with OTP.
  - Returns `Result<string>.Success(..., localizer[LocalizationKeys.Authentication.OtpResentSuccess].Value)`.

---

## 7. Localization
Add localization keys and translations in `LocalizationKeys.cs` and `messages.en.json`:
- `Authentication.OtpSentSuccess`: `"Verification code has been sent to your email."`
- `Authentication.OtpResentSuccess`: `"A new verification code has been sent to your email."`
- `Authentication.OtpRateLimited`: `"Please wait {0} seconds before requesting another verification code."`
- `Authentication.UserNotFound`: `"No active account found with this email address."`
