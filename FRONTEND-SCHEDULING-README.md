# دليل الـ Frontend — نظام المواعيد (Scheduling)

الملف ده مرجع كامل لكل الـ endpoints اللي اتعملت في §1.2 Scheduling: كل endpoint وظيفته إيه، الـ route بتاعه، الـ request (query أو body) بتاعه، الـ response، وإزاي تستخدمه في الشاشة.

**النطاق:** Availability (self-service للمدرّب + إدارة الأدمن + قراءة المتعلّم) وSessions والحجوزات وReschedule.
**آخر تحديث:** 2026-11-04
**Base URL:** `https://lms-api.runasp.net/api/v1/`

> ⚠️ **اقرا الجزء ده الأول:** الـ endpoints كلها مكتوبة ومبني عليها اختبارات بناء. التوليد **الفوري** للأيام المؤرخة شغال حالًا (كل إضافة/تعديل/حذف لنافذة أسبوعية بيولّد 30 يوم قدام)، لكن التوليد **الدوري** لسه مش شغال لأن Hangfire مقفول (`Hangfire:Enabled = false`) — يعني الأُفق ما بيتمشيش، والجلسات المنتهية ما بتتحولش `Completed`. التفاصيل في **قسم 8**.

---

## 1. قواعد مشتركة لكل الـ endpoints

### 1.1 Authentication

كل الـ endpoints في الدليل ده محتاجة توكن:

```
Authorization: Bearer <access_token>
Accept: application/json
Accept-Language: En
```

الـ role بيتقرأ من التوكن نفسه — **مفيش endpoint بيسألك "انت مين"**. المدرّب لما يبعت طلب على `/sessions` ياخد مواعيده هو، والمتعلّم ياخد حجوزاته هو، من نفس الـ route.

| المجموعة | الأدوار المسموحة |
|---|---|
| `/instructor-availability/my*` | Instructor |
| `/instructor-availability/pagginated`, `.../instructors/{id}/weekly-slots`, `.../weekly-slots/{id}` | Admin |
| `/onboarding/instructors` | Learner |
| `/sessions*`, `/reschedule-requests*` | Instructor أو Learner |

لو الدور غلط: **403**.

### 1.2 الـ envelope المشترك

كل الـ responses (نجاح وفشل) بنفس الشكل:

```json
{
  "success": true,
  "isSuccess": true,
  "message": "Sessions retrieved successfully.",
  "data": { },
  "errors": {},
  "statusCode": 200
}
```

| الحقل | نوعه | ملاحظة |
|---|---|---|
| `success` / `isSuccess` | bool | نفس القيمة، موجود للتوافق |
| `message` | string \| null | رسالة نجاح أو فشل، **اتعربت من الـ localization على السيرفر** |
| `data` | T \| null | الحمولة الحقيقية |
| `errors` | object | مفتاحه اسم الـ field وقيمته مصفوفة رسائل. فاضي في النجاح |
| `statusCode` | int | نفس الـ HTTP status |

**أهم نقطة في التعامل مع الأخطاء:** الـ 400 مش بييجي برسالة واحدة مجمّلة — بيجي في `errors` مفاتيح بأسماء الـ fields:

```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "data": null,
  "errors": {
    "EndTime": ["The end time must be later than the start time."]
  },
  "statusCode": 400
}
```

في الـ 403 والـ 404 الـ `errors` بتفضل `{}` والرسالة في `message` بس.

### 1.3 الـ pagination

أي endpoint بيقول `PagginatedResult` بيرجع داخل `data`:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 57,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

باراميترات الإدخال دايمًا `pageNumber` و`pageSize` — الافتراضي `1` و`20`، و`pageSize` أكبر مسموح بيه **50** (أكتر من كده يتعدّل بالفاليديتور برسالة `Page size must be between 1 and 50.`).

### 1.4 أشكال البيانات على الشبكة — جزء حسّاس

| النوع | الشكل على السلك | مثال | فين بيستعمل |
|---|---|---|---|
| `DateOnly` | `yyyy-MM-dd` بدون وقت ولا منطقة | `"2026-11-09"` | كل التواريخ اللي في query و body والـ responses الخاصة بالـ calendar والـ bookable slots |
| `TimeSpan` | `HH:mm:ss` وقت محلي **بدون منطقة** | `"09:30:00"` | `startTime` / `endTime` بتوع الـ weekly slots |
| `DateTime` | `yyyy-MM-ddTHH:mm:ssZ` — **UTC دايمًا** وفيه `Z` صريح | `"2026-11-09T07:30:00Z"` | `startsAtUtc`, `endsAtUtc`, `cancelledAt`, `createdAt`, و`fromUtc`/`toUtc` في الـ reschedule |
| Enums | **أرقام** (مش نصوص) | `"status": 1` | كل الـ enums تحت |
| `Guid` | نص صغير | `"b67e7114-..."` | — |

> **ليه كل التواريخ `DateOnly`؟** اليوم التقويمي مفيش فيه منطقة تتضيع. السيرفر ما بيقرأش منطقة التشغيل بتاعته نهائي في أي تاريخ. متبعتش `DateTime` في أي باراميتر اسمه `from` أو `to` أو `date` — ابعت تاريخ تقويمي بس.

> **الـ `DateTime` اللي في الردود جاهزة لـ `new Date(x)`.** فيها `Z` صريح، فمتزودش عليها offset تاني ولا تعاملها كأنها local.

### 1.5 خرائط الـ enums (أرقام)

```
SessionStatus:            0 = Pending | 1 = Confirmed | 2 = Completed | 3 = Cancelled | 4 = NoShow
BookingStatus:            0 = Pending | 1 = Confirmed | 2 = Cancelled | 3 = Declined
SessionType:              0 = OneToOne | 1 = Group
RescheduleRequestStatus:  0 = Pending | 1 = Approved | 2 = Declined
SessionTab (query only):  0 = Upcoming | 1 = Completed | 2 = Cancelled
UserType:                 0 = Learner | 1 = Instructor | 2 = Admin
DayOfWeek:                0 = Sunday | 1 = Monday | 2 = Tuesday | 3 = Wednesday
                          4 = Thursday | 5 = Friday | 6 = Saturday
```

