# Live Class (Agora RTC) — Backend Implementation Plan

**Date:** 2026-11-04
**Scope:** Real-time instructor/learner class audio-video for the `tlive` screen, plus the recording, chat and group-class decisions that hang off it.
**Backend measured at:** commit `72cd0e5`.
**Provider decision:** resolves **D4** (*Video provider — Zoom / Meet / Jitsi / Daily*) in [INSTRUCTOR-APP-README.md](INSTRUCTOR-APP-README.md) §4: **Agora**.
**Supersedes:** the "SignalR hub" line item in INSTRUCTOR-APP-README.md Phase 4 — see §2.3.

---

## 1. What the backend actually has to do

Agora's media path is **browser → Agora edge network**. No audio, video or screen-share bytes ever reach this API. The server has exactly three jobs:

1. **Mint a short-lived token** (AccessToken2, format `007`) per user per session, from `AppID` + `AppCertificate`.
2. **Decide who may enter** — membership already exists in the schema (`Session.InstructorId`, `SessionBooking`).
3. *(Phase 3 only)* Drive **Cloud Recording** over REST and consume Agora's event webhooks.

Everything interactive — publishing the camera, subscribing to the remote track, mute, screen share, reconnection — is the client SDK (`agora-rtc-sdk-ng`), which lives outside this workspace.

### 1.1 There is no server-side NuGet package to install

Agora publishes no maintained C# server SDK. Two options:

| Option | Verdict |
|---|---|
| **A. In-process token builder** — vendor Agora's official `AccessToken2` + `RtcTokenBuilder2` sources into `LMS.Infrastructure/Services/Agora/Internal/` | **Recommended.** Dependencies are `System.Security.Cryptography.HMACSHA256` and `System.IO.Compression.ZLibStream` (both in the .NET 8 target). No new package, no new process. |
| **B. Agora's Docker token server** | **Not viable.** Deploy is FTP-to-IIS shared hosting ([.github/workflows/deploy-ftp.yml](.github/workflows/deploy-ftp.yml)) — no sidecars, no extra ports, no container runtime. |

Do **not** re-derive the `007` byte layout from a blog post. Its shape is
`scheme + salt + issueTs + zlib(int16-prefixed packed services) + HMAC-SHA256(cert, appID + ts + salt + packed)`
where service type `1` = RTC (privileges: join channel, publish audio, publish video, publish data) and `2` = RTMP. One wrong byte yields `INVALID_APP_TOKEN`, which only surfaces when a real student presses **Join**. Reference sources: AgoraIO/Tools `DynamicKey/AgoraDynamicKey` (`RtcTokenBuilder2`) and the [token-server doc](https://docs.agora.io/en/realtime-media/cloud-recording/build/set-up-authentication/authentication-workflow).

### 1.2 Agora Console prerequisites (not code)

- Project created, **AppID + primary certificate** enabled (certificate generation requires the project to be in *Activated* state).
- Token expiration set; AppID type (Development/Test vs Production) switched before launch, since a Test AppID will not carry real traffic.
- For Phase 3 only: link S3-compatible storage (vendor, region, bucket, access key) for recording files.

---

## 2. Design

### 2.1 Channel, uid and role are derived — the MVP needs no migration

| Concept | Rule | Why |
|---|---|---|
| **Channel name** | `lms-s-{SessionId:N}` (lowercase) | Agora allows `A-Za-z0-9 _-` up to 64 chars. A pure function of an already-unique id means a channel can never drift from, or outlive, its session — so there is nothing to store. |
| **Agora uid** | **numeric `uint32`**, taken from the first 4 bytes of `Users.Id` with a non-zero guard | A string uid is capped at **23 bytes**; a `Guid` is 36 (`D`) or 32 (`N`) characters. Numeric is stable across rejoin (the student keeps the same tile) and needs no column. |
| **Role** | `host` for the session's instructor and, in 1:1, for the booking holder; `audience` for learners in a group session | Derived from `Session.InstructorId` / `SessionBooking`. |
| **Join window** | `[StartsAtUtc − preGrace, EndsAtUtc + postGrace]` from `SchedulingOptions`-style config | The client already receives both timestamps, so the **Join button stays a frontend pre-check** (existing convention); the server re-validates regardless. |

### 2.2 Privilege separation is the moderation control

The host token carries join + publish audio + publish video + publish data; the audience token carries join only. A learner in a group class then **physically cannot publish** — no server-side mute endpoint required, and it cannot be bypassed from the browser.

### 2.3 SignalR is not needed for media

INSTRUCTOR-APP-README.md Phase 4 assumed a SignalR hub. Agora removes that need entirely for audio/video. A hub is only worth adding for **text chat / presence / raise-hand**, and the alternative there is Agora's own data channel (already inside the token's publish-data privilege) or RTM. Decide chat in Phase 2 (§7).

