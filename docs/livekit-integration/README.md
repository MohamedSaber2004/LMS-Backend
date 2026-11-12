# LiveKit Video Streaming — دليل تكامل البث الحي في LMS

**الغرض من الملف ده:** دليل تنفيذي عملي لتشغيل الفصول المباشرة (1:1) بعد استبدال Agora بـ **LiveKit Cloud**. بيقولك **تعمل إيه وبأي ترتيب**، وفيه كل الفخاخ اللي اتقابلت في الطريق.

> **ليه اتحولنا من Agora؟** حساب Agora اتحوّل لحالة **Paused** (الشارة الوردانية في `console.agora.io/legacy/usage`)، والرسالة اللي ظهرت في المتصفح `CAN_NOT_GET_GATEWAY_SERVER: invalid vendor key, can not find appid` معناها الحقيقي إن **بوابة Agora بتخفي كل الـ App IDs** التابعة لحساب موقوف — مش غلط في الكود ولا في التوكن (اتثبت بالتجربة على SDK رسمي وملف token مفكّك حرفياً).رجوع الحساب للشغل بيحتاج **إضافة بطاقة ائتمان أو شحن رصيد**، ومفيش بطاقة ولا حساب بنكي → Agora مستحيلة عملياً.
>
> **LiveKit Cloud** بيشتغل على باقة **Build**: سعرها `$0`، **بدون بطاقة ائتمان**، 5,000 دقيقة اتصال/شهر، 100 مشارك متزامن، 50 جيجا بايت، و**TURN مضمّن** (مهم جداً للشبكات المقفولة في مصر).
>
> **ملفات Agora اتمسحت خالص** من الريبو (الكود + الدليل + صفحة الاختبار + مكتبة `AgoraRTC_N-4.22.0.js`) — مفيش أي حاجة لسه بتنادي عليها. كود الـ handlers القديم مسترجع من git لو احتجت ترجع له تاريخياً.

---

## 1. الصورة الكبيرة

| العنصر | القيمة |
|---|---|
| المزوِّد | **LiveKit Cloud** (باقة Build المجانية) — RTC فقط، بدون تسجيل |
| دور السيرفر | **إصدار التوكنات بس** — الصوت والصورة لا يمرّان على سيرفرنا أبداً |
| شكل المكالمة | 1:1 (مدرس + متعلّم)، غرفة واحدة لكل `Session` |
| الاستضافة | أي استضافة مشتركة عادية (MonsterASP.NET / RunAsp) — مفيش أي متطلبات Node أو Media Server عندنا |
| التوقيع | JWT موقّع بـ **HMAC-SHA256** بالمفتاح `ApiSecret` — اتكتب يدري في C# بدون أي حزمة NuGet جديدة |
| التسجيل (Egress) | **مؤجّل** — محتاج باقة مدفوعة + مكان تخزين، ومفيش لسه |

```
[متصفح المدرس] ──┐
                 ├── WebRTC/TURN ──> [LiveKit Cloud SFU]
[متصفح المتعلم] ─┘                        ^
                                           | (1) GET /live-classes/config  -> url
[LMS API] ──(2) POST start/join -> token──┘   (لا يمر منها أي ميديا)
```

---

## 2. قبل أي كود: تجهيز حساب LiveKit

1. ادخل **`https://cloud.livekit.io`** واعمّل حساب (Google أو بريد) — **مش بيطلب بطاقة**.
2. بعد التسجيل بيتم إنشاء **مشروع افتراضي** تلقائياً. اختار المشروع، ثم تبويب **Settings → API Keys** واعمل **New API Key**.
3. خد منك **3 قيم** دول بس (وده كل اللي لازم يوصل للسيرفر):
   - **Project URL** ← شكله `wss://<اسم-المشروع>-<hash>.livekit.cloud` (في **Settings → General** باسم `Websocket URL` / `Server URL`) — لازم يبقى **من نفس صفحة المشروع اللي فيه الـ API Key**، لأن مفتاح من مشروع تاني بيرجع `invalid API key`.
   - **API Key** ← بيبدأ بـ `API` (15 حرف، من غير underscore).
   - **API Secret** ← نص طويل عشوائي (43 حرف).
   - **انسخهم نص مش بالتصوير**: `Ctrl+C` من الخانة مباشرة والصقهم في رسالة/ملف. سرّ واحد اتنسخ من صورة فيه `l` بدل `I` أو `0` بدل `O` = كل الـ joins بترجع `401 invalid token` والكود سليم 100% (وده اللي حصل فعلاً — راجع فخ 11).
