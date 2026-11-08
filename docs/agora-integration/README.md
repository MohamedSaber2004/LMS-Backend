# Agora Video Streaming — دليل التكامل (LMS)

**الغرض من الملف ده:** دليل تنفيذي عملي لإضافة البث الحي للفيديو والصوت في مشروع LMS باستخدام Agora، من تجهيز حساب Agora لحد النشر على الإنتاج. الخطة التفصيلية الكاملة (بالجداول والمراجع) في [`docs/plans/2026-11-05-live-class-streaming-agora.md`](../plans/2026-11-05-live-class-streaming-agora.md) — الملف ده بيقولك **تعمل إيه وبأي ترتيب**، والخطة بتقولك **كل تفصيلة ليه**.

---

## 1. القرارات المثبتة (متتغيرش من غير قرار جديد)

| القرار | القيمة |
|---|---|
| المزوِّد | **Agora** (RTC للمكالمة + RTM للدردشة + Cloud Recording للتسجيل) |
| مكان البث | مربوط بالحصة الحالية **`Session`** — مفيش كيان «قاعة» منفصل عن الحجز |
| الحضور | **1:1**: المدرس + المتعلم اللي معاه المقعد (`SeatCapacity` = 1) |
| الصوت والصورة | **لا يمرّان على سيرفرنا أبداً** — المتصفح بيتكلم مع Agora مباشرة. دورنا: إصدار التوكنات، تسجيل الدخول/الخروج، بدء/إيقاف التسجيل، استقبال الـ webhooks |
| اسم القناة | `live-{sessionId:N}` — ثابت من رقم الحصة، **مش من الميعاد** (فالـ reschedule لا يغيّره) |

## 2. مين بيتكلم مع مين

```text
  [متصفح المدرس]                [متصفح المتعلم]
        |   \                        /   |
        |    \--- صوت + صورة ---> Agora RTC <--/
        |                                ^
   (1) طلب توكن                    (2) بدء/إيقاف التسجيل
        |                                |
        v                                v
   [LMS API]  -----(3) REST: acquire/start/stop---->  [Agora Cloud Recording]
      |                                                     |
      +<----(4) Webhook: «التسجيل بدأ / اترفع / فشل»--------+
      |
   [SQL Server]  <- الغرف، الحضور، حالات التسجيل، صندوق الإشعارات
```

البيانات الحساسة (`App Certificate` + `Customer Secret`) بتعيش في السيرفر بس — ما تنزلش للفرونت أبداً.

---

## 3. قبل أي كود: تجهيز Agora Console

1. اعمل حساب على **Agora Console** وأنشئ مشروع.
2. خد **App ID** — علني، هيتبعت للفرونت في `/config`.
3. خد **App Certificate** — من صفحة المشروع: «Generate primary certificate» — سري، يعيش في السيرفر بس.
4. فعّل **Video Calling** من Products & Usage (و RTM لو هتستخدم الدردشة).

**البث الحي مش محتاج أي تخزين سحابي** — الصوت والصورة بين المتصفحات عبر Agora مباشرة، والاستضافة مش بتشيل أي حاجة.

> **التسجيل مؤجل لأجل غير مسمى:** Agora Cloud Recording بترفع الملفات **بنفسها** لحساب تخزين سحابي خارجي (S3/Azure/GCS) — مفيش طريقة تخليها تتحفظ على مساحة الاستضافة العادية (زي MonsterASP.NET) لأن الملفات لا تعدي على سيرفرنا أبداً. لحد ما يتقرر bucket، **مفيش** `CustomerId`/`CustomerSecret`/`NcsSecret` ولا أي حقول تسجيل في الـ settings.

## 4. خطوات التكامل في الـ backend (الريبو ده)

### 4.1 الإعدادات — `AgoraSettings`