في الـ query strings الـ enums بتقبل رقم أو اسم (`tab=1` أو `tab=Completed`)، لكن في الـ **JSON bodies رقم بس**.

---

## 2. نموذج المنطقة الزمنية — افهم الجزء ده قبل ما تكتب أي شاشة

ده أكتر جزء بيغلط فيه الفرونت، فهو مش تفصيل:

1. **الـ weekly slots وقتها محلي خام.** المدرّب بيقول "أنا متاح كل تلاتة من 9:9 إلى 11:00 بتوقيت بلدي". السيرفر بيخزّنها كده بالظبط — `day` + `startTime` + `endTime` بدون أي منطقة.
2. **السيرفر هو اللي عارف منطقة المدرّب، انت مش بتسأله عليها.** بتتحل من دولة المدرّب (`Users.CountryId` → `Countries.TimezoneId`)، ولو مفيش دولة بتتحل من كود الهاتف. مفيش إدخال بشري لكل مستخدم.
3. **الأيام المؤرخة (instances) كلها UTC.** السيرفر بيحوّل النمط الأسبوعي + منطقة المدرّب إلى صفوف مؤرخة فيها `startsAtUtc`/`endsAtUtc`.
4. **الفرونت ما بيحوّلش حاجة.** كل حاجة بتستعرضها من الـ instances هي UTC فيها `Z` — المتصفح بيظهرها بتوقيت المشاهد لوحده.

يعني قدامك نوعين وقت في نفس النظام، وممنوع تخلطهم:

| البيانات | إيه بالظبط | بتعرضها إزاي |
|---|---|---|
| `day` + `startTime` + `endTime` (weekly slot) | توقيت بلد المدرّب | تعرضها على المدرّب نفسه أو الأدمن زي ما هي، ومعها اسم المنطقة |
| `startsAtUtc` / `endsAtUtc` (instance / session / booking) | لحظة عالمية | `new Date(x).toLocaleString(...)` بتوقيت المشاهد |

### المنطقة بتيجي فين في الـ responses

| الـ endpoint | الحقل |
|---|---|
| `GET /instructor-availability/my` | `timezoneId` |
| `GET /instructor-availability/my/calendar` | `timezoneId` |

**مفيش أي endpoint للمتعلّم بيرجع `timezoneId`** — المتعلّم مبيحتاجش المنطقة أصلًا، لأن كل الأسطح اللي بتخدمه (`/onboarding/instructors` للفلترة و`/sessions/bookable-slots` للحجز) بتشتغل على لحظات UTC جاهزة. متحاولش تعرض للمتعلّم أوقات محلية للمدرّب.

`timezoneId` قيمة IANA زي `"Africa/Cairo"`، و`nullable`. **لو رجع `null`** معناها دولة المدرّب مش عندها anchor في جدول الدول — والفرونت المفروض ما يحاولش يخمّن، يعرض حالة "المواعيد مش متاحة حاليًا" (السيرفر كمان بيرفض أي أمر availability بالرسالة `The time zone for this instructor could not be resolved...`).

لو عايز تعرض اسم المنطقة للمستخدم: `Intl.DateTimeFormat(..., { timeZoneName: 'short' })` بيكفي.

---

## 3. Availability — self-service للمدرّب

الـ base: `/api/v1/instructor-availability`. كل اللي في القسم ده `[RoleAuthorize(Instructor)]` و**كل كتابة بتتربط على مدرّب التوكن نفسه** — مفيش هنا `instructorId` بيتاخد من الطلب.

### 3.1 `GET /my` — شاشة "توفيري الأسبوعي"

**الوظيفة:** يرجّع كل حاجة المدرّب محتاجها في صفحة الإعدادة: الـ slots الأسبوعية + منطقته + وضع الإجازة.

**Response `data`:**

```json
{
  "instructorId": "8c1f...",
  "timezoneId": "Africa/Cairo",
  "vacationMode": false,
  "weeklySlots": [
    {
      "id": "0f3a...",
      "instructorId": "8c1f...",
      "day": 3,
      "dayName": "Wednesday",
      "startTime": "09:00:00",
      "endTime": "11:00:00"
    }
  ]
}
```

**استخدام:** ابدأ بيها الصفحة. `dayName` نص إنجليزي جاهز للعرض — متعملش انت mapping من الرقم للعرض.

---

### 3.2 `POST /my/weekly-slots` — يضيف نافذة أسبوعية

**الوظيفة:** يضيف نمط متكرر ("كل أربعاء 9–11 صباحًا").

**Body:**

| الحقل | نوع | مطلوب | ملاحظات |
|---|---|---|---|
| `day` | int 0–6 | أي | 0 = الأحد |
| `startTime` | `"HH:mm:ss"` | أي | وقت المدرّب المحلي |
| `endTime` | `"HH:mm:ss"` | أي | لازم يكون **بعد** `startTime` |
| `instructorId` | guid | **ل** | للمدرّب: سيبه بره نهائي |

```json
{ "day": 4, "startTime": "14:00:00", "endTime": "16:30:00" }
```

**Response:** `201` و`data` = عنصر `weeklySlots` زي فوق.

**الأخطاء المتوقعة (كلها 400 جوه `errors`):**

| الرسالة | المعنى للعميل |
|---|---|
| `The day of week must be between Sunday (0) and Saturday (6).` | `day` بره النطاق |
| `The end time must be later than the start time.` | نافذة عكسية أو متساوية |
| `This window overlaps another availability slot for the same day.` | فيه تداخل مع نافذة قائمة في نفس اليوم |

**لو بعت `instructorId` بتاع مدرّب تاني → 403** (دي الحماية اللي اتعملت بعد ثغرة §2).

**بعد النجاح:** السيرفر بيولّد الأيام المؤرخة للمدرّب ده فورًا، فمفيش داعٍ تستدعي حاجة تانية — نزل `GET /my/calendar` من جديد وهتلاقي الأيام الجديدة.

---

### 3.3 `PUT /my/weekly-slots/{id}` — يعدّل نافذة

**الوظيفة:** يعدّل يوم/بداية/نهاية نافذة موجودة. المدرّب ما بيتغيرش.

