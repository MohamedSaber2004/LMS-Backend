# الحجز والدفع — handoff للفرونت (وقت بداية دقيق + من غير موافقة المدرس)

التاريخ: 2026-11-12 · الباك إند فقط — الفرونت بره الريبو ده، فكل حاجة تحت **متعاقد عليه من الكود** بس لسه ما اتجربش في المتصفح.

## الفكرة في ٤ سطور

- الطالب بيبعت **تاريخ + وقت بداية `HH:mm` + مدة**، والباك إند يتأكد إن الفترة دي كلها جوه نافذة إتاحة مفتوحة للمدرس ده في اليوم ده، ومفيش تعارض مع حصصه هو أو المدرس.
- **مفيش خطوة "المدرس يأكد الحجز"**. الـ `POST /sessions/{id}/confirm` والـ `/decline` انمسحوا من الـ API.
- الحجز بيطلع **`Pending` = محجوز ومش مدفوع**. الطالب بيضغط دفع → `Confirmed` + اتخصمت الساعات + القاعة اتفتحت ليه وللمدرس — من غير أي تدخل من المدرس.
- الفلوس بتتحرك لوحدها: وقت الدفع صافي حق المدرس بينزل `PendingBalance`، ولما الحصة تخلص بينقال لـ `AvailableBalance` (ونسبة المنصة مسجلة على نفس الـ transaction).

## آلة الحالة — دلوقتي

| `session.status` | معناه | المقعد | الفلوس |
|---|---|---|---|
| `0 Pending` | اتحجز ومش مدفوع (Trial: محجوز وجاهز فورًا) | محتوط | مفيش حاجة اتخصمت |
| `1 Confirmed` | **مدفوع** وجاهز للدخول في الميعاد | محتوط | صافي المدرس في `PendingBalance` |
| `2 Completed` | خلصت (confirm-attendance أو الـ job بعد `endsAt`) | اتحرر | الصافي اتنقل لـ `AvailableBalance` + `TotalEarned` |
| `3 Cancelled` | ملغى (طالب/مدرس/أدمن/انقضاء وقت البداية) | اتحرر | الـ Held بقى `Reversed` والساعات اتردّت |

الأرقام دي بتوصل في JSON زي ما هي (`status: 1`) — مفيش enum-as-string.

`booking.status` للحجز الجديد دايمنًا `1 Confirmed` من لحظة الإنشاء (الـ `Pending` القديم معناه "مستني المدرس" — المفهوم ده اتلغى)، و`Pending || Confirmed` بيتعاملوا معاهم إنهم "مقعد محتوط".

## ⚠️ كسر التعاقد (لازم الفرونت يتعدل)

1. **`startTime` بقى إلزامي** في `POST /api/v1/sessions`. من غيره = 400.
2. **`POST /api/v1/sessions/{id}/confirm` و `POST /api/v1/sessions/{id}/decline` اتحذفوا** — أي شاشة/زر بستدعيهم هيلاقي 404. المدرس بقى مالهوش حاجة يعملها في الحجز غير الإلغاء أو طلب تغيير الميعاد.
3. رسالة نجاح الحجز اتغيرت نصها (تحت في قسم الحجز) — لو في شاشة بتطابق على النص القديم تتعدل.
4. **`openapi/v1.json` و collection Postman لسه قدام** — دول بيتولّدوا من الـ CI، والـ CI لسه ما شافش الكوميت ده. المتطلبات في الريبو مش متزامنة.
5. الـ migration بتاعة الـ `Status` **معلّقة** (قسم "migration معلّقة" تحت) — الكود الجديد بيقرا/يكتب العمود ده، فلو اتنشر من غير ما تتطبق كل عمليات الدفع هتفشل.

## `POST /api/v1/sessions` — الحجز (الطالب)

`instructorId` و `date` و `startTime` و `durationMinutes` كلها في الـ body:

```json
{
  "instructorId": "8f2c…",
  "date": "2026-11-15",
  "startTime": "14:30",
  "durationMinutes": 50
}
```