4. **الـ Region** (قرب/بعد الجغرافي للمشروع): لو اختارك منطقة بعيدة مش مشكلة للـ 1:1، لكن لو فيه خيار اختيار Region اختار أقرب حاجة ليك (أو للاتراكيا/أوروبا) علشان الكمون.
5. **منطقة المشروع ما لهاش أي شيء بالتكوين عندنا على عكس Agora** — مفيش حاجة زي `setArea`/`areaCode` في LiveKit؛ العنوان `wss://…livekit.cloud` نفسه بيمشّيك على البوابة الصحيحة.

> **السر ده حساس:** `ApiSecret` بيقدّر يصدر توكنات تدخل أي غرفة في حسابك. **ما يتعملوش commit**. انسخه للسيرفر مباشرة (قسم 8).

---

## 3. الإعدادات في الكود

### 3.1 `LiveKitSettings`

[`LMS.Application/Common/Options/LiveKitSettings.cs`](../../LMS.Application/Common/Options/LiveKitSettings.cs) — بنفس نمط `GoogleAuthSettings`، والخدمة **قابلة إنها تبقى غير مهيأة** (`HasCredentials`) زي `IEmailService.IsConfigured`:

| الحقل | يعني إيه |
|---|---|
| `Url` | `wss://….livekit.cloud` — بيتبعت للواجهة كما هي |
| `ApiKey` | الـ issuer في الـ JWT (`iss`) |
| `ApiSecret` | مفتاح توقيع HMAC-SHA256 |
| `TokenLifetimeMinutes` | مدة صلاحية التوكن — افتراضي `1440` (ساعات 24، زي Agora قبل كده) |
| `JoinLeadMinutes` | قد إيه قبل الموعد يفتح باب الدخول — افتراضي `15` |

```json
"LiveKit": {
  "Url": "wss://xxxxxxxxxxxx.livekit.cloud",
  "ApiKey": "API_XXXXXXXXXXXX",
  "ApiSecret": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "TokenLifetimeMinutes": 1440,
  "JoinLeadMinutes": 15
}
```

### 3.2 طبقة التوكن

- [`ILiveClassTokenService`](../../LMS.Application/Common/Interfaces/ILiveClassTokenService.cs) — الواجهة الوحيدة اللي بتعرف بيها الطبقات التانية المزوِّد (**اسم المحايد**: مفيش `LiveKit` في طبقة التطبيق، تقدر تبدّل المزوِّد تاني من غير ما تغيّر الـ handlers).
- [`LiveKitTokenService`](../../LMS.Infrastructure/Services/LiveKit/LiveKitTokenService.cs) — بتبني الـ JWT يدري (header + payload + HMAC) وبتستخدم `Base64UrlEncoder` الموجود في الحزمة اللي إحنا مرجعينها أصلاً لـ JWT.

**شكل التوكن** (مطلوب رسمياً: `exp`, `iss`, `sub`, `nbf`):

```json
{
  "iss": "API_XXXXXXXXXXXX",
  "sub": "3356758711",
  "nbf": 1762670000,
  "exp": 1762756400,
  "jti": "…",
  "video": { "room": "live-20a7…", "roomJoin": true, "canPublish": true, "canSubscribe": true, "canPublishData": true }
}
```

**ليه مكتوب يدري؟** لأن `ClaimValueTypes.Json` مش موجود في حزمة IdentityModel اللي بنستخدمها، ولأن المفتاح داخل `video` لازم يفضل **camelCase** ومكتوب كـ **كائن JSON** — أي序列化 غلط يخلّي `video` نص، والسيرفر بيرفض الاتصال برسالة مش مفهومة.

### 3.3 قواعد الأسماء (مهمة — نفس القيم القديمة شغّالة)