**Route:** `id` = الـ `id` بتاع الـ slot. **Body:** زي `POST` بالظبط (`day`, `startTime`, `endTime`) بدون `instructorId`.

**Response:** `200` و`data` = الـ slot بعد التعديل.

**أخطاء:** نفس تلاتة اللي فوق + `This slot cannot be changed because one or more sessions are still scheduled on it. Reschedule or cancel them first.` — لو على النافذة جلسات لسه قائمة، **لازم** تتلغى أو تتنقل الأول.

لو الـ `id` بتاع مدرّب تاني → 403.

---

### 3.4 `DELETE /my/weekly-slots/{id}` — يحذف نافذة

**Response:** `200`، و`data = null`، و`message = "Availability slot deleted successfully."` (soft delete).

**أخطاء:** نفس رسالة "النافذة عليها جلسات" + `Availability slot was not found.` لو مش بتاعه أو محذوفة.

---

### 3.5 `PUT /my/vacation-mode` — وضع الإجازة

**الوظيفة:** يوقف أو يفتح استقبال حجوزات جديدة. وضع الإجازة **ما يلغيش** الجلسات المؤكدة.

**Body:**

```json
{ "enabled": true }
```

**Response:** `200` و`message = "Vacation mode updated successfully."`

**الأثر على المتعلّم:** وهو شغال، مفيش أي surface للمتعلّم بيفلتر وضع الإجازة — القائمة `GET /onboarding/instructors` لسه بترجّعه. النقطتين اللي بيقف عندهم الحجز: `GET /sessions/bookable-slots` و`POST /sessions` — الاتنين بيرفضوا بـ `This instructor is not taking new bookings right now.`

**ملاحظة تصميم:** وضع الإجازة **مش** في الـ instructor profile DTO — هو في سطح Availability. اقرأه من `GET /my` أو من الـ calendar.

---

### 3.6 `GET /my/calendar?from=&to=` — التقويم المؤرخ

**الوظيفة:** يرجّع الأيام المفصّلة بين تاريخين: إيه المتاح، إيه المقفول، وإيه اللي اتحجز فعلًا.

**Query:**

| البارام | نوع | مطلوب | ملاحظات |
|---|---|---|---|
| `from` | `yyyy-MM-dd` | أي | تاريخ تقويمي **محلي** للمدرّب |
| `to` | `yyyy-MM-dd` | أي | لازم `>= from`، والفرق **ما يتعداش 92 يوم** |

`GET /api/v1/instructor-availability/my/calendar?from=2026-11-04&to=2026-11-18`

**Response `data`:**

```json
{
  "from": "2026-11-04",
  "to": "2026-11-18",
  "timezoneId": "Africa/Cairo",
  "days": [
    {
      "date": "2026-11-04",
      "available": [ { "id": "instance-id", "startsAtUtc": "2026-11-04T07:00:00Z", "endsAtUtc": "2026-11-04T09:00:00Z" } ],
      "blocked":   [ { "id": "instance-id", "startsAtUtc": "2026-11-04T12:00:00Z", "endsAtUtc": "2026-11-04T13:00:00Z" } ],
      "booked":    [ { "id": "session-id",  "startsAtUtc": "2026-11-04T15:00:00Z", "endsAtUtc": "2026-11-04T16:00:00Z" } ]
    }
  ]
}
```

**🔴 فخ لازم تعرفه:** الـ `id` جوه `available` و`blocked` هو **`availabilityInstanceId`** (بتستخدمه في الحجز)، لكن الـ `id` جوه `booked` هو **`sessionId`** (بتستخدمه في `GET /sessions/{id}`). نفس اسم الحقل، معنيين مختلفين حسب المصفوفة.

**كمان:** المصفوفات بتغطي كل يوم في المدة، فالأيام اللي مفيش فيها حاجة بترجع بثلاث مصفوفات فاضية — متستنتجش "اليوم مقفول" من `available.length === 0`؛ ده معناه إما مفيش نمط في اليوم ده أو اليوم لسه ما اتولّدتش له صفوف.

**أخطاء:** `Both the start and the end date are required.` / `The calendar range must end on or after the date it starts.` / `The calendar range cannot exceed 92 days.`

---

### 3.7 `PUT /my/calendar/day` — يقفل أو يفتح يوم معيّن

**الوظيفة:** يستثني يوم كامل من النمط الأسبوعي (إجازة طارئة) أو يرجّعه.

**Body:**

```json
{ "date": "2026-11-09", "open": false }
```

| الحقل | نوع | ملاحظات |
|---|---|---|
| `date` | `yyyy-MM-dd` | لازم يكون النهاردة أو بعد كده **بتوقيت المدرّب** |
| `open` | bool | `false` = اقفل اليوم، `true` = افتحه |

**Response:** `200` و`message = "Availability for the selected date updated successfully."`

**أخطاء متوقعة:**

| الرسالة | إمتى |
|---|---|
| `The date must be today or later in the instructor's local time zone.` | تاريخ في الماضي |
| `There is no availability window on this date, so it cannot be opened or blocked.` | اليوم مفيش فيه أصلًا نافذة (مثال: الجمعة ومفيش نمط لجمعة) — **متعرضش الزرار غير لو اليوم فيه نوافذ** |
| `This day has sessions that are still scheduled. Reschedule or cancel them before blocking it.` | القفل (`open=false`) وفيه جلسات قائمة |

**ملاحظة تشغيلية:** الأمر ده بيقلّب `isOpen` على الصفوف الموجودة، ما بيولّدش صفوف جديدة. فلو اليوم مش ظاهر في `GET /my/calendar`، قفله مستحيل من الأساس.

---

## 4. Availability — إدارة الأدمن

نفس الـ base، و`[RoleAuthorize(Admin)]`. الفرق الوحيد: الأدمن بيتصرّف **لمدرّب تاني**، فهنا `instructorId` مطلوب.

### 4.1 `GET /pagginated?instructorId=&day=&pageNumber=&pageSize=`

صف واحد فيه كل نوافذ كل المدرّبين، مع اسم المدرّب وتخصصه — للجدول الإداري.