- اعمل `AgoraSettings` بنفس نمط `GoogleAuthSettings` مع خواص `Has*` (الخدمة قابلة إنها تبقى غير مهيأة زي `IEmailService.IsConfigured`).
- الحقول — **4 بس** (من غير أي حاجة تسجيل): `AppId`, `AppCertificate`, `TokenLifetimeMinutes`, `JoinLeadMinutes` + خاصية `HasCredentials`.
- سجّلها بـ `Configure<AgoraSettings>` في `LMS.Application/DependencyInjection.cs`.
- **في الإنتاج:** الـ appsettings مستثناة من النشر — الحقن الوحيد هو خطوة `ensure-live-config.cjs` في [`.github/workflows/deploy-ftp.yml`](../../.github/workflows/deploy-ftp.yml)، فالمفاتيح هتتحط GitHub Secrets وتتضاف في نفس الخطوة.

### 4.2 فك قفل الكاميرا والميك — **إلزامي قبل أي اختبار**

المتصفح حالياً رافض الكاميرا على مستوى الهيدرات، مهما كان كود Agora صح:

1. **`PermissionsPolicy`** — في [`appsettings.json`](../../LMS.API/appsettings.json#L26) و [`appsettings.Production.json`](../../LMS.API/appsettings.Production.json#L55): غيّر `camera=()` و `microphone=()` لـ `camera=(https://دومين-الفرونت)` و `microphone=(https://دومين-الفرونت)`.
2. **`CSP`** — في [`appsettings.Production.json`](../../LMS.API/appsettings.Production.json#L53): `connect-src 'self'` لازم تضيف له `https://*.agora.io wss://*.agora.io *.sd-rtn.com`، وكذلك `media-src`.
3. **فخ معروف:** الـ Development بيفك الـ CSP تلقائياً — نجاح الاختبار على localhost **مش دليل** على نجاح الإنتاج. الاختبار الحقيقي على الرابط الحيّ بس.

### 4.3 قاعدة البيانات

- 4 كيانات جديدة في `LMS.Domain/Entities` بنفس نمط `Session.cs` (private setters + factory static):
  - `LiveRoom` — `SessionId`, `ChannelName`, `Status`, `StartedAtUtc?`, `EndedAtUtc?`, `HostUserId` + index فريد مفلتر `WHERE IsDeleted = 0`.
  - `LiveRoomParticipant` — `RoomId`, `UserId`, `Role`, `AgoraUid`, `JoinedAtUtc`, `LeftAtUtc?` (مصدر الحقيقة للحضور).
  - `LiveRecording` — `RoomId`, `Sid?`, `ResourceId?`, `Mode`, `Status`, `FilesJson`, `MaxIdleTime`, `LastError?` — **`FilesJson` أسماء ملفات بس، بلا AccessKey/SecretKey**.
  - `AgoraNotification` — `NoticeId` (unique), `ProductId`, `EventType`, `PayloadJson`, `ReceivedAtUtc`, `ProcessedAtUtc?`, `SignatureValid` (صندوق الإشعارات + idempotency).
- 4 enums بـ `HasConversion<int>()`، 4 `IEntityTypeConfiguration` بنمط `SessionConfiguration` مع `HasQueryFilter(x => !x.IsDeleted)` في آخر كل Configure.
- Migration واحدة: `AddLiveClasses` — **من غير `--no-build`** والبناء على `-p:OutputPath=bin/verify` (الـ debugger يقفل `bin/Debug`).

### 4.4 الخدمات

1. **التوكن (AccessToken2):** انسخ ملفات C# الرسمية من `AgoraIO/Tools → DynamicKey/AgoraDynamicKey/csharp` (`RtcTokenBuilder2.cs`, `AccessToken2.cs`, `Privileges.cs`, `ByteBuf`, `Utils/*`) داخل `LMS.Infrastructure/Services/Agora/Token/` — مكتبات NuGet المجتمعية غير رسمية ومش موثوقة في مسار التشفير.
2. **REST client:** `AddHttpClient<AgoraRestClient>()` في `LMS.Infrastructure/DependencyInjection.cs` مع timeout — Cloud Recording بيتكلم REST بس على `https://api.sd-rtn.com` بمصادقة Basic (`base64(CustomerId:CustomerSecret)`).
3. **Webhook verifier:** `IAgoraNotificationVerifier` — تحقق توقيع `Agora-Signature-V2` (HMAC-SHA256) على **الـ raw body** قبل أي model binding.

### 4.5 الـ endpoints — `LiveClassesController`

`[Authorize]` + `[RoleAuthorize(Instructor, Learner)]` على نفس قاعدة `SessionsController`: الـ handler يستنتج الدور من الحجز، **المنادي ما يختارش دوره بنفسه**. Base = `api/v{version}/live-classes`.

| المجموعة | الـ endpoints |
|---|---|
| البث | `GET /config` · `POST /{sessionId}/start` · `POST /{sessionId}/token` · `POST /{sessionId}/join` · `POST /{sessionId}/leave` · `GET /{sessionId}` · `POST /{sessionId}/end` |
| التسجيل | `POST /{sessionId}/recording/start` · `POST /{sessionId}/recording/stop` · `GET /{sessionId}/recording` · `GET /{sessionId}/playback` |
| القوائم | `GET /my` (مدرس) · `GET /my-bookings` (متعلم) · `GET /{sessionId}/events` · `GET /admin/pagginated` · `POST /admin/{sessionId}/force-end` |
| الإشراف + webhook | `POST /{sessionId}/mute` · `POST /webhooks/agora` (**anon** — أخطر واحدة) |

**تحذيرات `POST /webhooks/agora`:** `[AllowAnonymous]` + قراءة الـ raw body قبل الـ model binding + الرد `200` بـ JSON في أقل من **10 ثوانٍ** + idempotency على `noticeId` + ممنوع HTML عند رفض التوقيع.

الجدول الكامل بالتفاصيل في الخطة §8.

### 4.6 تعديلات endpoints قائمة (5)

| Endpoint | التعديل |
|---|---|
| `POST /sessions/{id}/confirm` | إنشاء `LiveRoom` بحالة `NotStarted` + رفض التأكيد لو Agora غير مهيأة |
| `POST /sessions/{id}/cancel` | منع الإلغاء لو الغرفة `Live` |
| `reschedule` + `approve` | رفض لو الغرفة بدأت — **من غير** إعادة تسمية القناة |
| `GET /sessions` · `my-bookings` · `{id}` | إضافة كتلة `live` (status, canJoinNow, joinOpensAtUtc, hasRecording, recordingStatus) — `null` لو الغرفة لسه ما اتنشأتش حتى ما ينكسرش الفرونت القديم |
| `POST /attachments/*` | `[Authorize]` — ثغرة قائمة لازم تتقفل قبل أي ملفات حصص |

`channelName`/`uid`/`token` **لا يدخلوا القوائم** — بيجوا من `/start` أو `/token` بس.

---

## 5. الفرونت-إند (ريبو تاني — مش هنا)

1. `npm install agora-rtc-sdk-ng`.
2. `GET /live-classes/config` → خد `appId` و `enabled`.
3. لما يضغط «بدء الحصة» أو «انضم» → `POST /live-classes/{id}/start` → `{ channelName, uid, token, expiredTs }`.
4. `client.join(appId, channelName, token, uid)` — **الـ uid اللي في الـ join لازم يطابق الـ uid اللي في التوكن بالضبط**.
5. سجّل `client.on('token-privilege-will-expire')` → اعمل `POST /token` وجدّد **قبل الانتهاء بـ 30 ثانية** — من غيرها المكالمة بتقطع في نص الحصة.
6. عند الخروج: `client.leave()` ثم `POST /leave`.

---

## 6. النشر للإنتاج

1. **Secrets:** أضف `AGORA_*` كـ GitHub Secrets، وحقنها في خطوة `ensure-live-config.cjs` في [`deploy-ftp.yml`](../../.github/workflows/deploy-ftp.yml) — أي مفتاح في الريبو مباشرة **مش هيوصل** للاستضافة.
2. **البيئة:** `web.config` بيثبّت البيئة على `Test` — أي سلوك مربوط بـ«إنتاج» مش هيشتغل. الاختبار النهائي على `lms-api.runasp.net` نفسه.
3. **HTTPS إجباري** — الكاميرا والميك ما يشتغلوش على HTTP.
4. **الـ webhook:** عناوين IP بتاع Agora بتتغيّر — قفلها بـ Azure IP Restrictions لو هتقفل، وحدّثها (الاستعلام الرسمي `GET https://api.agora.io/v2/ncs/ip` بنفس الـ Basic auth).

---

## 7. الفخاخ المعروفة (اقراها قبل ما تبدأ)

1. **`435`** = مفيش مستخدمين في القناة — بتظهر لو المدرس بدأ التسجيل قبل ما يدخل. لازم تترجم لرسالة مفهومة، مش 500.
2. **التوكن بيخلص** — أقصى عمر 24 ساعة، والتجديد قبل الانتهاء بـ 30 ثانية مسؤولية الفرونت.
3. **`uid` mismatch** — uid في التوكن ≠ uid في الـ join = رفض دخول صامت.
4. **`acquire` و `start`** — الـ `start` لازم خلال **ثانيتين** من الـ `acquire` (والـ resourceId صلاحيته 5 دقايق) → الاثنين في طلب واحد عندنا.
5. **`stop` إجباري** — نسيانه = فوترة مستمرة من Agora.
6. **Webhook مكرر** — Agora بترسل نفس الحدث أكتر من مرة → `noticeId` هو مفتاح idempotency.
7. **توقيع webhook على الـ raw body** — أي قراءة أو تحويل قبلها بيكسر الـ HMAC.
8. **Development يفك CSP** — نجاح محلي ≠ نجاح إنتاج. دايم الاختبار على الرابط الحي.
9. **`Permissions-Policy` الحالية مقفولة** (`camera=()`) — من غير خطوة 4.2 الكاميرا مش هتشتغل خالص.
10. **قاعدة البيانات مشتركة** — `Development` و `Production` على نفس الـ SQL الحيّ: أي migration لازم موافقة صريحة قبل التطبيق.

---

## 8. ترتيب التنفيذ

| الدفعة | المحتوى | النتيجة |
|---|---|---|
| **4-A** | الهيدرات + الإعدادات + الكيانات + التوكن + endpoints البث | حصة مباشرة كاملة من غير تسجيل |
| **4-B** | Cloud Recording + webhook + playback | التسجيل والمشاهدة |
| **4-C** | Hangfire (تنظيف الغرف) + كتم/قطع + تقارير حضور | النظام مش بيتكسر |
| **4-D** | لوحة الأدمن + إيقاف إجباري | عين الأدمن |

**قرارات لسه معلقة** (في الخطة §6 — ممنوع تنفيذ 4-B قبلها): bucket التخزين (S3/OSS/GCS — Agora **لا تسجّل على سيرفرنا**)، شكل الملف (`mix` ولا `individual`)، الدردشة (RTM ولا لأ)، تفعيل Hangfire.

---

## 9. المراجع

- الخطة الكاملة بالجداول والعقد التقني: [`docs/plans/2026-11-05-live-class-streaming-agora.md`](../plans/2026-11-05-live-class-streaming-agora.md)
- Cloud Recording RESTful API: `https://docs-md.agora.io/en/cloud-recording/reference/restful-api.md`
- Webhooks / Notifications (تواقيع + أحداث + IP query): `https://docs-md.agora.io/en/cloud-recording/develop/receive-notifications.md`
- بُناة التوكن C# الرسمية: `https://github.com/AgoraIO/Tools/tree/master/DynamicKey/AgoraDynamicKey/csharp`