- **اسم الغرفة** = `LiveRoom.ChannelName` = `live-{sessionId:N}` → حروف إنجليزية + أرقام + `-` بس، وده متوافق مع LiveKit.
- **الهوية (`identity`)** = **نفس الرقم القديم** اللي كنا بنصدره لـ Agora: أول 4 بايتات من `Guid` المستخدم كـ `uint` (`GenerateParticipantUid`)، و`0` بيتحوّل لـ `1`.
- الهوية لازم تكون **وحيدة داخل الغرفة**: participant تاني بنفس الهوية بيدخل → **القديم بيتطرّد**. وده مقصود: لو عملت Refresh للصفحة هتاخد نفس الهوية والجلسة القديمة تتقفل بدل ما يفضل كاميران شغالين.

---

## 4. مفيش أي migration — وده مقصود

عمودين فضلوا بأسمائهم القديمة من غير ما نلمسهم:

| الحاجة | الاسم في قاعدة البيانات | الاستخدام الجديد |
|---|---|---|
| اسم غرفة البث | `LiveRoom.ChannelName` | يتبعت كـ `room` في التوكن |
| معرف المشارك | `LiveRoomParticipant.AgoraUid` (`bigint`) | نفس الرقم كـ `identity` |

`bigint` بيشيل `uint32` براحة، فالبيانات القديمة ما اتغيّرش وما اتعملهاش تحويل. **أي محاولة «تنضيف» الأسماء دي محتاجة migration + تشغيل على قاعدة الإنتاج المشتركة — متعملش غير بطلب صريح.**

---

## 5. عقد الـ API (قبل ← بعد)

الـ routes كلها زي ما هي (`/api/v1/live-classes/...`) — **مفيش endpoint جديد ولا محذوف**، اللي اتغيّر هو **شكل الردود**:

### 5.1 `GET /api/v1/live-classes/config` (AllowAnonymous)

```jsonc
// قبل (Agora)
{ "enabled": true, "appId": "52fb12c0…", "joinLeadMinutes": 15 }

// بعد (LiveKit)
{ "enabled": true, "url": "wss://xxxx.livekit.cloud", "joinLeadMinutes": 15 }
```

### 5.2 `POST /api/v1/live-classes/{sessionId}/start` و `POST .../join`

```jsonc
// قبل
{ "roomId": "…", "channelName": "live-…", "uid": 3356758711,
  "token": "007eJxTY…", "expiresAtUtc": "…", "role": "Host", "isHost": true }

// بعد
{ "roomId": "…", "url": "wss://xxxx.livekit.cloud", "roomName": "live-…",
  "identity": "3356758711", "uid": 3356758711,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9…",
  "expiresAtUtc": "…", "role": "Host", "isHost": true }
```

| التغيّر | تأثيره على الواجهة |
|---|---|
| `appId` → `url` | الـ SDK بيتنفّذ بـ `room.connect(url, token)` بدل `client.join(appId, channel, token, uid)` |
| `channelName` → `roomName` | اسم الحقل اتغيّر (والقيمة نفسها `live-{guid}`) |
| `identity` (جديد، نص) | بيزوّد بس — التوكن نفسه فيه الهوية، ومحتاجه للعرض/التشخيص |
| `uid` (اتساب) | رقمي وحيادي، لسه بيتكتب في جدول الحضور |
| `token` | بقى **JWT** (`eyJ…`) بدل صيغة Agora (`007eJx…`) |
| الـ errors | مفتاح الخطأ بقى `LiveStreaming` بدل `Agora`، والرسائل اتحوّلت لكلام محايد («خدمة البث الحي غير مهيأة») |

### 5.3 `GET /api/v1/live-classes/{sessionId}`

`channelName` → `roomName` + `url` جديد + داخل `participants`: `agoraUid` → `uid`.

### 5.4 `POST .../token` و `.../leave` و `.../end`

بلا تغيير في الشكل (`{ token, expiresAtUtc }` / `bool`) — بس التوكن بقى JWT، والمدة بقت من `TokenLifetimeMinutes` بدل رقم 1440 مكتوب في الكود.

> ⚠️ **الواجهة (frontend) في repo تاني — التغييرات دي عقد مكسور عندوها ولازم تتعملها تحديث.** ودي نقطة مش قابلة للتحقق من هنا.