```json
{
  "id": "slot-id",
  "instructorId": "8c1f...",
  "instructorFullName": "Mohamed Saber",
  "specialization": "IELTS",
  "day": 3,
  "dayName": "Wednesday",
  "startTime": "09:00:00",
  "endTime": "11:00:00"
}
```

`instructorId` و`day` اختياريان كفلاتر.

### 4.2 `POST /instructors/{instructorId}/weekly-slots`

زي بتاع المدرّب بالظبط، بس الـ `instructorId` من الـ **route** (لو موجود في الـ body السيرفر بيطغّي قيمة الـ route عليه). الـ body: `day`, `startTime`, `endTime`.

**أخطاء إضافية:** `Instructor was not found.` و`Availability can only be managed for an approved instructor.` — يعني لازم يكون التقديم معتمد.

### 4.3 `PUT /weekly-slots/{id}` و `DELETE /weekly-slots/{id}`

نفس سلوك self-service بالظبط، بنفس رسائل الأخطاء.

---

## 5. Availability — قراءة المتعلّم

> **ملاحظة:** مبقاش موجودة الـ endpoint كانت بترجع توفّر مدرّب مختار للمتعلّم (`GET /instructor-availability/instructors/{instructorId}`) — اتشالت لأن كل حقولها كانت بتيجي أصلًا من أماكن تانية: الهُوية والـ `hourlyRate` من `GET /onboarding/instructors`، والـ slots اللي تهم الحجز من `GET /sessions/bookable-slots` (متأرخة وUTC جاهزة). المتعلّم مبيحتاجش الأوقات المحلية للمدرّب ولا منطقته.

### 5.1 `GET /onboarding/instructors?...` — فلترة قائمة المدرّبين بالوقت

الـ endpoint الموجود أصلًا، واتضاف له تلات باراميترات جديدة:

| البارام | نوع | ملاحظات |
|---|---|---|
| `day` | int 0–6 | اليوم الأسبوعي |
| `after` | `"HH:mm:ss"` | اختيارى — بداية النافذة المطلوبة |
| `before` | `"HH:mm:ss"` | اختيارى — نهاية النافذة المطلوبة |
| `minPrice` / `maxPrice` | decimal | موجود قبل كده |
| `specialization` / `countryId` | string / guid | موجود قبل كده |
| `pageNumber` / `pageSize` | int | موجود قبل كده |

**القاعدة:** المدرّب يظهر لو عنده **نافذة واحدة في اليوم ده بتغطي النافذة المطلوبة بالكامل**. لو سبت `after`/`before` من غير قيمة، النطاق بيتوسع على اليوم كله.

**أخطاء:** `A day of week is required when filtering by time.` (بعت `after` أو `before` من غير `day`) و`The availability time range must end after it starts.`

> **تنبيه:** الفلترة بتقارن بتوقيت المدرّب المحلي. لو المستخدم بيبحث "بعد 6 مساءً بتوقيتي"، الفرونت لازم يحوّل 18:00 المحلي بتاعه لوقت المدرّب قبل ما يبعت `after`.

---

## 6. Sessions والحجوزات

الـ base: `/api/v1/sessions` — `[RoleAuthorize(Instructor, Learner)]`.

### 6.1 دورة الحياة

```
المتعلّم يحجز              المدرّب يوافق              الوقت يعدي
     │                          │                        │
     ▼                          ▼                        ▼
  Pending (0)  ─────confirm──── Confirmed (1) ─────► Completed (2)
     │                              │
     │ decline                      │ cancel (قبل القفل)
     ▼                              ▼
  Cancelled (3)  ◄─────────────────┘
  NoShow (4)  ─ reserved for later
```

- **حجز المتعلّم بيخلق جلسة جديدة** بحالة `Pending`. الجلسة والـ booking بيتولدوا مع بعض.
- **`confirm` / `decline` للمدرّب بس**، وبيشتغلوا على `Pending` بس.
- **`Completed` و`NoShow` بيظبطوهم الـ background jobs** مش الفرونت.
- **Reschedule ما يغيّرش الـ status** — بيضيف طلب تاني بيتقبل أو يترفض من الناحية التانية.

### 6.2 الأقفال (24 ساعة / 12 ساعة)

| الجهة | القفل | إيه اللي بيتقفل |
|---|---|---|
| المدرّب | **24 ساعة** قبل `startsAtUtc` | `cancel` و`reschedule` |
| المتعلّم | **12 ساعة** قبل `startsAtUtc` | `cancel` و`reschedule` |

السيرفر بيطبّق القفل فعلًا برسالتين مختلفتين (`InstructorLockPassed` / `LearnerLockPassed`)، **بس المفروض الفرونت يعمل pre-check** ويعطّل الزرار قبل ما الطلب يمشي:

```js
const LOCK_HOURS = { instructor: 24, learner: 12 };

function canAct(session, role) {
  const msLeft = new Date(session.startsAtUtc) - Date.now();
  return msLeft > LOCK_HOURS[role] * 3600_000
      && (session.status === 0 || session.status === 1);
}
```

القيم دي في `SchedulingOptions` في الـ config؛ دلوقتي 24/12/15 دقيقة فاصل. لو اتغيرت في السيرفر مش هتعرف من الـ API، فمتخلّيهاش ثابتة في مكانين في الكود بتاعك.

### 6.3 `GET /sessions?tab=&pageNumber=&pageSize=` — جدول المدرّب

**الوظيفة:** مواعيد المدرّب اللي في التوكن.

| البارام | نوع | الافتراضي | معاني الـ tab |
|---|---|---|---|
| `tab` | int أو اسم | `0` | `0` = Pending + Confirmed والماضي مستثنى · `1` = Completed · `2` = Cancelled + NoShow |
| `pageNumber` | int | 1 | — |
| `pageSize` | int | 20 | max 50 |

**Response `data.items[]` = `SessionDto`:**