### 2.4 Attendance is the one thing that would force a migration

Phase 6 (dashboard) wants `SessionParticipant` join logs, and in-session marking wants "no-show / network drop / teacher missed". That is a real table — the token endpoint alone cannot produce it, and Agora does not report per-user join events to your server unless you use the *in-channel* participant-count API or client-reported joins.

**Consequence:** §2.1's "no migration" claim holds **only while attendance and session summary stay out of scope.** The moment attendance is required, `SessionParticipant(SessionId, UserId, JoinedAtUtc, LeftAtUtc, Source)` becomes the first migration of this feature. Keep the token endpoint honest about it: mint the token, and if/when the join log exists, write the row in the same handler.

---

## 3. Hard blockers before any line of application code

### 3.1 `Permissions-Policy` denies camera and microphone today

```
LMS.API/appsettings.json:26                 "PermissionsPolicy": "geolocation=(), microphone=(), camera=()"
LMS.API/appsettings.Test.json:55            "PermissionsPolicy": "camera=(), microphone=(), geolocation=(), payment=(), ..."
LMS.API/appsettings.Development.json:86     same
LMS.API/appsettings.Production.json:55      same
```

`camera=()` / `microphone=()` means the browser blocks the permission prompt **before any JS runs** — WebRTC cannot start no matter what the API returns. Required value for the pages that host the class:

```
camera=(self), microphone=(self), display-capture=(self), autoplay=(self)
```

**(self)** rather than `*`, because the SDK runs from your own origin, not an embedded third-party frame.

### 3.2 Editing the repo file does not change production

Two independent facts from the existing setup:

- `.github/workflows/deploy-ftp.yml:157` **excludes `appsettings*.json`** from the FTP mirror, so a committed value never reaches `/wwwroot`.
- `web.config` pins `ASPNETCORE_ENVIRONMENT=Test`, so **`appsettings.Test.json` is the live file**.

Therefore the header change must be applied by the workflow's config-patch step (§4.2), the same mechanism already used for `JwtSettings.Secret` and `EmailSettings`. Verify the live response header after deploy — not the repo diff:

```bash
curl -sSI https://lms-api.runasp.net/api/v1/authentication/login | grep -i permissions-policy
```

---

## 4. Configuration and secrets

### 4.1 Options class

`LMS.Application/Common/Options/AgoraOptions.cs`, bound with `services.Configure<AgoraOptions>(configuration.GetSection("Agora"))` next to `SchedulingOptions` (see `LMS.API/Program.cs:318` for the existing pattern).

```json
"Agora": {
  "AppId": "<public id>",
  "AppCertificate": "<secret, never sent to a client>",
  "TokenLifetimeMinutes": 5,
  "PreJoinMinutes": 10,
  "PostGraceMinutes": 15
}
```

`Agora` must be added to the `IOptions<AgoraOptions>` graph **and** to the config-allowlist behaviour of the deploy patch step, not just to the dev appsettings.

### 4.2 Secret delivery

1. Add GitHub Secrets: `AGORA_APP_ID`, `AGORA_APP_CERTIFICATE`.
2. Extend the **Ensure the live config has…** step in [.github/workflows/deploy-ftp.yml](.github/workflows/deploy-ftp.yml) (the `ensure-live-config.cjs` block starting near line 171) to write `Agora.AppId` / `Agora.AppCertificate` into the downloaded live `appsettings.Test.json`, using the same `blank()` guard the JWT and email keys use so an operator-edited live value is never overwritten.

### 4.3 Fail degraded, not fatal

A blank certificate must **not** throw at startup. The existing comment in the workflow records what that looks like: an empty `JwtSettings.Secret` kills the process with `IDX10703`. The live-class feature is one endpoint, so:

- Token endpoint → `503` + localized `Live.NotConfigured` when `AppId`/`AppCertificate` are blank.
- The rest of the API keeps serving. This matters because the shared host can be deployed without the patch step succeeding.

---

## 5. Endpoint surface

### 5.1 `POST /api/v1/live/{sessionId}/token` — the whole of Phase 1

Request: empty body. Response `data`:

```json
{
  "appId": "…",
  "channel": "lms-s-3f9a…",
  "uid": 2945912447,
  "role": "host",
  "token": "007…",
  "expiresAtUtc": "2026-11-04T18:05:00Z"
}
```

Handler order — request checks in `MintLiveTokenCommandValidator`, orchestration only in the handler (existing convention):