---

## 6. تكامل الواجهة

```bash
npm install livekit-client
```

```js
import { Room, RoomEvent, Track } from "livekit-client";

const room = new Room({ adaptiveStream: true, dynacast: true });

room.on(RoomEvent.TrackSubscribed, (track, _pub, participant) => {
  const el = track.attach();
  if (track.kind === Track.Kind.Video) remoteEl.appendChild(el);
  else audioHolderEl.appendChild(el);          // الصوت لازم يتصل في DOM
});
room.on(RoomEvent.TrackUnsubscribed, (track) => {
  const removed = track.detach();
  (Array.isArray(removed) ? removed : [removed]).forEach(e => e?.remove());
});

const { url, token } = await api.joinLiveClass(sessionId);   // من الـ handlers
await room.connect(url, token);                               // لا appid، لا uid، لا setArea
await room.localParticipant.setMicrophoneEnabled(true);
await room.localParticipant.setCameraEnabled(true);

// المغادرة
await room.disconnect();
```

**الفروق عن Agora:**
- مفيش `appId` ولا `AgoraRTC.createClient` ولا `setClientRole` ولا `setArea` ولا `createCameraVideoTrack` — الكاميرا/المايك بيزيدوا من جوه نفس الـ room.
- مفيش `mode: "live" / "rtc"` و`codec`. المدرس والمتعلم الاتنين publishers عادي.
- `identity` و`room` جايين من التوكن نفسه؛ مش بتسترسلهم في الطلب.
- `track.attach()` بيرجّع عنصر `<video>`/`<audio>` ولازم يتحط في الـ DOM (مش بياخد container زي `play()`).
- إعادة الاتصال بعد انقطاع نت بتتم لوحدها (`RoomEvent.Reconnecting/Reconnected`) — Agora ما كانتش.

---

## 7. الفخاخ (11)