```json
{
  "id": "session-id",
  "instructorId": "8c1f...",
  "instructorFullName": "Mohamed Saber",
  "title": null,
  "type": 0,
  "startsAtUtc": "2026-11-09T07:00:00Z",
  "endsAtUtc": "2026-11-09T08:00:00Z",
  "status": 0,
  "price": 25.0,
  "seatCapacity": 1,
  "seatsBooked": 1,
  "cancelledAt": null,
  "cancelReason": null,
  "myBookingId": null,
  "myBookingStatus": null
}
```

**ملاحظات:**
- `title` حاليًا دايمًا `null` — مفيش شاشة بتجمعه لحد دلوقتي. متعرضوش.
- `myBookingId` / `myBookingStatus` **ليهم معنى في `my-bookings` بس**؛ في جدول المدرّب بترجع `null`.
- `price` = `HourlyRate` بتاع المدرّب للنافذة كلها (مش مضروبة في المدة) — قرار مؤقت لحد ما نظام الدفع يتحدد.
- الترتيب تصاعدي بالـ `startsAtUtc`.

**أخطاء:** `The session tab must be upcoming, completed or cancelled.` / `Page number must be greater than zero.` / `Page size must be between 1 and 50.`

### 6.4 `GET /sessions/my-bookings?tab=&pageNumber=&pageSize=` — جدول المتعلّم

نفس الباراميترات ونفس الـ DTO، بس كل عنصر فيه حجز للمتعلّم الحالي. هنا **استخدم** `myBookingId` و`myBookingStatus`:

| `myBookingStatus` | معنى للمستخدم |
|---|---|
| `0` Pending | مستني موافقة المدرّب — زر "Waitlist" |
| `1` Confirmed | متأكد |
| `2` Cancelled | هو ألغاه |
| `3` Declined | المدرّب رفضه |

الفلتر بيأخد بالحسبان حالة الحجز **وحالة الجلسة** الاتنين — يعني حجز Confirmed على جلسة Cancelled هيظهر في `tab=2`.

### 6.5 `GET /sessions/bookable-slots?instructorId=&from=&to=` — اختيار الميعاد

**الوظيفة:** النوافذ المؤرخة اللي لسه قابلة للحجز عند مدرّب معيّن — دي اللي بتبني منها wizard الحجز.

| البارام | نوع | ملاحظات |
|---|---|---|
| `instructorId` | guid | مطلوب |
| `from` | `yyyy-MM-dd` | مطلوب |
| `to` | `yyyy-MM-dd` | مطلوب، `>= from`، والمدة **≤ 92 يوم** |

**Response `data`:**

```json
[
  {
    "availabilityInstanceId": "d91b...",
    "date": "2026-11-09",
    "startsAtUtc": "2026-11-09T07:00:00Z",
    "endsAtUtc": "2026-11-09T08:00:00Z"
  }
]
```

**مش paginated** — مصفوفة خام في `data`.

**الفلتر:** النافذة مفتوحة + ماابتدتش + مفيش عليها جلسة قائمة.

**أخطاء:** `Instructor identifier is required.` / `Both the start and the end date are required.` / `The date range must end on or after the date it starts.` / `The date range cannot exceed 92 days.` / `This instructor is not taking new bookings right now.` (وضع الإجازة، أو الحساب غير نشط، أو التقديم مش معتمد).

**🔴 لو رجعت فاضية:** المتعلّم مالوش تشخيص — يعرض بس "لا توجد مواعيد متاحة حاليًا". التشخيص على سطح المدرّب: `timezoneId` في `GET /instructor-availability/my` لو `null` يبقى المدرّب من غير anchor فمفيش توليد خالص. ولو فيه منطقة، يبقى الأُفق 30 يوم خلص أو الـ slots الأسبوعية نفسها فاضية أو وضع الإجازة شغّال (قسم 8).

### 6.6 `POST /sessions` — يحجز

**الوظيفة:** يحجز نافذة مؤرخة كاملة. **مفيش اختيار مدة** — الحجز بياخد النافذة كما هي.

**Body:**

```json
{
  "instructorId": "8c1f...",
  "availabilityInstanceId": "d91b..."
}
```

الاتنين مطلوبين، و`availabilityInstanceId` هو الـ `id` من `available` في الـ calendar أو الـ `availabilityInstanceId` من bookable-slots.

**Response:** `201` و`data` = `SessionDto` بحالة `status: 0`.
`message = "Booking created successfully. It becomes confirmed once the instructor accepts it."`

**كل أسباب الرفض (400 في `errors`):**

| الرسالة | إمتى |
|---|---|
| `This availability slot is not open for booking.` | مقفول أو اتحذف |
| `This availability slot has already started.` | المعاد عدى |
| `The instructor already has a session at this time.` | تعارض في أجندة المدرّب |
| `The instructor must keep at least 15 minutes between two sessions.` | فاصل أقل من 15 دقيقة |
| `You already have another session at this time.` | تعارض في أجندة المتعلّم |
| `You must keep at least 15 minutes between two sessions.` | فاصل أقل من 15 دقيقة |
| `This instructor is not taking new bookings right now.` | إجازة / حساب موقوف / غير معتمد |
| `Availability slot was not found.` | الـ `id` غلط |

**تسلسل منطقى للـ wizard:** `GET /onboarding/instructors` (الهُوية + الـ `hourlyRate` من الكارت) ← `GET /sessions/bookable-slots` ← `POST /sessions`. وضع الإجازة مش بيتقرأ من أي surface للمتعلّم — بيظهر أول ما `bookable-slots` يرجّع 400 بالرسالة دي، فاستخدمها كحالة فارغة.

### 6.7 `GET /sessions/{id}` — تفاصيل الجلسة

**الوظيفة:** صفحة الجلسة: بياناتها + قائمة الحاضرين + طلبات النقل.

**الصلاحية:** المدرّب صاحب الجلسة أو أي متعلّم ليه حجز عليها — غير كده **403**.

**Response `data` = `SessionDetailDto`:**

```json
{
  "session": { /* SessionDto */ },
  "bookings": [
    {
      "id": "booking-id",
      "sessionId": "session-id",
      "learnerId": "77aa...",
      "learnerFullName": "Omar Khaled",
      "status": 0,
      "cancelledAt": null,
      "cancelReason": null
    }
  ],
  "rescheduleRequests": [
    {
      "id": "req-id",
      "sessionId": "session-id",
      "requestedByUserId": "77aa...",
      "requestedByType": 0,
      "fromUtc": "2026-11-09T07:00:00Z",
      "toUtc": "2026-11-09T08:00:00Z",
      "status": 0,
      "createdAt": "2026-11-05T10:22:41Z"
    }
  ]
}
```

