# دليل الـ Frontend — ملف المدرّب الشامل (`/instructor-profile`)

> 📌 **حالة الوثيقة:** متكتوبة من الكود الموجود في البرنج (2026-11-05). **لسه مش منشور** على `lms-api.runasp.net` — Live لسه بيسلك السلوك القديم. أي اختبار لازم يكون على نسخة محلية أو بعد النشر.

---

## 1. تقسيم الـ endpoints — اقرأ الجزء ده الأول

| الـ endpoint | لمين | الحالة |
|---|---|---|
| `GET /api/v1/instructor-profile` | **المدرّب** ( شاشة "Me" / Profile ) | شاملة: بيانات المدرّب + الهوية الأساسية جوه `profile` |
| `PUT /api/v1/instructor-profile` | **المدرّب** | شاملة برضه: تعدّل المدرّب والأساسي في نفس الـ call |
| `GET /api/v1/authentication/profile` | أدمن / أي مستخدم تاني | سابتها مفتوحة من غير أي قيد دور. شكل `data` بتاعها = نفس شكل `profile` اللي جوه `/instructor-profile` بالظبط |
| `PUT /api/v1/authentication/update-user-information` | أدمن / أي مستخدم تاني | زيها: مفتوحة، وبتسلك نفس الـ fields الأربعة |
| ملف المتعلّم (student) | — | **لسه اتعمل**. هيتعمل في مرحلة لاحقة |

**القرار:** تطبيق المدرّب **ميفيش** نفسه ينادي `authentication/profile` و`instructor-profile` ورا بعض. `instructor-profile` بترجع الاتنين في رد واحد. الـ `authentication/*` بتاعت الملف العام لباقي الأدوار.

---

## 2. `GET /api/v1/instructor-profile`

- **الحماية:** `Authorize` + `RoleAuthorize(Instructor)`.
- **باراميترات:** ولا واحد. الهوية بتتاخد من التوكن.
- **الرسالة في النجاح:** `Instructor profile retrieved successfully.`

### شكل الرد

```json
{
  "success": true,
  "isSuccess": true,
  "message": "Instructor profile retrieved successfully.",
  "data": {
    "instructorId": "3c1f2a9e-7b44-4d1f-9a2c-51ef0a6d7b11",
    "userId": "b67e7114-2f8c-4a55-9d0e-7a1f3c5d8e90",
    "profile": {
      "userId": "b67e7114-2f8c-4a55-9d0e-7a1f3c5d8e90",
      "email": "sara@example.com",
      "fullName": "Sara Mahmoud",
      "profilePictureUrl": "https://.../uploads/users/sara.jpg",
      "countryId": "12f9c0a1-...",
      "countryName": "Egypt",
      "timezoneId": "Africa/Cairo",
      "language": 1,
      "roles": ["Instructor"],
      "instructorId": "3c1f2a9e-7b44-4d1f-9a2c-51ef0a6d7b11",
      "createdAt": "2026-09-14T10:22:41Z"
    },
    "vacationMode": false,
    "specialization": "IELTS preparation",
    "bio": "Eight years preparing students for academic IELTS...",
    "yearsOfExperience": 8,
    "education": "BA English Literature, Cairo University",
    "linkedInUrl": "https://www.linkedin.com/in/sara-mahmoud",
    "hourlyRate": 25.0,
    "applicationStatus": 1,
    "isActive": true,
    "certificates": [
      {
        "id": "8ad1...",
        "filePath": "https://.../uploads/certificates/8ad1.pdf",
        "originalFileName": "degree.pdf",
        "type": 0
      }
    ],
    "createdAt": "2026-09-15T08:03:12Z",
    "updatedAt": "2026-11-01T17:44:05Z"
  },
  "errors": {},
  "statusCode": 200
}
```

### جدول الحقول