1. **التوكن هو مصدر الحقيقة.** اسم الغرفة والهوية جايين من الـ `video` grant و`sub` — أي قيمة تكتبها في الواجهة (أو في صفحة الاختبار) للزينة فقط؛ لو خالفت التوكن هتاخد `cannot connect to room` أو تدخل غرفة تانية.
2. **camelCase داخل `video`** — `roomJoin`/`canPublish`/`canPublishData`. سيرياليزر بـ snake_case أو كائن بيتحوّل لنص = رفض صامت.
3. **`enabled: false` مش خطأ.** لو `Url`/`ApiKey`/`ApiSecret` فيهم فراغ: `/config` بيرجّع `enabled:false`، و`start`/`join` بيرجّعوا 400 بمفتاح `LiveStreaming`. الحجز نفسه **بيفضل شغال** (الحصة اتحجزت والكلمة السر بتتولد وقت الدخول بس) — يعني لو شفت الحجز ناجح والدخول مرفوض، المشكلة في إعدادات LiveKit مش في الحجز. ما تفتكرش إن الكود مكسور.
4. **قاعدة البيانات المشتركة مع الإنتاج.** التشغيل المحلي على `Development` بيفصل على نفس SQL الحي — أي `start/join` بتعمله وانت بتجرّب بيتسجل فيه participant حقيقي. جرّب على session تجريبية صريحة.
5. **الـ CSP والـ Permissions-Policy لازم يتحدثوا.** في `appsettings`: `connect-src` يحتاج `https://*.livekit.cloud wss://*.livekit.cloud https://*.livekit.net wss://*.livekit.net`، و`script-src` يحتاج `https://cdn.jsdelivr.net` (لو بتستخدم الـ UMD زي صفحة الاختبار)، و`media-src` بقى `'self' data: blob:` (مفيش دواين ميديا تانية). و`PermissionsPolicy` لازم يفضل `camera=*, microphone=*` — من غيره الكاميرا مرفوضة على مستوى الهيدر مهما كان التوكن صح.
6. **الاستضافة المشتركة ما بحدّثش `appsettings*.json` تلقائياً.** الـ deploy في [`.github/workflows/deploy-ftp.yml`](../../.github/workflows/deploy-ftp.yml) بيعمل mirror بيستثني `appsettings*.json` (`-x '(^|/)appsettings[^/]*\.json'`)، وبعدها خطوة `ensure-live-config.cjs` بتصلّح **`JwtSettings.Secret` و`EmailSettings` بس** على الملف الحي `appsettings.Test.json` (الـ `web.config` مثبّت البيئة على `Test`). يعني: **الحقول الجديدة والـ CSP لازم تتكتب على السيرفر بيدك** — التعديل في الريبو لوحدّه ما يوصلش.
7. **الدقائق بتتحسب على المشاركين مش الجلسات.** 5,000 دقيقة اتصال/شهر = حصة 60 دقيقة 1:1 بتاكل **120 دقيقة** → تقريباً **40 حصة في الشهر** على المجاني. راقب `cloud.livekit.io` → Usage، واعمل الحساب ده قبل ما تزوّد الجدول.
8. **مفيش تسجيل.** LiveKit Egress محتاج باقة مدفوعة + bucket تخزين؛ الاستضافة المشتركة ما تستلمش ميديا. لسه **مش متعمل** — زي Agora بالظبط.
9. **انتهاء التوكن أثناء المكالمة.** 24 ساعة كفاية لحصة، لكن لو الصفحة قافية والجلسة لسه عاملة، اطلب `POST /{sessionId}/token` من غير ما تعمل refresh للصفحة. التوكن المنتهي ما بيقطعش اتصال قائم عند LiveKit، لكنه بيمنع أي إعادة اتصال.
10. **`dotnet build` بيفشل بـ MSB3027/3021 لو السيرفر الشغال قافل `bin`.** ابنِ لمجلد تاني (`-p:OutputPath=bin/verify/`) قبل ما تشغّل أي نسخة اختبار، ومتقتلش النسخة اللي في Visual Studio (بتكون من `bin/Debug`).
11. **`invalid token` و `invalid API key` رسالتين مختلفتين — استغلّهم.** اتحقق من القيم من غير متحتاج حد يشاركك الشاشة: اطلب توكين من `/token` (أو من `tokencheck`) وارفعه على `POST https://<الـ host>/rtc/v1/validate/` بهيدر `Authorization: Bearer <token>`:
    - `401 "invalid API key"` → الـ **ApiKey نفسه غير معروف** عند المشروع اللي في الـ `Url` (يعني المفتاح والمشروع من الاتنين مختلفين، أو نصّ المفتاح غلط).
    - `401 "invalid token"` مع `iss` موجود → المفتاح **ظبط**، والغلاط في **الـ ApiSecret** (حرف متلغّز من صورة: `l`/`I`/`1` و`O`/`0`).
    - `200 {}` → التركيبة كاملة صح، وأي رفض بعده يبقى من الكود أو من اسم الغرفة.
    ده بيفرز سبب الـ 401 في صفحة الاختبار (`ConnectionError: could not establish signal connection: invalid token`) في ثانية واحدة.

---

## 8. التشغيل على السيرفر الحي

1. **خُد الـ 3 قيم من LiveKit Console** (قسم 2).
2. **عدّل `/wwwroot/appsettings.Test.json` على السيرفر** (FTP — FileZilla أو سكريبت `curl`) — ده الملف الحي فعلاً:
   - قسم `"LiveKit": { "Url", "ApiKey", "ApiSecret", … }` بالقيم الحقيقية.
   - سطر `Security:Headers:ContentSecurityPolicy` بالـ hosts الجديدة (قسم 7.5).
   - **ما تعملش upload للملف من الريبو** — هتمسح قيم السيرفر زي connection string. نزّله، عدّل، ارفع.
3. ⚠️ **`ApiSecret` حالياً مكتوب في [`appsettings.Development.json`](../../LMS.API/appsettings.Development.json)** — والملف ده **متتبّع في git ومش مستور بـ `.gitignore`**، يعني أي commit جاي هيخوّض السر للريبو (وده نفسه اللي حصل لشهادة Agora من قبل). اختار واحدة ونفّذها:
   - **أ**: تسيبه زي ما هو (الريبو خاص) — أسهل حاجة للتجربة المحلية وعلى VS.
   - **ب**: تضيف `appsettings.Development.json` لـ `.gitignore` + `git rm --cached` مرة واحدة، والقيم تعيش عندك محلياً بس.
   - **ج**: تسيب الحقول فاضية في الريبو، وتحط `LIVEKIT_URL` / `LIVEKIT_API_KEY` / `LIVEKIT_API_SECRET` كـ GitHub Secrets وتضيف خطوة في `deploy-ftp.yml` زي خطوة `EMAIL_*` الحالية — ده كمان بيحل مشكلة السيرفر الحي في القسم 8.