**🔴 الفرق في الصلاحيات داخل الرد:** `bookings` بتترجع **للمدرّب بس**. المتعلّم بيشوفها **مصفوفة فاضية** — متعرضش "قائمة الحاضرين" للمتعلّم، ومتفرّقتهاش بين "فاضية" و"مش مسموح" لأن الاتنين بيبانوا نفس الحاجة. بيانات المتعلّم نفسه موجودة في `session.myBookingId` / `session.myBookingStatus`.

`rescheduleRequests` بتترجع للناحين.

### 6.8 `POST /sessions/{id}/confirm` و `POST /sessions/{id}/decline` — رد المدرّب

**الوظيفة:** يقبل/يرفض حجز لسه `Pending`. **للمدرّب صاحب الجلسة بس**، وعلى `Pending` بس.

مفيش body. الـ `id` من الـ route.

| | confirm | decline |
|---|---|---|
| الكود | `200` | `200` |
| الـ `data` | `SessionDto` بحالة `1` | `SessionDto` بحالة `3` |
| الأثر | الجلسة والـ bookings كلها تتحول Confirmed | الحجز يتحول Declined والمقعد يتحرر والجلسة تلغى |
| الرسالة | `Session confirmed successfully.` | `Session declined successfully.` |

**أخطاء:** `Only a pending session can be answered this way.` (لو اتقبل قبل كده أو اتلغى) و`Session was not found.` و403 لو مش مدرّب الجلسة.

### 6.9 `POST /sessions/{id}/cancel` — إلغاء، للناحين من نفس الـ route

**الوظيفة:** المدرّب يلغي الجلسة كلها، أو المتعلّم يتنازل عن مقعده. **السيرفر بيعرف الدور من التوكن** — انت ما بتبعتش دورك.

**Body:**

```json
{ "reason": "Sick leave" }
```

`reason` اختياري، و**حدّه الأقصى 1000 حرف**.

**السلوك حسب الدور:**

| الدور | لو لسه فيه مقاعد محجوزة | الأثر |
|---|---|---|
| المدرّب | — | يلغي الجلسة وكل الحجوزات، بغض النظر عن العدد |
| المتعلّم | فيه ناس تانية | بيحرر مقعده **فقط**، والجلسة تفضل شغالة |
| المتعلّم | هو الوحيد | بيحرر مقعده **ويلغي الجلسة** |

**Response:** `200` و`data` = `SessionDto` بعد العملية، `message = "Cancellation completed successfully."`

**أخطاء:** `This session has already ended or was called off.` (مش Pending/Confirmed أو وقته عدى) · `The session starts in less than 24 hours, so the instructor can no longer cancel or reschedule it.` · `The session starts in less than 12 hours, so the learner can no longer cancel or reschedule it.` · `Cancellation reason cannot exceed 1000 characters.`

**في الـ UI:** متعرفش مسبقًا هل الإلغاء ده هيلغي الجلسة ولا هيسيبها شغالة — الفرق ده في الداتا. عرض رسالة عامة ("Sure cancel your seat?") وارجع الرد وقرر من `session.status`.

### 6.10 `POST /sessions/{id}/reschedule` — يقترح ميعاد جديد

**الوظيفة:** يرفع طلب نقل. **مفيش حاجة بتتحرك لحد ما الناحية التانية ترّد.**

**Body:**

```json
{
  "fromUtc": "2026-11-12T07:00:00Z",
  "toUtc": "2026-11-12T08:00:00Z"
}
```

**🔴 المدة لازم تتطابق حرفيًا:** `toUtc - fromUtc` = `endsAtUtc - startsAtUtc` للجلسة الحالية. مفيش تغيير مدة مع النقل.

**Response:** `201` و`data` = `RescheduleRequestDto` بحالة `status: 0`.

**أخطاء:**

| الرسالة | إمتى |
|---|---|
| `The proposed window must start in the future and end after it starts.` | ميعاد في الماضي أو مقلوب |
| `This session can no longer be rescheduled.` | الجلسة خلصت أو اتلغت |
| `A reschedule request for this session is already waiting for an answer.` | فيه طلب `status: 0` تاني — **اقفل الزرار** لو `rescheduleRequests` فيها أي طلب غير مُجاب |
| `The proposed window must be the same length as the current session.` | المدة اختلفت |
| `The session starts in less than 24 hours...` / `...12 hours...` | القفل بتاع الدور |

### 6.11 `POST /reschedule-requests/{id}/approve` و `/decline` — الرد على النقل

**الوظيفة:** الناحية التانية ترد. **اللي رفع الطلب ما لوش يرد على نفسه** — المدرّب لو هو اللي طلب، المتعلّم يرد، والعكس.

الـ `id` هنا = الـ `id` جوه `rescheduleRequests[]` في تفاصيل الجلسة. مفيش body.

| | approve | decline |
|---|---|---|
| الكود | `200` | `200` |
| الـ `data` | `SessionDto` بالميعاد الجديد | `RescheduleRequestDto` بحالة `2` |
| الأثر | الجلسة والـ bookings تنتقل للميعاد الجديد | الجلسة تفضل على ميعادها |
| الرسالة | `Reschedule request approved and the session moved.` | `Reschedule request declined and the session kept its time.` |

**`approve` بيعيد فحص التعارض وقت التنفيذ** (لأن الوقت ممكن يكون اتغير بين الطلب والرد):

| الرسالة | إمتى |
|---|---|
| `This reschedule request was already answered.` | حد رد قبله |
| `The proposed window no longer fits the instructor's availability or the calendar of either side.` | الميعاد بقى متعارض أو بره availability — **اعرض للمستخدم يختار ميعاد تاني** |
| `Reschedule request was not found.` | `id` غلط |
| 403 | هو اللي رفع الطلب |

---

## 7. خريطة الشاشات → الـ endpoints