| حقل | نوع | ملاحظات |
|---|---|---|
| `instructorId` | Guid | لازم موجود وإلا 400 (الرسالة بيسمّي `InstructorNotFound`) |
| `date` | `"yyyy-MM-dd"` | يوم **بتوقيت المدرس** المحلي — مش UTC |
| `startTime` | TimeSpan: `"14:30"` أو `"14:30:00"` | **بتوقيت المدرس** كمان. نفس شكل `startTime` اللي بتبعتوه فعلًا في حفظ نوافذ الإتاحة |
| `durationMinutes` | `25` أو `50` | أي رقم تاني = 400. أي مدة تانية (مثلاً 30) = 400 |

`startTime` بيبعت كـ `TimeSpan` عادي مش ISO datetime: `"2026-11-15T14:30:00"` **غلط**، و`"14:30"` صح (الثواني اختيارية، والـ ثواني اللي بتيجي من `new Date()` بتتجاهَل بالتقطيع للدقائق).

### اللي بيتعمل جوه

- التوقيت المحلي + التاريخ بيتحولوا لـ UTC بإطار `Countries.TimezoneId` بتاع المدرس.
- التحقق: المدرس بيقبل حجوزات؟ ← الساعة موجودة في اليوم ده في منطقته (DST)؟ ← مش في الماضي؟ ← **الفترة `startTime → startTime + duration` جوه نافذة مفتوحة واحدة بالكامل** ← لا تعارض مع حصة للمدرس/للطالب ولا كسر للاستراحة الدنيا (15 دقيقة، `Scheduling:MinimumBreakMinutes`).
- نافذة الـ 3 ساعات **ممكن عليها أكثر من حجز** — نفس المبدأ اللي بيفلتح لك أكتر من حصة في اليوم.
- أول حصة للطالب = **Trial**: بتبقى `Confirmed` فورًا، سعرها 0، وجاهزة للدخول في الميعاد من غير دفع.
- غير الـ Trial: لازم يبقى فيه ساعات كفاية في باقة نشطة (`0.5` لمدة 25، `1.0` لمدة 50) — والساعات **بتتخصم وقت الدفع مش وقت الحجز**.
- **قاعة البث بتتنفس دلوقتي هنا** (كانت وقت تأكيد المدرس)، يعني `live` في الـ response فيها `roomId` من أول ثانية.
- `price` في الرد = `HourlyRate × المدة بالساعات` (تقريب لرقمين) — رقم للتوضيح، الحساب الفعلي بساعات الباقة.

نجاح: `201` + `SessionDto`، و`message` =
- غير Trial: `Booking created and your seat is held. Pay for the session before it starts to unlock the live room.`
- Trial: `Free trial lesson booked. It is ready to join at the scheduled time.`

### أخطاء الـ 400 (كلها تسمّي الحالة والتوقيت الفعلي)

| الحالة | `errors[0]` حرفيًا |
|---|---|
| `startTime` ناقص | `Choose the time of day the session should start.` |
| `startTime` بره 00:00–23:59 | `The session start time must be a time of day between 00:00 and 23:59.` |
| `date` ناقص | `Choose the date you want to book on.` |
| `durationMinutes` مش 25/50 | `Session duration must be either 25 or 50 minutes.` |
| التاريخ في الماضي | رسالة الإتاحة `DateInPast` (زي الأول) |
| المدرس مقفول | `This instructor is not taking new bookings right now.` |
| مفيش منطقة زمنية للمدرس | رسالة الإتاحة `TimezoneUnresolved` (زي الأول) |
| الساعة مش موجودة في اليوم ده (DST) | `14:30 does not exist on 2026-11-15 in this instructor's time zone. Pick another time.` |
| الوقت عدّى | `14:30 on 2026-11-15 has already passed, so it cannot be booked. Pick a later time.` |
| مفيش أي نافذة مفتوحة في اليوم | `This instructor has no open availability window on 2026-11-15, so nothing can be booked that day.` |
| الفترة بره النوافذ | `14:30 on 2026-11-15 is outside this instructor's open availability. The windows that day run 09:00 to 17:00, and a 50-minute session must fit entirely inside one of them.` (حدود النوافذ **بتوقيت المدرس**) |
| المدرس مشغول | `14:30 on 2026-11-15 cannot be booked: The instructor already has a session at this time.` |
| استراحة المدرس | `… cannot be booked: The instructor must keep at least 15 minutes between two sessions.` |
| الطالب مشغول | `… cannot be booked: You already have another session at this time.` |
| استراحة الطالب | `… cannot be booked: You must keep at least 15 minutes between two sessions.` |
| مفيش ساعات في الباقة | `Insufficient package session balance. Please subscribe to a package plan first.` |