| الحقل | نوعه | من وين | ملاحظة |
|---|---|---|---|
| `instructorId` | guid | `Instructors.Id` | نفس قيمة `profile.instructorId` |
| `userId` | guid | `Instructors.UserId` | نفس قيمة `profile.userId` |
| `profile` | object | جدول الـ Users + الدول + الـ roles | **نفس شكل `GET /authentication/profile` حرفيًا** — مفيش أي field متغيّر أو متزود |
| `vacationMode` | bool | `Instructors.VacationMode` | **للقراءة بس**. التعديل من `PUT /api/v1/my/vacation-mode` |
| `specialization` | string | `Instructors.Specialization` | |
| `bio` | string | `Instructors.Bio` | |
| `yearsOfExperience` | int | `Instructors.YearsOfExperience` | |
| `education` | string | `Instructors.Education` | |
| `linkedInUrl` | string \| null | `Instructors.LinkedInUrl` | `null` لو المدرّب مسّحه |
| `hourlyRate` | decimal | `Instructors.HourlyRate` | **السعر الحي** للـ 1-to-1. مش سعر التقديم (ده في الـ application) |
| `applicationStatus` | int enum | `InstructorApplication.Status` | 0 Pending · 1 Approved · 2 Rejected |
| `isActive` | bool | `Users.IsActive` | حالة **الحساب** ( الأدمن بيقفّله ) — مش لها علاقة بالـ approval |
| `certificates[]` | array | `InstructorCertificates` | كل ملفات التقديم: شهادة، خبرة، وفيديو تعريفي |
| `createdAt` / `updatedAt` | DateTime UTC | `Instructors` | تاريخ **سجل المدرّب** — مش تاريخ إنشاء الحساب (ده في `profile.createdAt`) |

---

## 3. `PUT /api/v1/instructor-profile`

- **الحماية:** زي الـ GET.
- **الرسالة في النجاح:** `Instructor profile updated successfully.`
- **الرد:** **نفس شكل الـ GET بالظبط** (شامل).

### الـ body

```json
{
  "fullName": "Sara M. Mahmoud",
  "countryId": "12f9c0a1-...",
  "language": 1,
  "profilePictureUrl": "https://.../uploads/users/new.jpg",

  "specialization": "IELTS preparation",
  "bio": "Eight years preparing students for academic IELTS and business English.",
  "yearsOfExperience": 8,
  "education": "BA English Literature, Cairo University",
  "linkedInUrl": "https://www.linkedin.com/in/sara-mahmoud",
  "hourlyRate": 27.5
}
```

| الـ field | مطلوب؟ | لو مبعتوش | ملاحظات |
|---|---|---|---|
| `fullName` | اختياري | **بيسيبه زي ما هو** | ابعت نص فيه حرف واحد على الأقل. لو بعت `""` أو مسافات → 400 (`Full name is required.`) |
| `countryId` | اختياري | **بيسيبه زي ما هو** | لازم country موجودة → وإلا 400 (`The selected country was not found.`). **مفيش طريقة تمسح بيها الدولة** |
| `language` | اختياري | **بيسيبه زي ما هو** | `1` = En · `2` = Ar. أي رقم تاني → 400 (`The selected language is invalid.`) |
| `profilePictureUrl` | اختياري | **بيسيبه زي ما هو** | `""` أو مسافات = **يمسح الصورة**. أطول من 500 → 400 |
| `specialization` | **مطلوب** | — | مش فارغ، ≤ 150 |
| `bio` | **مطلوب** | — | من 30 لـ 2000 حرف |
| `yearsOfExperience` | **مطلوب** | — | عدد من 0 لـ 60 |
| `education` | **مطلوب** | — | مش فارغ، ≤ 500 |
| `hourlyRate` | **مطلوب** | — | أكبر من 0، ≤ 10000 |
| `linkedInUrl` | اختياري | `null` | URL كامل `http`/`https` بس؛ `""` = تمسحه؛ ≤ 500 |

> ⚠️ **الحقول الخمسة اللي تحت مطلوبين في كل `PUT`** — يعني الـ endpoint دي **مش** PATCH على مستوى بيانات المدرّب. الفرونت لازم يبعت الفورم كامل (وخد القيم الحالية من الـ GET). الجزء الأساسي (`fullName` / `countryId` / `language` / `profilePictureUrl`) هو الوحيد اللي اختياري بسلوك "omitted = keep".

> ⚠️ **الفرق بين `null` و`""`:** في `profilePictureUrl` وفي `linkedInUrl` الـ `""` = امسح، والـ `null`/عدم الإرسال = خلي. في `fullName` الـ `""` = خطأ 400. متوحدش المعاملة فيهم.

### شرط الاعتماد

التعديل مسموح **للمدرّب المعتمد بس** (`applicationStatus = 1`). لو لسه Pending أو مرفوض:

```json
{
  "success": false,
  "isSuccess": false,
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "data": null,
  "errors": {
    "General": ["Only an approved instructor can edit this profile. Resubmit your application instead."]
  }
}
```

**مش 403** — 400 مع مفتاح `General`. والمرفوض يرجع لـ Approved من `POST /api/v1/instructor-applications/resubmit`، ومش من الـ PUT ده.

الـ **GET** ما فيهوش الشرط ده: Pending و Rejected كمان بيقروا البروفايل بتاعهم عادي (عشان شاشة "حالة طلبي").

---

## 4. خرائط الـ enums (أرقام، مش نصوص)

| الـ field | القيم |
|---|---|
| `applicationStatus` | `0` = Pending · `1` = Approved · `2` = Rejected |
| `profile.language` | `1` = En · `2` = Ar (مفيش `0`) |
| `certificates[].type` | `0` = Certificate · `1` = ExperienceCertificate · `2` = IntroVideo |

> الفيديو التعريفي بييجي جوه `certificates[]` بنوع `2` — مفيش field مستقل اسمه `introVideoUrl`.

---

## 5. رسائل الخطأ الحرفية

كل الردود بنفس الـ envelope. الـ 400 بتيجي في `errors` بمفتاح اسم الـ field؛ الـ 401/403/404 الرسالة في `message` بس والـ `errors` فاضية.

| الحالة | السبب | النص |
|---|---|---|
| `401` | توكن مفقود/مرفوض أو `userId` فاضي | رسالة الـ unauthorized العامة |
| `403` | التوكن لصاحب دور تاني (مش Instructor) | رسالة الـ forbidden العامة |
| `404` | مفيش سجل `Instructors` للمستخدم ده (أو متحذف) | `No active account was found with this email address.` |
| `400` | فشل فاليديشن | الرسائل اللي في الجداول فوق، + `Specialization is required.` / `Specialization cannot exceed {0} characters.` / `Bio is required.` / `Bio must be at least {0} characters long.` / `Bio cannot exceed {0} characters.` / `Years of experience is required.` / `Years of experience must be between {0} and {1}.` / `Education is required.` / `Education cannot exceed {0} characters.` / `An hourly rate greater than zero is required.` / `Hourly rate cannot exceed {0}.` / `LinkedIn URL cannot exceed {0} characters.` / `The LinkedIn URL is invalid.` |

**متطابقش على نص الرسالة** لو عندك behavior معلّق عليها — استخدم الـ `statusCode` + مفتاح الـ field. الرسائل كلها إنجليزي وبتتعرّب على السيرفر حسب `Accept-Language`.

---

## 6. مقارنة — `authentication/profile` و`/instructor-profile`

| | `GET /authentication/profile` | `GET /instructor-profile` |
|---|---|---|
| `data` | كائن الهوية الأساسي | كائن المدرّب + نفس كائن الهوية جوه `profile` |
| `profile.timezoneId` | ✅ | ✅ |
| `profile.instructorId` | ✅ | ✅ |
| حقول المدرّب | ❌ | ✅ |
| `certificates[]` | ❌ | ✅ |
| `vacationMode` | ❌ | ✅ |
| `hourlyRate` | ❌ | ✅ |

الاتنين بياخدوا كائن الهوية من **نفس الدالة** على السيرفر (`UserProfileMapper`)، فمستحيل يحصل انحراف بينهم.

### اللي اتصلّح في `authentication/*` في نفس التعديل ده

`PUT /api/v1/authentication/update-user-information` كان بيرجع `timezoneId` و`instructorId` **`null`** على طول (بين الـ GET والـ PUT). بقى الآن بيرجع نفس شكل الـ GET. أي كود في الفرونت كان بيعيد يخزن الرد بعد التعديل — شوفه، دلوقتي الحقول دي فيها قيمة.

---

## 7. تنبيهات وفخاخ