| الشاشة | الدور | الخطوات |
|---|---|---|
| إعداد التوفير | مدرّب | `GET /my` → `POST/PUT/DELETE /my/weekly-slots` → `PUT /my/vacation-mode` |
| تقويم المدرّب | مدرّب | `GET /my/calendar?from&to` → `PUT /my/calendar/day` |
| ملف المدرّب | متعلّم | الكارت من `GET /onboarding/instructors` → `GET /sessions/bookable-slots?instructorId&from&to` للمواعيد الفعلية |
| بحث المدرّبين | متعلّم | `GET /onboarding/instructors?day&after&before&minPrice...` |
| wizard الحجز | متعلّم | `GET /sessions/bookable-slots?instructorId&from&to` → `POST /sessions` |
| حجوزاتي | متعلّم | `GET /sessions/my-bookings?tab` → `GET /sessions/{id}` → `POST /sessions/{id}/cancel` أو `/reschedule` |
| مواعيدي | مدرّب | `GET /sessions?tab` → `GET /sessions/{id}` → `POST /sessions/{id}/confirm` أو `/decline` |
| طلبات النقل | الناحيتين | `GET /sessions/{id}` (قسم `rescheduleRequests`) → `POST /reschedule-requests/{id}/approve` أو `/decline` |
| إدارة الأدمن | أدمن | `GET /instructor-availability/pagginated` → `POST /instructors/{id}/weekly-slots` → `PUT/DELETE /weekly-slots/{id}` |

---

## 8. إمتى الـ dates بتتولّد — وهل الـ flow شغال دلوقتي؟

`/sessions/bookable-slots` بيقرأ من جدول `AvailabilityInstances` — وهي الصفوف المؤرخة اللي بتتولّد من النمط الأسبوعي + منطقة المدرّب. **التوليد بيحصل بطريقتين مختلفتين**، والفرق ده يحدّد انت تقدر تخت إيه دلوقتي:

| المسار | إمتى بيشتغل | Hangfire مطلوب؟ |
|---|---|---|
| **فوري** | `POST` / `PUT` / `DELETE` على `my/weekly-slots` **ونسخ الأدمن منها** — كل أمر بيفنادي توليد المدرّب ده قبل ما الرد يخلص، فالصفوف بتتحفظ في الداتابيز على طول | **لأ** |
| **دوري** | job كل يوم 02:00 UTC (`scheduling-materialize-availability`) يوسّع الأُفق ويلحق التعديلات، وjob كل 10 دقايق (`scheduling-expire-sessions`) يعلّم المنتهي | **أيوه** |

حالة السيرفر دلوقتي: `Hangfire:Enabled = false`.

**الترتيب اللي محتاجه الدوران الدوري يشتغل:**

1. `2026-11-04-scheduling.sql` — الجداول الأربعة + العمودين (✅ متطبّق).
2. `2026-11-04-country-timezones.sql` — يحشي `Countries.TimezoneId` (✅ متطبّق: 185 دولة من 230).
3. `2026-11-04-hangfire-schema.sql` — سكيمـا Hangfire، **مطلوب** لأن `PrepareSchemaIfNecessary = false`.
4. يقلّب `Hangfire:Enabled` لـ `true`.

### يعني أقدر أختبر الحجز حالًا؟

**أيوه.** المدرّب لما يدخّل نافذة أسبوعية، الأيام المؤرخة بتتعمل فورًا لمدّة `DaysAhead = 30` يوم قدام، فـ `GET /my/calendar` و`GET /sessions/bookable-slots` و`POST /sessions` كلهم شغالين من غير Hangfire.

### اللي مش هيحصل غير ما Hangfire يتفعّل

| الحاجة | الأثر على الفرونت |
|---|---|
| الأُفق ما بيتمشيش — بعد 30 يوم مفيش صفوف جديدة | التقويم هيبقى فاضي بعد الأُفق، و`bookable-slots` هيدي فاضي بعد 30 يوم |
| تعديل الأدمن لـ `TimezoneId` بتاعة بلد ما بينعكسش على الأيام الموجودة | أيام المدرّبين التابعين للبلد دي تفضل غلط لحد ما حد يلمس نافذة يدويًا |
| الجلسات اللي وقتها عدّى ما بتتحولش `Completed` | **مهم:** الجلسة المنتهية بتختفي من `tab=0` (لأن `endsAtUtc > now` بيبطل) وما بتظهرش في `tab=1` ولا `tab=2` — يعني **بتختفي من كل التابات** لحد ما الـ job يشتغل. متستنتجش "مفيش مواعيد" من `tab=1` |

**وفي كل الأحوال:** الصفحة تتصمم إنها تتحمل الفراغ — حالة فارغة واضحة مش crash أو spinner للأبد.

---

## 9. تنبيهات وقرارات مؤقتة

| # | الحاجة | الأثر على الفرونت |
|---|---|---|
| 1 | `Session.price` = `HourlyRate` للنافذة كلها، مش مضروبة في المدة | متحسبش انت سعر — اعرض `price` زي ما هي، ومتقلش "1 hour = 25, 2 hours = 50" |
| 2 | مفيش نظام دفع لسه (Phase 3 متاجل) | متعملش شاشة payment. `price` للعرض بس |
| 3 | `SessionType.Group` موجود في الموديل بس **مفيش أي كود بينشئه** | كل الجلسات `type: 0`. متعملش UI للمجموعات دلوقتي |
| 4 | `seatCapacity` حاليًا دايمًا `1` | متعرضش "3/5 مقاعد اتحجزت" — مفيش مصدر للـ 5 |
| 5 | متعلّمان يحجزا نفس النافذة في نفس اللحظة ممكن ينجح الاتنين (سباق معروف) | متحلّش المشكلة في الفرونت؛ متعتمدش على `seatsBooked` كرقم لحظي |
| 6 | 45 دولة لسه `timezoneId = null` (الدول متعددة المناطق: `+1`, `+7`, `+52`, `+55`, `+61`, `+62`...) | مدرّبين من دول هيرفضوا بـ `TimezoneUnresolved` — عرض رسالة واضحة مش خطأ تقني |
| 7 | `title` بترجع `null` دايمًا | متعرضش الحقل |
| 8 | المنصة إنجليزي فقط حاليًا | كل الرسائل راجعة إنجليزي من السيرفر؛ متترجمهاش انت، بس متكتبهاش hardcoded كمان — خدّها من `message` |
| 9 | وضع الإجازة **مش** في instructor profile | اقرأه من `GET /my` أو calendar، متدوّرش عليه في `/instructor-profile` |
| 10 | `Completed` و`NoShow` بيحددهم الـ jobs مش الـ API | متعملش زرار "علّم كمكتملة" — مفيش endpoint ليه |