و`404 SessionNotFound` لو المدرس نفسه مش موجود، و`SlotNotFound` بقى نصها: `There is no longer a bookable availability window for this instructor at the selected time.` (دي حالة سباق نادرة: الـ validator عدّى والـ handler لقى النافذة اتقفلت بينها).

المنطق كله في [CreateBookingCommandValidator.cs:43](file:///E:/LMS/LMS.Application/Features/Sessions/Commands/CreateBooking/CreateBookingCommandValidator.cs#L43) والتحقق الزمني في [SessionsValidation.cs:34](file:///E:/LMS/LMS.Application/Features/Sessions/SessionsValidation.cs#L34).

## `POST /api/v1/sessions/{id}/pay` — الدفع (الطالب)

`id` في الـ route، **مفيش body**. بيخصم الساعات من أقدم باقة نشطة كافية، بيحوّل الحصة لـ `Confirmed`، بيسمّيها `IsPaid` + `PaidAtUtc`، وبيعمل hold لصافي المدرس.

| حارس | الرد |
|---|---|
| اتدفع قبل كده | 400 `Payment for this session has already been completed.` |
| مش `Pending` | 400 `This session is {status}, so there is nothing left to pay for it.` (مثلاً `This session is Completed, …`) |
| وقت البداية عدّى | 400 `This session was due to start at 2026-11-15 14:30 UTC, so it can no longer be paid for.` |
| مفيش باقة كافية | 400 `Insufficient package session balance. Please subscribe to a package plan first.` |
| مش حجزه | 403 |

نجاح: `200` + `SessionDto` (`isPaid: true`, `status: 1`, `live` جاهزة) و`message` = `Session payment completed successfully. You can now start the session at the scheduled time.`

**الفروق اللي الفرونت لازم يراعيها:** الدفع مقفول بعد `startsAtUtc` — حتى دقيقة واحدة. ولأن الحجز غير المدفوع بيتلغى نفسه عند وقت البداية، الزر لازم يكون ظاهر/مفعّل بين الحجز و`startTime` بس. التريال مفيش ليه `pay` أصلًا (محققة `Confirmed` من البداية).

الكود: [PaySessionCommand.cs:59](file:///E:/LMS/LMS.Application/Features/Sessions/Commands/PaySession/PaySessionCommand.cs#L59)

## فتح القاعة = الدفع

`/start` (المدرس) و`/join` (الطالب) بترفضوا لو الحصة مش `Confirmed` أو مش مدفوعة ومن غير تريال:

- `Session must be confirmed before accessing live class. Current status: {0}.`
- `The session has not been paid for. Payment must be completed before joining.`

دي `ValidationException` (400). الحارس القديم كان "المدرس عمل confirm"؛ بقى "الطالب دفع". **ملاحظة مهمة:** الحجز نفسه بينجح حتى لو إعدادات LiveKit في البيئة فاضية — نقلنا إنشاء `LiveRoom` للحجز بس **نقلناش** حارس `ILiveClassTokenService.IsConfigured`، لأن وضعه على الحجز كان هيوقف كل الحجوزات على الإنتاج اللي إعدادات LiveKit فيه لسه فاضية. يعني الفشل هيحصل عند `/start` و`/join` وليس عند الحجز.

## `GET /api/v1/sessions/bookable-slots`

`instructorId` + `from` + `to` (+ `timeZoneId` اختياري). الرد **نوافذ** مش خانات: `availabilityInstanceId`, `date`, `startsAtUtc`, `endsAtUtc` و`localStartsAt`/`localEndsAt`/`localDate`/`timeZoneId` لو بعت `timeZoneId`.

الفرونت يختار من النافذة ساعة `HH:mm` ويبعتها في `startTime` — لازم الفترة كلها داخل النافذة. اللي اتغير هنا: **النافذة اللي عليها حجز غير مدفوق ووقته عدّى بتفضل بتظهر متاحة** (الحجز الميت مابقاش محتوط مقعد حتى قبل ما الـ job يلحقه)، فالقايمة والسlots اللي الحجز بيصادق عليهم بقوا بنفس القاعدة.

## الإلغاء (`POST /api/v1/sessions/{id}/cancel`)

- **المدرس**: بيلغي الحصة، بيرد ساعات الطالب، **وبيعكس الـ Held** في محفظة المدرس (يبقى `Reversed` و`PendingBalance` ينقص).
- **الطالب**: بيسيب مقعده. لو تبقى مقعد واحد فاضي على الأقل والحصة مدفوعة، الحصة بتتلغى والـ Held بيتعكس برضه. ولو لسه مش مدفوعة (Pending) مفيش فلوس تتعكس أصلًا.
- القاعة اللي `Live` بتمنع الإلغاء (400 `CannotCancelWhileLive`) — زي الأول.
- الأدمن: `POST /api/v1/admin/sessions/{id}/cancel` لسه بنفس السلوك + الـ refund القديم.

## `POST /api/v1/sessions/{id}/confirm-attendance` (الطالب)

بقّى **تعليم حضور بس**: بيرفع `IsAttendanceConfirmed` ويحوّل الحصة لـ `Completed`. بلوك الفلوس اللي كان هنا **اتشال** (كان بيحوّل الصافي لـ AvailableBalance؛ الده بقى شغل الـ sweep بعد `endsAtUtc`، أو العكس لو اتلغت). رسالة النجاح `Session attendance confirmed successfully.`

## الفلوس: الـ ledger بدل زر المدرس

وقت الدفع، على `TeacherWalletTransaction` واحد بنوع `SessionEarning (0)`:

| عمود | القيمة |
|---|---|
| `Amount` | صافي المدرس = `gross − commissionAmount` |
| `GrossAmount` | `session.price` (أو `HourlyRateSnapshot × الساعات` لو 0) |
| `CommissionAmount` / `CommissionRate` | نسبة المنصة بالجنيه/الدولار وبالنسبة المئوية (snapshot وقت الحجز) |
| `Status` | `0 Held` → `1 Released` أو `2 Reversed` |

وحينها `TeacherWallet.PendingBalance += Amount` بس. **مفيش جدول جديد للمنصة**: نسبة المنصة محسوبة من نفس الـ transactions، و`GET /api/v1/admin/finances/overview` بيجمعها واستثنى الـ `Reversed` (من غير استثناء كانت هتفضل محتسبة حتى بعد الإلغاء).

التحرير بيحصل في [SessionLifecycleService.cs:86](file:///E:/LMS/LMS.Infrastructure/Services/Scheduling/SessionLifecycleService.cs#L86): كل `Held` لحصة غير ملغية و`endsAtUtc ≤ الآن − 30 دقيقة` → `PendingBalance −=`، `AvailableBalance +=`، `TotalEarned +=`. الـ sweep ده بيشتغل **دائمًا** حتى لو مفيش حصص اتقفلت في نفس الدورة (كان باگ: الحصة اللي اتعلمّلت حضور و`Completed` ما كانش بيحرر فلوسها أبدًا).

الـ 30 دقيقة دي `Scheduling:JoinTailMinutes` (افتراضي 30، و`appsettings.json:37`). يعني الفلوس بتوصل المدرس بعد نهاية الحصة بنص ساعة — من غير ما حد يضغط حاجة. والسحب (`WithdrawEarnings`) بيقدر بس اللي `Released`.

## التنظيف التلقائي

Hangfire job `scheduling-expire-sessions` كل **10 دقايق** (`*/10 * * * *`):
- حصة `Pending` و`startsAtUtc ≤ الآن` → بتتلغى، وحجزها بيتلغى، والمقعد بيتحرر. (النافذة بتظهر متاحة فورًا للغير حتى قبل الـ job.)
- حصة `Confirmed` و`endsAtUtc ≤ الآن − 30 دقيقة` → `Complete()`.
- وبعدهم sweep التحرير فوق.

و`scheduling-cleanup-live-rooms` كل 5 دقايق بيقفل القاعات اليتيمة. الـ job ده شغال في التلات بيئات (`Scheduling` مش disabled) — بس مفيش أي cron جديد اتضاف للشغل ده.

## migration معلّقة

`20261112150008_AddWalletEarningReleaseStatus` — additive بس: عمود `Status int NOT NULL DEFAULT 1` على `TeacherWalletTransactions`. الـ default = `Released` (1) مش `Held` عشان الصفوف القديمة **اتحررت فعلًا** ولازم ما ترجعش Pending release. لسه **ما اتطبقتش** على الداتابيز الحيّة (مستني "طبّق الـ migration").

⚠️ الكود الجديد بيقرا وبيكتب `Status`، فالنشر قبل التطبيق = كل `POST /sessions/{id}/pay` بيرمي 500 (`Invalid column name 'Status'`). الترتيب لازم يكون: migration أولًا، الكود تاني.

## فخاخ

- **`startTime` = TimeSpan، مش ISO**: `"14:30"` أو `"14:30:00"`. `DateOnly date` = `"yyyy-MM-dd"`. الاتنين **بتوقيت المدرس** (من `Countries.TimezoneId`) مش UTC ومش وقت الطالب.
- لو المدرس عنده DST في اليوم ده والساعة مش موجودة (مثلاً 02:30 يوم القفز) → 400 باسم الساعة والتاريخ.
- الحجز **بيستنى دفع** مش المدرس: أي UI كان بيعرض "بانتظار موافقة المدرس" لازم يتغير لـ "بانتظار الدفع"، وأي polling على حالة `Pending` لازم يفترض إنها هتموت عند وقت البداية.
- الحجز غير المدفوع **بيعدّي المقعد لحد ما الـ job يشتغل** (10 دقايق)، فالـ UI لازم ما يعتمدش على إن `status` بقت `Cancelled` عشان يفتح الخانة — `bookable-slots` بترجعها متاحة قبلها.
- الدفع بعد `startTime` مستحيل، والحصةTrial بتخطي الدفع — زر "ادفع" يتخفي للتريال.
- مفيش **gateway حقيقي**: الدفع = خصم ساعات من الباقة + تغيير حالة. أي شاشة بتقول "تم تحويل المبلغ" لازم تتظبط على إن المبلغ ده بيدخل `PendingBalance` ويحرر بعد الحصة.
- لو LiveKit مش متظبط في البيئة: الحجز والدفع بينجحوا، و`/start` و`/join` هيفشلوا برسالة الإعداد — نفس فخ الـ README بتاع LiveKit.

## متروك عمدًا / لسه موجود

- **سباق الحجز المزدوج**: مكانش فيه unique index على `(InstructorId, StartsAtUtc)` قبل كده ولسه مفيش — نظريًا طالبين على نفس الوقت (احتمال قل لأنه بقى فيه نافذة تحقق + سباق 10 دقايق أقل حساسية). قرار إضافته ليك.
- **تسريب الاسترداد**: `RefundHours` بيرد على **آخر باقة** (`OrderByDescending(CreatedAt)`) ولو لاقيتش بيسيبها null — السلوك القديم لسه زي ما هو.
- **تغيير الميعاد (reschedule) لسه محتاج موافقة المدرس**: `/reschedule-requests/{id}/approve|decline` والـ `WindowIsInsideAvailabilityAsync` اللي بتاعتها — دي قصة تانية مختلفة عن "تأكيد الحجز" اللي اتلغى، وما اتلمستش.
- الحصص الجماعية والكورسات (lesson-attached sessions) لسه مش بتمرّوا على مسار الدفع ده — شروط `session.Lesson == null` في حارس القاعة بتسيبيها تعدي.