| # | الحاجة | الأثر على الفرونت |
|---|---|---|
| 1 | `instructor-profile` شاملة | شاشة "Me" بتتعبّى من **call واحد**. متطلبش `authentication/profile` ورا ده للمدرّبين |
| 2 | `profile.createdAt` ≠ `createdAt` | الأول = إنشاء الحساب، التاني = إنشاء سجل المدرّب. متعرضش الاتنين كـ ".join date" |
| 3 | التعديل مش PATCH | الحقول الخمسة الأساسية للمدرّب مطلوبة دايمًا — عبّي الفورم من الـ GET قبل الإرسال |
| 4 | `isActive` حالة الحساب | `false` مع `applicationStatus = 1` = معتمد بس الحساب مقفول من الأدمن. متعرضش "مرفوض" |
| 5 | `vacationMode` للقراءة | التعديل من `PUT /my/vacation-mode` بس. متبعتوش في body الـ PUT ده (مفيش ليّه معنى هنا) |
| 6 | مفيش كورسات ولا مواعيد في الرد | متطلبش `/instructor-profile` وتقول "ده كل حاجة عن المدرّب". الكورسات من `GET /my/...` بتوع الكورسات، والجلسات من `/sessions` |
| 7 | `hourlyRate` الحي ≠ سعر التقديم | اللي الأدمن بيشوفه وقت المراجعة هو snapshot في الـ application. العرض للمتعلمين بياخد الحي |
| 8 | تغيير العقد additive | `instructorId`, `userId`, `profile`, `vacationMode` **متزودين**. أي كود قديم بيقرا `specialization`/`bio`/... شغال من غير تعديل |
| 9 | الـ endpoint مفتوحة لأي مدرّب معتمد | مفيش قيد إن المدرّب "المعيّن" على كورس معين — الملف واحد لكل المدرّبين |
| 10 | `authentication/profile` سابتها مفتوحة | الأدمن لسه يقدر يناديها. متفرضش إن المدرّب ممنوع منها — هو يقدر، بس مش محتاجها |

---

## 8. أمثلة curl

```bash
TOKEN='<jwt بتاع مدرّب>'
BASE='https://lms-api.runasp.net/api/v1'
H=(-H "Authorization: Bearer $TOKEN" -H 'Accept: application/json' -H 'Accept-Language: En')

# 1) اقرأ البروفايل الشامل
curl "$BASE/instructor-profile" "${H[@]}"

# 2) عدّل كل حاجة — بيانات مدرّب + اسم وبلد وصورة
curl -X PUT "$BASE/instructor-profile" "${H[@]}" -H 'Content-Type: application/json' -d '{
  "fullName": "Sara M. Mahmoud",
  "countryId": "12f9c0a1-0000-0000-0000-000000000000",
  "language": 1,
  "profilePictureUrl": "https://example.com/new.jpg",
  "specialization": "IELTS preparation",
  "bio": "Eight years preparing students for academic IELTS and business English.",
  "yearsOfExperience": 8,
  "education": "BA English Literature, Cairo University",
  "linkedInUrl": "https://www.linkedin.com/in/sara-mahmoud",
  "hourlyRate": 27.5
}'

# 3) عدّل بيانات المدرّب بس — سيب الحقول الاختيارية ملغية خالص
curl -X PUT "$BASE/instructor-profile" "${H[@]}" -H 'Content-Type: application/json' -d '{
  "specialization": "Business English",
  "bio": "Corporate English trainer for technical teams across the Gulf region.",
  "yearsOfExperience": 6,
  "education": "MA Applied Linguistics",
  "hourlyRate": 30
}'

# 4) امسح الصورة الشخصية (بعت "" مش null)
#    كرر الـ PUT رقم 3 وزوّد: "profilePictureUrl": ""

# 5) قارن الرد: ملف الهوية العام (للأدمن)
curl "$BASE/authentication/profile" "${H[@]}"
```

---

## 9. حالة النشر

- الكود ده **لسه مش منشور** — Live لسه بيرجع الشكل القديم (من غير `profile`) والـ PUT القديم.
- بعد النشر: أي `PUT` قديم بنفس الـ body بتاع أول (من غير `fullName`/`countryId`/`language`/`profilePictureUrl`) شغال من غير أي تعديل — الحقول دي اختيارية، وسيبها = مفيش تغيير في الحساب.
- للمتابعة: التعديل اتعمل في `LMS.Application/Features/InstructorProfile/*` و`LMS.Application/Features/Authentication/UserProfileMapper.cs`. **مفيش migration جديدة** في الشغل ده — مفيش أي عمود اتزود؛ كل الحقول موجودة أصلًا في `Instructors` و`Users`.