---

## 10. أمثلة curl سريعة

```bash
TOKEN='<jwt>'
BASE='https://lms-api.runasp.net/api/v1'
H=(-H "Authorization: Bearer $TOKEN" -H 'Accept: application/json' -H 'Accept-Language: En')

# 1) توفيري الأسبوعي
curl "${H[@]}" "$BASE/instructor-availability/my"

# 2) أضيف نافذة (بدون instructorId نهائيًا)
curl -X POST "${H[@]}" -H 'Content-Type: application/json' \
  "$BASE/instructor-availability/my/weekly-slots" \
  -d '{"day":4,"startTime":"14:00:00","endTime":"16:30:00"}'

# 3) تقويمي
curl "${H[@]}" "$BASE/instructor-availability/my/calendar?from=2026-11-04&to=2026-11-18"

# 4) اقفل يوم
curl -X PUT "${H[@]}" -H 'Content-Type: application/json' \
  "$BASE/instructor-availability/my/calendar/day" \
  -d '{"date":"2026-11-09","open":false}'

# 5) وضع الإجازة
curl -X PUT "${H[@]}" -H 'Content-Type: application/json' \
  "$BASE/instructor-availability/my/vacation-mode" -d '{"enabled":true}'

# 6) النوافذ القابلة للحجز لمدرّب
curl "${H[@]}" "$BASE/sessions/bookable-slots?instructorId=8c1f...&from=2026-11-04&to=2026-11-18"

# 7) احجز
curl -X POST "${H[@]}" -H 'Content-Type: application/json' \
  "$BASE/sessions" -d '{"instructorId":"8c1f...","availabilityInstanceId":"d91b..."}'

# 8) مواعيدي (المدرّب) — القادمية
curl "${H[@]}" "$BASE/sessions?tab=0&pageNumber=1&pageSize=20"

# 9) قبل الحجز
curl -X POST "${H[@]}" "$BASE/sessions/session-id/confirm"

# 10) اقترح ميعاد جديد (نفس مدة الجلسة بالظبط)
curl -X POST "${H[@]}" -H 'Content-Type: application/json' \
  "$BASE/sessions/session-id/reschedule" \
  -d '{"fromUtc":"2026-11-12T07:00:00Z","toUtc":"2026-11-12T08:00:00Z"}'

# 11) رد على طلب النقل (الناحية التانية بس)
curl -X POST "${H[@]}" "$BASE/reschedule-requests/req-id/approve"

# 12) حجوزاتي (المتعلّم)
curl "${H[@]}" "$BASE/sessions/my-bookings?tab=0"
```

---

## 11. مرجع سريع لكل الـ endpoints

| # | Method | Route | الدور | الوظيفة في سطر |
|---|---|---|---|---|
| 1 | GET | `/instructor-availability/my` | Instructor | slots الأسبوع + المنطقة + وضع الإجازة |
| 2 | POST | `/instructor-availability/my/weekly-slots` | Instructor | يضيف نافذة أسبوعية متكررة |
| 3 | PUT | `/instructor-availability/my/weekly-slots/{id}` | Instructor | يعدّل نافذة |
| 4 | DELETE | `/instructor-availability/my/weekly-slots/{id}` | Instructor | يحذف نافذة |
| 5 | PUT | `/instructor-availability/my/vacation-mode` | Instructor | يوقف/يفتح استقبال الحجوزات |
| 6 | GET | `/instructor-availability/my/calendar?from&to` | Instructor | الأيام المؤرخة: متاح/مقفول/محجوز |
| 7 | PUT | `/instructor-availability/my/calendar/day` | Instructor | يقفل أو يفتح يوم كامل |
| 8 | GET | `/instructor-availability/pagginated` | Admin | جدول كل نوافذ كل المدرّبين |
| 9 | POST | `/instructor-availability/instructors/{instructorId}/weekly-slots` | Admin | يضيف نافذة لمدرّب |
| 10 | PUT | `/instructor-availability/weekly-slots/{id}` | Admin | يعدّل نافذة لمدرّب |
| 11 | DELETE | `/instructor-availability/weekly-slots/{id}` | Admin | يحذف نافذة لمدرّب |
| 12 | GET | `/onboarding/instructors?day&after&before` | Learner | قائمة المدرّبين مفلترة بالوقت |
| 13 | GET | `/sessions?tab&page&size` | Instructor | مواعيد التدريس |
| 14 | GET | `/sessions/my-bookings?tab&page&size` | Learner | حجوزاتي |
| 15 | GET | `/sessions/bookable-slots?instructorId&from&to` | Learner | النوافذ القابلة للحجز |
| 16 | POST | `/sessions` | Learner | يحجز نافذة → Pending |
| 17 | GET | `/sessions/{id}` | الطرفين | تفاصيل + roster (مدرّب) + طلبات النقل |
| 18 | POST | `/sessions/{id}/confirm` | Instructor | يقبل الحجز |
| 19 | POST | `/sessions/{id}/decline` | Instructor | يرفض الحجز |
| 20 | POST | `/sessions/{id}/cancel` | الطرفين | يلغي الجلسة أو يتنازل عن المقعد |
| 21 | POST | `/sessions/{id}/reschedule` | الطرفين | يقترح ميعاد جديد |
| 22 | POST | `/reschedule-requests/{id}/approve` | الناحية التانية | ينفّل النقل |
| 23 | POST | `/reschedule-requests/{id}/decline` | الناحية التانية | يرفض النقل |

كل المسارات نسبية لـ `https://lms-api.runasp.net/api/v1/`.