4. **لو الـ site بيرد بـ 502 بعد الرفع:** الـ `app_offline.htm` أو إعادة تدوير التطبيق (لوحة RunAsp) — الإعدادات بيتقروا عند بدء التشغيل فقط.
5. **جرّب بالترتيب ده** (كله شغال من غير ما حد يشاركك):
   - `GET https://lms-api.runasp.net/api/v1/live-classes/config` → `enabled:true` + `url` صح.
   - افتح **`https://lms-api.runasp.net/livekit-test.html`** — الصفحة بتقرا `/config` لوحدها وبتعبّي `url`.
   - اطلب `start`/`join` من الـ API (أو من `livekit-test.html?token=…`) وحط التوكن في الصفحة، واضغط انضمام — لازم الكاميرا تشتغل ويظهر `Live ✅`.
   - افتح نفس الصفحة في **متصفح تاني** (أو جهاز تاني) بنفس الغرفة → لازم تشوف الكاميرتين.

> للتجربة المحلية بعد بناء `bin/verify` وتشغيله على `https://localhost:44312` — الصفحة نفسها على `https://localhost:44312/livekit-test.html`.

---

## 9. الملفات اللي اتغيّرت في الريبو

| الملف | الدور |
|---|---|
| `LMS.Application/Common/Options/LiveKitSettings.cs` | تكوين المزوِّد |
| `LMS.Application/Common/Interfaces/ILiveClassTokenService.cs` | واجهة محايدة للطبقات التانية |
| `LMS.Infrastructure/Services/LiveKit/LiveKitTokenService.cs` | بناء JWT + توليد الهوية |
| `LMS.Application/Features/LiveClasses/DTOs/{LiveClassTokenDto,LiveClassDetailsDto,LiveClassConfigDto}.cs` | شكل الردود الجديد |
| `LMS.Application/Features/LiveClasses/Queries/GetLiveClassConfig/*` | بديل `GetAgoraConfig` |
| handlers الـ 4 (`Start/Join/Refresh/GetSession`) | بتستدعي `ILiveClassTokenService`؛ إنشاء الـ `LiveRoom` اتنقل من `ConfirmSession` (اتشيل مع خطوة تأكيد المدرس) لـ `CreateBooking`، فالحجز نفسه بيخلق الغرفة |
| `LMS.API/wwwroot/livekit-test.html` | صفحة اختبار يدوية زي `agora-test.html` |
| `appsettings{,.Development,.Test,.Production}.json` | قسم `LiveKit` + CSP |
| **اتمسح** `LMS.Infrastructure/Services/Agora/**` + `AgoraSettings.cs` + `IAgoraTokenService.cs` + `AgoraConfigDto.cs` + `GetAgoraConfig/**` | 12 ملف — كلها متتبّعة في git فمسترجعة |

---

## 10. المراجع

- باقات LiveKit Cloud (Build = `$0`، بدون بطاقة، 5,000 دقيقة / 100 متزامن / 50GB): https://livekit.com/pricing
- التوكنات والـ grants (المطالب المطلوبة `exp/iss/sub/nbf` + أسماء `video`): https://docs.livekit.io/frontends/reference/tokens-grants/
- الـ Web SDK (`livekit-client`) — `Room`, `RoomEvent`, `attach/detach`: https://docs.livekit.io/frontends/
- إدارة المشاركين والهوية الوحيدة لكل غرفة: https://docs.livekit.io/intro/basics/rooms-participants-tracks/participants/
- ملف `docs/agora-integration/**` **اتمسح** (الدليل + صفحة الاختبار + مكتبة SDK) — مرجع Agora الرسمي مش مطلوب هنا بعد دلوقتي.