1. Session exists and is not soft-deleted (global query filter handles this).
2. `Status == Confirmed`. A `Pending`, `Cancelled`, `Completed` or `NoShow` session gets no channel.
3. `DateTime.UtcNow` inside `[StartsAtUtc − PreJoinMinutes, EndsAtUtc + PostGraceMinutes]`.
4. Caller is the owning instructor (`SessionsValidation.InstructorOwnsSessionAsync`) **or** holds a Pending/Confirmed booking (`SessionsValidation.LearnerHoldsBookingOnSessionAsync`) — both already exist in [LMS.Application/Features/Sessions/SessionsValidation.cs](LMS.Application/Features/Sessions/SessionsValidation.cs). Cross-owner attempts are 403 via `ActorOwnership`, consistent with every other session write.
5. Mint with privileges per role (§2.2).
6. `expiresAtUtc = min(now + TokenLifetimeMinutes, EndsAtUtc + PostGraceMinutes)` — a token never outlives its class.

`AppID` is returned to the client (the SDK requires it). `AppCertificate` never leaves the server.

### 5.2 Deliberately not added

- **`GET /live/{sessionId}`** — the session detail response already carries `id`, `startsAtUtc`, `endsAtUtc`, `status`. A second read endpoint for the same row is the duplication that got the learner availability endpoint deleted.
- **A "can I join now" flag on the session DTO** — eligibility pre-checks belong in the frontend.

### 5.3 Phase 3 endpoints

| Route | Caller | Notes |
|---|---|---|
| `POST /api/v1/live/{sessionId}/recording/start` | `[RoleAuthorize(UserType.Instructor)]`, owning instructor only | acquire → start |
| `POST /api/v1/live/{sessionId}/recording/stop` | same | end → **release** |
| `POST /api/v1/agora/callbacks` | anonymous | event webhook; verify the signature header against the Callback URL secret set in the Agora Console **before** reading the body; keep it off the NET-Tracker body capture (binary/large payloads are already a known limitation there) |

---

## 6. Cloud Recording (Phase 3)

Follow [Agora's REST quickstart](https://docs.agora.io/en/realtime-media/cloud-recording/rest-quickstart) verbatim rather than this summary for paths and bodies. The lifecycle is:

```
acquire resource → start(mode, storageName, fileNamePrefix)
   → (update / query) → end → release
files: per-sid "get record files" endpoint, or the file-uploaded webhook
```

- **Auth:** `Authorization: Basic base64(AppID:AppCertificate)` — so the recording client also needs the certificate. Keep it inside the named `IHttpClientFactory` client handler; never in a DTO or log.
- **Mode:** `mix` for a 1:1 class (one composite file, one playback URL); `view` only if per-participant files are actually wanted.
- **`release` is mandatory on both paths** — the stop endpoint *and* the webhook handler. An unreleased resource bills indefinitely.
- **The webhook is the source of truth**, not a scheduled job: when the last participant leaves, Agora stops the recording on its idle timeout and fires the event. Your `SessionLifecycleService` cannot cover this because **Hangfire is disabled in the live environment** (`Hangfire:Enabled = false`).
- **New table** (the only genuinely new state in the feature — every column below is a field Agora returns and the replay UI shows):

```
SessionRecording(
  Id, SessionId, Sid, ResourceId, Mode, StorageVendor, BucketName,
  FileName, Status, StartedAtUtc, EndedAtUtc, DurationSeconds, SizeBytes)
```

Soft delete + `HasQueryFilter` + `BaseEntity` audit fields, per the global convention; migration plus the hand-applied SQL script, since `dotnet ef database update` is never run against the shared database.

---

## 7. Group classes: the blocker is booking, not Agora

`SessionType.Group` and `SeatCapacity` exist in [LMS.Domain/Entities/Session.cs:47](LMS.Domain/Entities/Session.cs#L47), but `CreateBookingCommandHandler` forces capacity 1 and takes the whole availability window, and group sessions are explicitly out of the deferred scope.

- Agora carries a 1:1 or an ≤ ~17-participant channel with the design above, unchanged.
- **Beyond ~17** you need CDN relay: service type `2` (RTMP) in the token, live-stream config in the Agora Console, and an HLS URL in the player — a separate cost and a separate decision.
- The real prerequisite for group classes is **multi-learner booking on one session**, which is backend work unrelated to Agora.

**So: Phase 1 ships 1:1 only.**

---

## 8. Abuse and cost controls

Agora bills per RTC minute, so the token endpoint is the attack surface.

- `IpRateLimiting` is already wired ([LMS.API/Program.cs:318](LMS.API/Program.cs#L318)); add a rule for `/api/v1/live/*`. Prefer a **per-user** limiter keyed on the `sub` claim (10/min) over a per-IP one — shared NAT would otherwise throttle a whole office, and an attacker rotates IPs more easily than user accounts.
- **5-minute TTL** plus client refresh on the SDK's `token-privilege-will-expire` event. Rejoin costs one extra round-trip; a leaked long-lived token costs an open channel.
- Never mint before the session is `Confirmed`, so a cancelled class cannot be entered.
- Recording is metered separately — gate start/stop to the owning instructor only, and consider a per-session recording cap.

---

## 9. Project gates

| Gate | Status / action |
|---|---|
| **Localization 1:1** | New `LocalizationKeys.Live.*` constants + `messages.en.json` entries for: token issued, not configured, session not confirmed, outside join window, not a participant. No hardcoded strings. |
| **Parity checker missing** | `bin-scan/loc-parity.js` was gitignored scratch and `bin-scan/` no longer exists. Move it to a tracked `scripts/Verify-LocalizationParity.ps1` as part of this work, or the gate silently disappears. |
| **No test project** | Nothing in this repo tests code. Verification is the scratch-console pattern used for the seeders. |
| **Frontend outside this workspace** | `E:\LMS` is backend-only. Response-shape changes are unverifiable contract changes here; the client must be checked in the browser. |
| **Git** | No commit or push without an explicit request. |

### Verification sequence

1. **Golden vector** — scratch console mints a token and compares against the Agora Console token generator. This is the only step that catches a broken builder before deploy.
2. `curl` the live endpoint with a real learner JWT and a real instructor JWT: confirm the role/privilege difference, and that a non-participant gets 403.
3. Confirm the response header: `curl -sSI … | grep -i permissions-policy` (§3.2).
4. **One real Agora join from the browser** — the only proof that channel/uid/role/token agree. Cannot be done from this workspace.

---

## 10. Phasing and file manifest

| Phase | Ships | Files | Migration |
|---|---|---|---|
| **1 — 1:1 live class** | Camera, mic, screen share, rejoin | `LMS.Application/Common/Options/AgoraOptions.cs` (new)<br>`LMS.Infrastructure/Services/Agora/AgoraRtcTokenService.cs` (new)<br>`LMS.Infrastructure/Services/Agora/Internal/{AccessToken2,RtcTokenBuilder2}.cs` (vendored)<br>`LMS.Application/Features/Live/Commands/MintLiveToken/{Command,Handler,Validator}.cs` + `DTOs/LiveTokenDto.cs` (new)<br>`LMS.API/Controllers/V1/LiveController.cs` (new)<br>`LMS.API/Routes/V1/ApiRoutes.cs` → `Live`<br>`Program.cs` options binding + DI<br>`appsettings.json` / `.Test` / `.Development` / `.Production` → `Agora` + `PermissionsPolicy`<br>`.github/workflows/deploy-ftp.yml` → Agora secrets + header patch<br>`LocalizationKeys` + `messages.en.json` | **none** |
| **2 — chat, whiteboard, presence** | Decide Agora data-channel/RTM vs SignalR (§2.3) | same folders | none (unless attendance lands here) |
| **3 — recording + replay** | Record, stop, playback link, webhook | `LMS.Domain/Entities/SessionRecording.cs`<br>`LMS.Persistence/Configurations/…`<br>`Features/Live/Commands/{StartRecording,StopRecording}`<br>`LMS.API/Controllers/V1/AgoraCallbackController.cs`<br>`Infrastructure/Services/Agora/AgoraRecordingClient.cs` | yes + hand-applied SQL |
| **4 — attendance & summary** | `SessionParticipant` join log, `GET /sessions/{id}/summary` | feeds Phase 6 dashboard | yes |
| **5 — group classes / CDN relay** | >17 participants | multi-learner booking refactor first | possibly |

---

## 11. Open decisions

| # | Question | Blocks |
|---|---|---|
| A1 | Is there an **activated Agora project** with the certificate visible, and which region/account tier? | everything — no code compiles against a live token otherwise |
| A2 | **Recording in the MVP?** It is the only Phase-1-or-2 item that forces a migration, an S3 bucket and a webhook surface | Phase 3 placement |
| A3 | **Attendance / join log now?** Decides whether `SessionParticipant` lands in Phase 1 (and thus whether Phase 1 has a migration) | Phase 4 |
| A4 | **Chat/whiteboard** now, and via Agora data-channel/RTM or SignalR? | Phase 2 |
| A5 | **1:1 only** for the first release, given multi-learner booking does not exist? | Phase 5 |
| A6 | Are `display-capture` (screen share) and `autoplay` also to be relaxed in the policy header, or is screen share a later release? | Phase 1 header change |

---

## 12. Related documents

- [INSTRUCTOR-APP-README.md](INSTRUCTOR-APP-README.md) — §3 environment blocker (superseded by §3 here), §4 decision **D4** (resolved by this document), Phase 4 checklist (SignalR line superseded by §2.3).
- [FRONTEND-SCHEDULING-README.md](FRONTEND-SCHEDULING-README.md) — the session/booking endpoints the token endpoint guards against; the Arabic hand-off doc for `/live/{sessionId}/token` will follow that file's format once Phase 1 is implemented.
