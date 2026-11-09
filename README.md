# 🎓 LMS API Documentation - الدليل الشامل لواجهات برمجة التطبيقات لمنصة LMS

> **مرحباً بك في التوثيق التقني الكامل لنظام إدارة التعلم (LMS API)**.
> تم إنشاء هذا الملف استناداً إلى مواصفات OpenAPI 3.0 ومطابقته مباشرة مع بنية المتحكمات (Controllers) ونماذج CQRS (Commands & Queries) داخل المشروع.
> يوضح الدليل لكل مسار: **المسار (Route)**، **طريقة الطلب (HTTP Method)**، **الصلاحيات المطلوبة (Authorization)**، **بيانات الطلب (Request Parameters & Body)**، و**هيكل الاستجابة (Response Models)** مع أمثلة عملية.

---

## 📌 المحتويات الرئيسية
1. [المواصفات العامة للنظام (General Specifications)](#-المواصفات-العامة-للنظام-general-specifications)
2. [الهيكل الموحد للاستجابة (Standard ApiResponse Structure)](#-الهيكل-الموحد-للاستجابة-standard-apiresponse-structure)
3. [دليل الثوابت والتعدادات (Enums Reference)](#-دليل-الثوابت-والتعدادات-enums-reference)
4. [فهرس الوحدات البرمجية (Modules Index)](#-فهرس-الوحدات-البرمجية-modules-index)
5. [شرح الـ Endpoints بالتفصيل لكل موديول](#-شرح-الـ-endpoints-بالتفصيل)

---

## ⚙️ المواصفات العامة للنظام (General Specifications)

- **إصدار الواجهة (API Version)**: `v1.0` (OpenAPI 3.0.1)
- **مسار الأساس (Base Prefix)**: `/api/v1`
- **المصادقة وتأمين المسارات**:
  - تعتمد الواجهة على رمز JWT عبر الهيدر الموحد: `Authorization: Bearer <ACCESS_TOKEN>`
  - الـ Endpoints التي تحمل وسم 🔒 تتطلب توكن مسجل، والمسارات التي تحمل 🔓 عامة ومتاحة للجميع.
- **تحديد اللغة (Localization)**:
  - يدعم النظام اللغتين العربية والإنجليزية عبر الهيدر: `Accept-Language: En` أو `Accept-Language: Ar`
- **تنسيق التاريخ والوقت**: بصيغة ISO 8601 القياسية العالمية، مثل: `2026-11-09T10:00:00Z`
- **نظام الصفحات (Pagination)**: في مسارات الـ Paginated يتم استخدام:
  - `pageNumber`: رقم الصفحة (افتراضياً `1`)
  - `pageSize`: عدد العناصر بالصفحة (افتراضياً `10`)
  - `searchQuery` أو `search`: للبحث النصي الحر

---

## 📦 الهيكل الموحد للاستجابة (Standard ApiResponse Structure)

تلتزم كافة مسارات الـ API بإرجاع البيانات مغلفة بكائن استجابة قياسي موحد `ApiResponse<T>`:

```json
{
  "success": true,
  "isSuccess": true,
  "statusCode": 200,
  "message": "Operation completed successfully / تم إنجاز العملية بنجاح",
  "data": {
    "/* كائن البيانات الفعلي المطلوب T (Object or Array) */": "..."
  },
  "errors": {
    "FieldName": [
      "Validation error message if any"
    ]
  }
}
```

| الحقل | النوع | الوصف |
| :--- | :--- | :--- |
| `success` / `isSuccess` | `boolean` | يشير إلى نجاح العملية أو فشلها |
| `statusCode` | `integer` | رمز حالة الـ HTTP المرجع (مثل 200, 201, 400, 404, 500) |
| `message` | `string` | رسالة نصية توضيحية للمستخدم أو رسالة الخطأ |
| `data` | `T` | كائن البيانات الفعلي العائد من العملية (يحتوي النتائج) |
| `errors` | `object` | قاموس يحتوي على تفاصيل الأخطاء الحقلية عند فشل التحقق (Validation) |

---

## 📑 دليل الثوابت والتعدادات (Enums Reference)

تستخدم المنظومة أرقاماً صحيحة (integers) لتمثيل الحالات والأنواع. يوضح الجدول التالي مسميات هذه القيم:

| اسم الـ Enum | الاستخدام | القيم الرقمية ومعانيها |
| :--- | :--- | :--- |
| **`AttendanceStatus`** | AttendanceStatus | **1** = `Present (حاضر)`<br>**2** = `Absent (غائب)`<br>**3** = `Late (متأخر)`<br>**4** = `Excused (معذور)` |
| **`BookingStatus`** | BookingStatus | **0** = `Pending (قيد الانتظار)`<br>**1** = `Confirmed (مؤكد)`<br>**2** = `Cancelled (ملغي)`<br>**3** = `Declined (مرفوض)` |
| **`CourseLevel`** | CourseLevel | **0** = `Beginner (مبتدئ)`<br>**1** = `Elementary (أساسي)`<br>**2** = `Intermediate (متوسط)`<br>**3** = `Advanced (متقدم)` |
| **`CourseType`** | CourseType | **1** = `Live (جلسات مباشرة)`<br>**2** = `Recorded (مسجل)` |
| **`DayOfWeek`** | DayOfWeek | **0** = `Sunday (الأحد)`<br>**1** = `Monday (الإثنين)`<br>**2** = `Tuesday (الثلاثاء)`<br>**3** = `Wednesday (الأربعاء)`<br>**4** = `Thursday (الخميس)`<br>**5** = `Friday (الجمعة)`<br>**6** = `Saturday (السبت)` |
| **`InstructorApplicationStatus`** | InstructorApplicationStatus | **0** = `Pending (قيد المراجعة)`<br>**1** = `Approved (مقبول)`<br>**2** = `Rejected (مرفوض)` |
| **`InstructorCertificateType`** | InstructorCertificateType | **0** = `Certificate (شهادة أكاديمية)`<br>**1** = `ExperienceCertificate (شهادة خبرة)`<br>**2** = `IntroVideo (فيديو تعريفي)` |
| **`Language`** | Language | **1** = `En (الإنجليزية)`<br>**2** = `Ar (العربية)` |
| **`LiveRoomStatus`** | LiveRoomStatus | **0** = `NotStarted (لم تبدأ)`<br>**1** = `Live (جارية الآن)`<br>**2** = `Ended (انتهت)`<br>**3** = `Abandoned (متروكة)` |
| **`MediaKind`** | MediaKind | **0** = `Image (صورة)`<br>**1** = `Video (فيديو)`<br>**2** = `Audio (ملف صوتي)`<br>**3** = `Document (مستند)` |
| **`OnboardingQuestionType`** | OnboardingQuestionType | **0** = `SingleSelect (اختيار واحد)`<br>**1** = `MultiSelect (اختيار متعدد)`<br>**2** = `Slider (شريط تمرير)` |
| **`ParticipantRole`** | ParticipantRole | **1** = `Host (المقدم / المعلم)`<br>**2** = `Attendee (الحاضر / الطالب)` |
| **`RescheduleRequestStatus`** | RescheduleRequestStatus | **0** = `Pending (قيد الانتظار)`<br>**1** = `Approved (موافق عليه)`<br>**2** = `Declined (مرفوض)`<br>**3** = `Withdrawn (مسحوب)` |
| **`SessionStatus`** | SessionStatus | **0** = `Pending (قيد الانتظار)`<br>**1** = `Confirmed (مؤكد)`<br>**2** = `Completed (مكتمل)`<br>**3** = `Cancelled (ملغي)`<br>**4** = `NoShow (لم يحضر)` |
| **`SessionTab`** | SessionTab | **0** = `Upcoming (القادمة)`<br>**1** = `Completed (المكتملة)`<br>**2** = `Cancelled (الملغية)` |
| **`SessionType`** | SessionType | **0** = `OneToOne (فردية 1-1)`<br>**1** = `Group (جماعية)` |
| **`UserType`** | UserType | **0** = `Learner (طالب)`<br>**1** = `Instructor (معلم / مدرب)`<br>**2** = `Admin (مسؤول النظام)` |

---

## 🧭 فهرس الوحدات البرمجية (Modules Index)

| # | اسم الموديول (Module) | العنوان العربي | عدد المسارات | ملخص الوظيفة |
| :-: | :--- | :--- | :-: | :--- |
| 1 | [**Attachments**](#attachments) | المرفقات والوسائط | `4` | واجهات رفع واستبدال وتنزيل الملفات (الصور، الفيديوهات، المقاطع الصوتية، والمستندات). |
| 2 | [**Authentication**](#authentication) | المصادقة والحسابات | `12` | تسجيل الدخول، إنشاء الحساب، التحقق عبر رمز OTP، استعادة كلمة المرور، تحديث البيانات والملف الشخصي. |
| 3 | [**Countries**](#countries) | الدول والعملات | `6` | إدارة الدول والرموز الهاتفية ومفاتيح الاتصال وقوائم الدول المتاحة. |
| 4 | [**Courses**](#courses) | الكورسات (للمتعلمين) | `7` | تصفح الكورسات المتاحة، الدروس، التسجيل في كورس، متابعة التقدم وإجراء الاختبارات. |
| 5 | [**CoursesAdmin**](#coursesadmin) | إدارة الكورسات (المسؤول) | `14` | لوحة تحكم المسؤول لإضافة وتعديل وحذف الكورسات، الدروس، الجلسات المباشرة، بنك الأسئلة وتعيين المدربين. |
| 6 | [**HelpCenter**](#helpcenter) | مركز المساعدة | `3` | تصفح مقالات المساعدة والأسئلة الشائعة وتصنيفاتها للمستخدمين. |
| 7 | [**HelpCenterAdmin**](#helpcenteradmin) | إدارة مركز المساعدة (المسؤول) | `8` | إضافة وتعديل وحذف تصنيفات ومقالات مركز المساعدة وقاعدة المعرفة. |
| 8 | [**InstructorApplications**](#instructorapplications) | طلبات انضمام المدربين (المسؤول) | `4` | استعراض ومراجعة طلبات الانضمام المقدمة من المعلمين واعتمادها أو رفضها مع كتابة الأسباب. |
| 9 | [**InstructorAvailabilityAdmin**](#instructoravailabilityadmin) | إدارة أوقات المدربين (المسؤول) | `4` | تحكم المسؤول في جدول وتوفر مواعيد المدربين وساعات العمل الأسبوعية. |
| 10 | [**InstructorProfile**](#instructorprofile) | الملف التعريفي للمدرب | `2` | عرض وتحديث بيانات المدرب، التخصصات، السيرة الذاتية، اللغات، والبيانات البنكية. |
| 11 | [**LiveClasses**](#liveclasses) | الفصول الافتراضية المباشرة (LiveKit) | `7` | تكامل الفصول الحية 1:1: توليد توكن الدخول، بدء الجلسة، الانضمام، المغادرة، والإنهاء. |
| 12 | [**LMS.API**](#lms-api) | فحص النظام (Health Check) | `1` | فحص الاتصال بجذور الـ API والتأكد من سلامة الخادم. |
| 13 | [**MyAvailability**](#myavailability) | جدول توفر المعلم (My Availability) | `8` | واجهات خاصة بالمعلم لإدارة فتراته الزمنية الأسبوعية، التقويم، ونمط الإجازة (Vacation Mode). |
| 14 | [**MyInstructorApplication**](#myinstructorapplication) | متابعة طلب الانضمام (للمعلم) | `2` | متابعة حالة طلب التدريس وإعادة التقديم وتعديل المرفقات في حال الرفض. |
| 15 | [**Onboarding**](#onboarding) | التهيئة وتحديد المستوى (Onboarding) | `4` | أسئلة تحديد المستوى والاهتمامات للمتعلمين الجدد، حفظ الإجابات، واقتراح المدربين الأنسب. |
| 16 | [**OnboardingAdmin**](#onboardingadmin) | إدارة أسئلة التهيئة (المسؤول) | `7` | إنشاء وتحديث وحذف أسئلة استبيان التهيئة والخيارات المتعددة التابعة لها. |
| 17 | [**RescheduleRequests**](#reschedulerequests) | طلبات إعادة الجدولة | `2` | الرد على طلبات إعادة جدولة الجلسات من قبل الطرف المعني (قبول أو رفض). |
| 18 | [**Sessions**](#sessions) | الجلسات والحجوزات (Sessions) | `9` | استعراض وحجز الجلسات الفردية أو الجماعية، تأكيد أو إلغاء الحجز، وطلب إعادة الجدولة. |
| 19 | [**TeacherAvailability**](#teacheravailability) | إتاحة المعلم السريعة | `1` | تفعيل أو تعطيل وضع الإجازة السريع للمعلم. |
| 20 | [**TeacherRescheduleRequests**](#teacherreschedulerequests) | طلبات إعادة الجدولة للمعلم | `2` | عرض كافة طلبات إعادة الجدولة الخاصة بحصص المعلم مع إمكانية سحب الطلب. |
| 21 | [**TeacherSessions**](#teachersessions) | جلسات المعلم | `3` | إدارة المعلم لجلساته التعليمية، الاطلاع على قائمة الطلاب المشاركين، وإلغاء أو جدولة الجلسة. |
| 22 | [**Users**](#users) | إدارة المستخدمين (المسؤول) | `6` | استعراض المستخدمين، البحث والفلترة المتقدمة، إنشاء حساب مستخدم، وتعديل بياناته أو كلمة مروره. |

---

## 🚀 شرح الـ Endpoints بالتفصيل

### <a id="attachments"></a>📁 Attachments (المرفقات والوسائط)
> 💡 **نطاق العمل**: واجهات رفع واستبدال وتنزيل الملفات (الصور، الفيديوهات، المقاطع الصوتية، والمستندات).

#### 🔹 1. `POST` `/api/v1/attachments/upload`

- **الوصف الوظيفي**: رفع ملف مفرد جديد (صورة، فيديو، صوت، مستند) إلى السيرفر وتخزينه.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `File` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `MediaType` | `MediaKind`<br>*(Enum: `0`: Image (صورة) | `1`: Video (فيديو) | `2`: Audio (ملف صوتي) | `3`: Document (مستند))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `MediaKind` |
| `Place` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "File": "<ملف ثنائي Binary File>",
  "MediaType": 0,
  "Place": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/attachments/upload-multiple`

- **الوصف الوظيفي**: رفع عدة ملفات دفعة واحدة وإرجاع روابط وأسماء الملفات المرفوعة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `Files` | Array<string (`binary`)> | ⚪ اختياري (Optional) | - |
| `MediaType` | `MediaKind`<br>*(Enum: `0`: Image (صورة) | `1`: Video (فيديو) | `2`: Audio (ملف صوتي) | `3`: Document (مستند))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `MediaKind` |
| `Place` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "Files": [
    "<ملف ثنائي Binary File>"
  ],
  "MediaType": 0,
  "Place": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringListApiResponse`](#stringlistapiresponse) |
| `400` | Bad Request | [`StringListApiResponse`](#stringlistapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    "نص توضيحي"
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `POST` `/api/v1/attachments/update`

- **الوصف الوظيفي**: استبدال ملف مرفوع سابقاً بملف جديد مع حذف أو تحديث القديم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `OldFileName` | string | ⚪ اختياري (Optional) | - |
| `File` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `MediaType` | `MediaKind`<br>*(Enum: `0`: Image (صورة) | `1`: Video (فيديو) | `2`: Audio (ملف صوتي) | `3`: Document (مستند))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `MediaKind` |
| `Place` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "OldFileName": "نص توضيحي",
  "File": "<ملف ثنائي Binary File>",
  "MediaType": 0,
  "Place": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `GET` `/api/v1/attachments/download`

- **الوصف الوظيفي**: تنزيل أو قراءة ملف مرفق مخزن من خلال اسمه ومكانه ونوعه.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `FileName` | string | لا | - |
| `FilePlace` | integer (`int32`) | لا | - |
| `MediaType` | `MediaKind`<br>*(Enum: `0`: Image (صورة) | `1`: Video (فيديو) | `2`: Audio (ملف صوتي) | `3`: Document (مستند))* | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | string (`binary`) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | - |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
"<ملف ثنائي Binary File>"
```

---

### <a id="authentication"></a>📁 Authentication (المصادقة والحسابات)
> 💡 **نطاق العمل**: تسجيل الدخول، إنشاء الحساب، التحقق عبر رمز OTP، استعادة كلمة المرور، تحديث البيانات والملف الشخصي.

#### 🔹 1. `POST` `/api/v1/authentication/login`

- **الوصف الوظيفي**: تسجيل الدخول باستخدام البريد الإلكتروني وكلمة المرور وتوليد JWT Token.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `password` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "email": "نص توضيحي",
  "password": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`ProblemDetails`](#problemdetails) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/authentication/register`

- **الوصف الوظيفي**: تسجيل حساب جديد كطالب (Learner) أو تقديم طلب انضمام كمعلم (Instructor) مع المرفقات.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `FullName` | string | 🔴 **إلزامي (Required)** | - |
| `Email` | string | 🔴 **إلزامي (Required)** | - |
| `Password` | string | 🔴 **إلزامي (Required)** | - |
| `ConfirmPassword` | string | 🔴 **إلزامي (Required)** | - |
| `PhoneNumber` | string | 🔴 **إلزامي (Required)** | - |
| `CountryId` | string (`uuid`) | ⚪ اختياري (Optional) | - |
| `Role` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `Specialization` | string | ⚪ اختياري (Optional) | - |
| `Bio` | string | ⚪ اختياري (Optional) | - |
| `YearsOfExperience` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `HourlyRate` | number (`double`) | ⚪ اختياري (Optional) | - |
| `Education` | string | ⚪ اختياري (Optional) | - |
| `LinkedInUrl` | string | ⚪ اختياري (Optional) | - |
| `Certificates` | Array<string (`binary`)> | 🔴 **إلزامي (Required)** | - |
| `IntroVideo` | string (`binary`) | ⚪ اختياري (Optional) | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "FullName": "نص توضيحي",
  "Email": "نص توضيحي",
  "Password": "نص توضيحي",
  "ConfirmPassword": "نص توضيحي",
  "PhoneNumber": "نص توضيحي",
  "CountryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Role": 0,
  "Specialization": "نص توضيحي",
  "Bio": "نص توضيحي",
  "YearsOfExperience": 1,
  "HourlyRate": 10.5,
  "Education": "نص توضيحي",
  "LinkedInUrl": "نص توضيحي",
  "Certificates": [
    "<ملف ثنائي Binary File>"
  ],
  "IntroVideo": "<ملف ثنائي Binary File>"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`RegisterResponseDtoApiResponse`](#registerresponsedtoapiresponse) |
| `202` | Accepted | [`RegisterResponseDtoApiResponse`](#registerresponsedtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `POST` `/api/v1/authentication/forgot-password`

- **الوصف الوظيفي**: إرسال رمز تحقق (OTP) إلى البريد الإلكتروني عند نسيان كلمة المرور.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `email` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "email": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `POST` `/api/v1/authentication/resend-otp`

- **الوصف الوظيفي**: إعادة إرسال رمز التحقق (OTP) للمستخدم.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `email` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "email": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `429` | Too Many Requests | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `POST` `/api/v1/authentication/verify-otp`

- **الوصف الوظيفي**: التحقق من صحة وصلاحية رمز الـ OTP المدخل من قبل المستخدم.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `otp` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "email": "نص توضيحي",
  "otp": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `POST` `/api/v1/authentication/reset-password`

- **الوصف الوظيفي**: تعيين كلمة مرور جديدة للمستخدم بعد التحقق من الـ OTP.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `otp` | string | 🔴 **إلزامي (Required)** | - |
| `newPassword` | string | 🔴 **إلزامي (Required)** | - |
| `confirmNewPassword` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "email": "نص توضيحي",
  "otp": "نص توضيحي",
  "newPassword": "نص توضيحي",
  "confirmNewPassword": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 7. `POST` `/api/v1/authentication/refresh-token`

- **الوصف الوظيفي**: تجديد الـ Access Token المنتهي باستخدام Refresh Token صالح.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `refreshToken` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "refreshToken": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`AuthResponseDtoApiResponse`](#authresponsedtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `roles` | Array<string> | 🔴 **إلزامي (Required)** | - |
| `accessToken` | string | 🔴 **إلزامي (Required)** | - |
| `refreshToken` | string | 🔴 **إلزامي (Required)** | - |
| `refreshTokenExpiration` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "نص توضيحي",
    "fullName": "نص توضيحي",
    "roles": [
      "نص توضيحي"
    ],
    "accessToken": "نص توضيحي",
    "refreshToken": "نص توضيحي",
    "refreshTokenExpiration": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 8. `POST` `/api/v1/authentication/change-password`

- **الوصف الوظيفي**: تغيير كلمة المرور الحالية للمستخدم المسجل دخوله.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `currentPassword` | string | 🔴 **إلزامي (Required)** | - |
| `newPassword` | string | 🔴 **إلزامي (Required)** | - |
| `confirmNewPassword` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "currentPassword": "نص توضيحي",
  "newPassword": "نص توضيحي",
  "confirmNewPassword": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 9. `POST` `/api/v1/authentication/logout`

- **الوصف الوظيفي**: تسجيل الخروج وإبطال الـ Refresh Token الحالي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `refreshToken` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "refreshToken": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 10. `DELETE` `/api/v1/authentication/delete-account`

- **الوصف الوظيفي**: حذف الحساب نهائياً من النظام بعد التأكد من كلمة المرور.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `password` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "password": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 11. `PUT` `/api/v1/authentication/update-user-information`

- **الوصف الوظيفي**: تعديل البيانات الأساسية للمستخدم (الاسم، رقم الهاتف، الدولة، الصورة الشخصية).
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `language` | `Language`<br>*(Enum: `1`: En (الإنجليزية) | `2`: Ar (العربية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `Language` |
| `profilePictureUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "fullName": "نص توضيحي",
  "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "language": 1,
  "profilePictureUrl": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`UserProfileDtoApiResponse`](#userprofiledtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `profilePictureUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `language` | `Language`<br>*(Enum: `1`: En (الإنجليزية) | `2`: Ar (العربية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `Language` |
| `roles` | Array<string> | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "نص توضيحي",
    "fullName": "نص توضيحي",
    "profilePictureUrl": "نص توضيحي",
    "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryName": "نص توضيحي",
    "timezoneId": "نص توضيحي",
    "language": 1,
    "roles": [
      "نص توضيحي"
    ],
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 12. `GET` `/api/v1/authentication/profile`

- **الوصف الوظيفي**: جلب تفاصيل الملف الشخصي الكامل للمستخدم المسجل دخوله حالياً.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`UserProfileDtoApiResponse`](#userprofiledtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `profilePictureUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `language` | `Language`<br>*(Enum: `1`: En (الإنجليزية) | `2`: Ar (العربية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `Language` |
| `roles` | Array<string> | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "نص توضيحي",
    "fullName": "نص توضيحي",
    "profilePictureUrl": "نص توضيحي",
    "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryName": "نص توضيحي",
    "timezoneId": "نص توضيحي",
    "language": 1,
    "roles": [
      "نص توضيحي"
    ],
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="countries"></a>📁 Countries (الدول والعملات)
> 💡 **نطاق العمل**: إدارة الدول والرموز الهاتفية ومفاتيح الاتصال وقوائم الدول المتاحة.

#### 🔹 1. `GET` `/api/v1/countries`

- **الوصف الوظيفي**: استرجاع قائمة كافة الدول النشطة في النظام للاختيار منها.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CountryDtoIReadOnlyCollectionApiResponse`](#countrydtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "countryEnName": "نص توضيحي",
      "code": "نص توضيحي",
      "timezoneId": "نص توضيحي",
      "isActive": true
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/countries`

- **الوصف الوظيفي**: إضافة دولة جديدة للنظام (كود الدولة، الاسم بالعربي والإنجليزي، العملة ومفتاح الاتصال).
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `countryEnName` | string | 🔴 **إلزامي (Required)** | - |
| `code` | string | 🔴 **إلزامي (Required)** | - |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "countryEnName": "نص توضيحي",
  "code": "نص توضيحي",
  "timezoneId": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`CountryDtoApiResponse`](#countrydtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/countries/pagginated`

- **الوصف الوظيفي**: استرجاع قائمة الدول بنظام الصفحات (Pagination) والبحث.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CountryDtoPagginatedResultApiResponse`](#countrydtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`CountryDto`](#countrydto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "countryEnName": "نص توضيحي",
        "code": "نص توضيحي",
        "timezoneId": "نص توضيحي",
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `GET` `/api/v1/countries/{id}`

- **الوصف الوظيفي**: جلب تفاصيل دولة معينة عبر معرّفها الخاص (ID).
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CountryDtoApiResponse`](#countrydtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `countryEnName` | string | 🔴 **إلزامي (Required)** | - |
| `code` | string | 🔴 **إلزامي (Required)** | - |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryEnName": "نص توضيحي",
    "code": "نص توضيحي",
    "timezoneId": "نص توضيحي",
    "isActive": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `DELETE` `/api/v1/countries/{id}`

- **الوصف الوظيفي**: حذف دولة من النظام بواسطة معرّفها.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `PUT` `/api/v1/countries/update`

- **الوصف الوظيفي**: تحديث بيانات دولة موجودة مسبقاً في النظام.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `countryEnName` | string | 🔴 **إلزامي (Required)** | - |
| `code` | string | 🔴 **إلزامي (Required)** | - |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "countryEnName": "نص توضيحي",
  "code": "نص توضيحي",
  "timezoneId": "نص توضيحي",
  "isActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CountryDtoApiResponse`](#countrydtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `countryEnName` | string | 🔴 **إلزامي (Required)** | - |
| `code` | string | 🔴 **إلزامي (Required)** | - |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryEnName": "نص توضيحي",
    "code": "نص توضيحي",
    "timezoneId": "نص توضيحي",
    "isActive": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="courses"></a>📁 Courses (الكورسات (للمتعلمين))
> 💡 **نطاق العمل**: تصفح الكورسات المتاحة، الدروس، التسجيل في كورس، متابعة التقدم وإجراء الاختبارات.

#### 🔹 1. `GET` `/api/v1/courses/pagginated`

- **الوصف الوظيفي**: تصفح قائمة الكورسات بنظام الصفحات مع إمكانية الفلترة بالنوع والمستوى والبحث.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Search` | string | لا | - |
| `Level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | لا | - |
| `InstructorId` | string (`uuid`) | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CourseListItemDtoPagginatedResultApiResponse`](#courselistitemdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`CourseListItemDto`](#courselistitemdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "نص توضيحي",
        "description": "نص توضيحي",
        "coverUrl": "نص توضيحي",
        "level": 0,
        "type": 1,
        "order": 1,
        "lessonCount": 1,
        "instructors": [
          "..."
        ],
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `GET` `/api/v1/courses/{id}`

- **الوصف الوظيفي**: جلب التفاصيل الكاملة لكورس معين تشمل الدروس والمحاضرين والأسعار.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CourseDetailDtoApiResponse`](#coursedetaildtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `description` | string | 🔴 **إلزامي (Required)** | - |
| `coverUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseLevel` |
| `type` | `CourseType`<br>*(Enum: `1`: Live (جلسات مباشرة) | `2`: Recorded (مسجل))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseType` |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isEnrolled` | boolean | 🔴 **إلزامي (Required)** | - |
| `instructors` | Array<[`InstructorRefDto`](#instructorrefdto)> | 🔴 **إلزامي (Required)** | - |
| `lessons` | Array<[`LessonListItemDto`](#lessonlistitemdto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "نص توضيحي",
    "description": "نص توضيحي",
    "coverUrl": "نص توضيحي",
    "level": 0,
    "type": 1,
    "order": 1,
    "isEnrolled": true,
    "instructors": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "specialization": "نص توضيحي"
      }
    ],
    "lessons": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "نص توضيحي",
        "order": 1,
        "isFreeDemo": true,
        "hasStudyBook": true,
        "hasVideo": true,
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionStartsAtUtc": "2026-11-09T10:00:00Z",
        "sessionEndsAtUtc": "2026-11-09T10:00:00Z",
        "liveRoomStatus": 0,
        "canJoinLiveRoom": true
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/courses/lessons/{id}/content`

- **الوصف الوظيفي**: جلب المحتوى التعليمي الكامل للدرس (فيديو، نصوص، مرفقات).
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LessonContentDtoApiResponse`](#lessoncontentdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `courseId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `lessonId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `courseTitle` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isFreeDemo` | boolean | 🔴 **إلزامي (Required)** | - |
| `videoUrl` | string | 🔴 **إلزامي (Required)** | - |
| `studyBookUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionStartsAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionEndsAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `liveRoomStatus` | `LiveRoomStatus`<br>*(Enum: `0`: NotStarted (لم تبدأ) | `1`: Live (جارية الآن) | `2`: Ended (انتهت) | `3`: Abandoned (متروكة))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `LiveRoomStatus` |
| `canJoinLiveRoom` | boolean | 🔴 **إلزامي (Required)** | - |
| `progress` | [`LessonProgressDto`](#lessonprogressdto) | ⚪ اختياري (Optional) | - |
| `quizQuestions` | Array<[`QuizQuestionDto`](#quizquestiondto)> | 🔴 **إلزامي (Required)** | - |
| `lastAttempt` | [`QuizResultDto`](#quizresultdto) | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "courseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "lessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "courseTitle": "نص توضيحي",
    "title": "نص توضيحي",
    "order": 1,
    "isFreeDemo": true,
    "videoUrl": "نص توضيحي",
    "studyBookUrl": "نص توضيحي",
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionStartsAtUtc": "2026-11-09T10:00:00Z",
    "sessionEndsAtUtc": "2026-11-09T10:00:00Z",
    "liveRoomStatus": 0,
    "canJoinLiveRoom": true,
    "progress": {
      "lastWatchedSeconds": 1,
      "isCompleted": true,
      "completedAt": "2026-11-09T10:00:00Z"
    },
    "quizQuestions": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "text": "نص توضيحي",
        "order": 1,
        "options": [
          "..."
        ]
      }
    ],
    "lastAttempt": {
      "score": 1,
      "totalQuestions": 1,
      "submittedAtUtc": "2026-11-09T10:00:00Z",
      "answers": [
        {
          "questionId": "...",
          "selectedOptionId": "..."
        }
      ]
    }
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `GET` `/api/v1/courses/my`

- **الوصف الوظيفي**: استعراض الكورسات التي قام المستخدم الحالي بالتسجيل والاشتراك بها.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`MyCourseDtoPagginatedResultApiResponse`](#mycoursedtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`MyCourseDto`](#mycoursedto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "courseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "نص توضيحي",
        "description": "نص توضيحي",
        "coverUrl": "نص توضيحي",
        "level": 0,
        "type": 1,
        "lessonCount": 1,
        "instructors": [
          "..."
        ],
        "completedLessonCount": 1,
        "lastLessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "lastLessonTitle": "نص توضيحي",
        "lastWatchedSeconds": 1,
        "lastLessonIsCompleted": true,
        "enrolledAtUtc": "2026-11-09T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `POST` `/api/v1/courses/{id}/enroll`

- **الوصف الوظيفي**: تسجيل الطالب واشتراكه في كورس تعليمي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CourseDetailDtoApiResponse`](#coursedetaildtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `description` | string | 🔴 **إلزامي (Required)** | - |
| `coverUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseLevel` |
| `type` | `CourseType`<br>*(Enum: `1`: Live (جلسات مباشرة) | `2`: Recorded (مسجل))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseType` |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isEnrolled` | boolean | 🔴 **إلزامي (Required)** | - |
| `instructors` | Array<[`InstructorRefDto`](#instructorrefdto)> | 🔴 **إلزامي (Required)** | - |
| `lessons` | Array<[`LessonListItemDto`](#lessonlistitemdto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "نص توضيحي",
    "description": "نص توضيحي",
    "coverUrl": "نص توضيحي",
    "level": 0,
    "type": 1,
    "order": 1,
    "isEnrolled": true,
    "instructors": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "specialization": "نص توضيحي"
      }
    ],
    "lessons": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "نص توضيحي",
        "order": 1,
        "isFreeDemo": true,
        "hasStudyBook": true,
        "hasVideo": true,
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionStartsAtUtc": "2026-11-09T10:00:00Z",
        "sessionEndsAtUtc": "2026-11-09T10:00:00Z",
        "liveRoomStatus": 0,
        "canJoinLiveRoom": true
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `PUT` `/api/v1/courses/lessons/{id}/progress`

- **الوصف الوظيفي**: تحديث تقدم الطالب في الدرس ونسبة الإكمال أو إنهاء الدرس.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `lessonId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `lastWatchedSeconds` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isCompleted` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "lessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "lastWatchedSeconds": 1,
  "isCompleted": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LessonProgressDtoApiResponse`](#lessonprogressdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `lastWatchedSeconds` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isCompleted` | boolean | 🔴 **إلزامي (Required)** | - |
| `completedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "lastWatchedSeconds": 1,
    "isCompleted": true,
    "completedAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 7. `POST` `/api/v1/courses/lessons/{id}/quiz`

- **الوصف الوظيفي**: إرسال إجابات اختبار الدرس وحساب النتيجة والتقييم النهائي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `lessonId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `answers` | Array<[`SubmitQuizAnswerInput`](#submitquizanswerinput)> | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "lessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "answers": [
    {
      "questionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "selectedOptionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`QuizResultDtoApiResponse`](#quizresultdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

### <a id="coursesadmin"></a>📁 CoursesAdmin (إدارة الكورسات (المسؤول))
> 💡 **نطاق العمل**: لوحة تحكم المسؤول لإضافة وتعديل وحذف الكورسات، الدروس، الجلسات المباشرة، بنك الأسئلة وتعيين المدربين.

#### 🔹 1. `GET` `/api/v1/courses/admin/courses`

- **الوصف الوظيفي**: استعراض جميع الكورسات في لوحة المسؤول بنظام الصفحات والفلترة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Search` | string | لا | - |
| `Level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | لا | - |
| `IsActive` | boolean | لا | - |
| `InstructorId` | string (`uuid`) | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CourseListItemDtoPagginatedResultApiResponse`](#courselistitemdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`CourseListItemDto`](#courselistitemdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "نص توضيحي",
        "description": "نص توضيحي",
        "coverUrl": "نص توضيحي",
        "level": 0,
        "type": 1,
        "order": 1,
        "lessonCount": 1,
        "instructors": [
          "..."
        ],
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/courses/admin/courses`

- **الوصف الوظيفي**: إنشاء كورس تعليمي جديد وتحديد مستواه ونوعه وتفاصيله.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `Title` | string | 🔴 **إلزامي (Required)** | - |
| `Description` | string | 🔴 **إلزامي (Required)** | - |
| `CoverFile` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `CoverUrl` | string | ⚪ اختياري (Optional) | - |
| `Level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseLevel` |
| `Type` | `CourseType`<br>*(Enum: `1`: Live (جلسات مباشرة) | `2`: Recorded (مسجل))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseType` |
| `Order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "Title": "نص توضيحي",
  "Description": "نص توضيحي",
  "CoverFile": "<ملف ثنائي Binary File>",
  "CoverUrl": "نص توضيحي",
  "Level": 0,
  "Type": 1,
  "Order": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`CourseListItemDtoApiResponse`](#courselistitemdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/courses/admin/courses/{id}`

- **الوصف الوظيفي**: عرض تفاصيل كورس في لوحة الإدارة للتحكم به.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CourseAdminDetailDtoApiResponse`](#courseadmindetaildtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `description` | string | 🔴 **إلزامي (Required)** | - |
| `coverUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseLevel` |
| `type` | `CourseType`<br>*(Enum: `1`: Live (جلسات مباشرة) | `2`: Recorded (مسجل))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseType` |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `instructors` | Array<[`InstructorRefDto`](#instructorrefdto)> | 🔴 **إلزامي (Required)** | - |
| `lessons` | Array<[`LessonAdminDto`](#lessonadmindto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "نص توضيحي",
    "description": "نص توضيحي",
    "coverUrl": "نص توضيحي",
    "level": 0,
    "type": 1,
    "order": 1,
    "isActive": true,
    "instructors": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "specialization": "نص توضيحي"
      }
    ],
    "lessons": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "نص توضيحي",
        "videoUrl": "نص توضيحي",
        "studyBookUrl": "نص توضيحي",
        "order": 1,
        "isFreeDemo": true,
        "isActive": true,
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionStartsAtUtc": "2026-11-09T10:00:00Z",
        "sessionEndsAtUtc": "2026-11-09T10:00:00Z",
        "liveRoomStatus": 0,
        "quizQuestions": [
          "..."
        ]
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `PUT` `/api/v1/courses/admin/courses/{id}`

- **الوصف الوظيفي**: تعديل بيانات كورس موجود مسبقاً.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `Id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `Title` | string | 🔴 **إلزامي (Required)** | - |
| `Description` | string | 🔴 **إلزامي (Required)** | - |
| `CoverFile` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `CoverUrl` | string | ⚪ اختياري (Optional) | - |
| `Level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseLevel` |
| `Type` | `CourseType`<br>*(Enum: `1`: Live (جلسات مباشرة) | `2`: Recorded (مسجل))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseType` |
| `Order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `IsActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Title": "نص توضيحي",
  "Description": "نص توضيحي",
  "CoverFile": "<ملف ثنائي Binary File>",
  "CoverUrl": "نص توضيحي",
  "Level": 0,
  "Type": 1,
  "Order": 1,
  "IsActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`CourseListItemDtoApiResponse`](#courselistitemdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `description` | string | 🔴 **إلزامي (Required)** | - |
| `coverUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `level` | `CourseLevel`<br>*(Enum: `0`: Beginner (مبتدئ) | `1`: Elementary (أساسي) | `2`: Intermediate (متوسط) | `3`: Advanced (متقدم))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseLevel` |
| `type` | `CourseType`<br>*(Enum: `1`: Live (جلسات مباشرة) | `2`: Recorded (مسجل))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `CourseType` |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `lessonCount` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `instructors` | Array<[`InstructorRefDto`](#instructorrefdto)> | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "نص توضيحي",
    "description": "نص توضيحي",
    "coverUrl": "نص توضيحي",
    "level": 0,
    "type": 1,
    "order": 1,
    "lessonCount": 1,
    "instructors": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "specialization": "نص توضيحي"
      }
    ],
    "isActive": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `DELETE` `/api/v1/courses/admin/courses/{id}`

- **الوصف الوظيفي**: حذف كورس تعليمي من النظام.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `POST` `/api/v1/courses/admin/lessons`

- **الوصف الوظيفي**: إضافة درس جديد تابع لكورس محدد.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `CourseId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `Title` | string | 🔴 **إلزامي (Required)** | - |
| `VideoFile` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `VideoUrl` | string | ⚪ اختياري (Optional) | - |
| `StudyBookFile` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `StudyBookUrl` | string | ⚪ اختياري (Optional) | - |
| `Order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `IsFreeDemo` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "CourseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Title": "نص توضيحي",
  "VideoFile": "<ملف ثنائي Binary File>",
  "VideoUrl": "نص توضيحي",
  "StudyBookFile": "<ملف ثنائي Binary File>",
  "StudyBookUrl": "نص توضيحي",
  "Order": 1,
  "IsFreeDemo": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`LessonAdminDtoApiResponse`](#lessonadmindtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 7. `PUT` `/api/v1/courses/admin/lessons/{id}`

- **الوصف الوظيفي**: تعديل بيانات ومحتوى درس موجود.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `Id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `Title` | string | 🔴 **إلزامي (Required)** | - |
| `VideoFile` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `VideoUrl` | string | ⚪ اختياري (Optional) | - |
| `StudyBookFile` | string (`binary`) | ⚪ اختياري (Optional) | - |
| `StudyBookUrl` | string | ⚪ اختياري (Optional) | - |
| `Order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `IsFreeDemo` | boolean | 🔴 **إلزامي (Required)** | - |
| `IsActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Title": "نص توضيحي",
  "VideoFile": "<ملف ثنائي Binary File>",
  "VideoUrl": "نص توضيحي",
  "StudyBookFile": "<ملف ثنائي Binary File>",
  "StudyBookUrl": "نص توضيحي",
  "Order": 1,
  "IsFreeDemo": true,
  "IsActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LessonAdminDtoApiResponse`](#lessonadmindtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `videoUrl` | string | 🔴 **إلزامي (Required)** | - |
| `studyBookUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isFreeDemo` | boolean | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `sessionId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionStartsAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionEndsAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `liveRoomStatus` | `LiveRoomStatus`<br>*(Enum: `0`: NotStarted (لم تبدأ) | `1`: Live (جارية الآن) | `2`: Ended (انتهت) | `3`: Abandoned (متروكة))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `LiveRoomStatus` |
| `quizQuestions` | Array<[`QuizQuestionAdminDto`](#quizquestionadmindto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "نص توضيحي",
    "videoUrl": "نص توضيحي",
    "studyBookUrl": "نص توضيحي",
    "order": 1,
    "isFreeDemo": true,
    "isActive": true,
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionStartsAtUtc": "2026-11-09T10:00:00Z",
    "sessionEndsAtUtc": "2026-11-09T10:00:00Z",
    "liveRoomStatus": 0,
    "quizQuestions": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "text": "نص توضيحي",
        "order": 1,
        "isActive": true,
        "options": [
          "..."
        ]
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 8. `DELETE` `/api/v1/courses/admin/lessons/{id}`

- **الوصف الوظيفي**: حذف درس تعليمي من الكورس.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 9. `POST` `/api/v1/courses/admin/lessons/{id}/session`

- **الوصف الوظيفي**: إنشاء وربط جلسة بث مباشر بالدرس المحدد.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `lessonId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `seatCapacity` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "lessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "startsAtUtc": "2026-11-09T10:00:00Z",
  "endsAtUtc": "2026-11-09T10:00:00Z",
  "seatCapacity": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LessonAdminDtoApiResponse`](#lessonadmindtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `videoUrl` | string | 🔴 **إلزامي (Required)** | - |
| `studyBookUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isFreeDemo` | boolean | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `sessionId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionStartsAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `sessionEndsAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `liveRoomStatus` | `LiveRoomStatus`<br>*(Enum: `0`: NotStarted (لم تبدأ) | `1`: Live (جارية الآن) | `2`: Ended (انتهت) | `3`: Abandoned (متروكة))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `LiveRoomStatus` |
| `quizQuestions` | Array<[`QuizQuestionAdminDto`](#quizquestionadmindto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "نص توضيحي",
    "videoUrl": "نص توضيحي",
    "studyBookUrl": "نص توضيحي",
    "order": 1,
    "isFreeDemo": true,
    "isActive": true,
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionStartsAtUtc": "2026-11-09T10:00:00Z",
    "sessionEndsAtUtc": "2026-11-09T10:00:00Z",
    "liveRoomStatus": 0,
    "quizQuestions": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "text": "نص توضيحي",
        "order": 1,
        "isActive": true,
        "options": [
          "..."
        ]
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 10. `POST` `/api/v1/courses/admin/quiz-questions`

- **الوصف الوظيفي**: إضافة سؤال كويز جديد مع الخيارات وتحديد الإجابة الصحيحة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `lessonId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `text` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `options` | Array<[`QuizOptionInput`](#quizoptioninput)> | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "lessonId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "text": "نص توضيحي",
  "order": 1,
  "options": [
    {
      "text": "نص توضيحي",
      "isCorrect": true,
      "order": 1
    }
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`QuizQuestionAdminDtoApiResponse`](#quizquestionadmindtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 11. `PUT` `/api/v1/courses/admin/quiz-questions/{id}`

- **الوصف الوظيفي**: تعديل سؤال كويز محدد وخياراته.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `text` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `options` | Array<[`QuizOptionInput`](#quizoptioninput)> | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "text": "نص توضيحي",
  "order": 1,
  "isActive": true,
  "options": [
    {
      "text": "نص توضيحي",
      "isCorrect": true,
      "order": 1
    }
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`QuizQuestionAdminDtoApiResponse`](#quizquestionadmindtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `text` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `options` | Array<[`QuizOptionAdminDto`](#quizoptionadmindto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "text": "نص توضيحي",
    "order": 1,
    "isActive": true,
    "options": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "text": "نص توضيحي",
        "isCorrect": true,
        "order": 1
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 12. `DELETE` `/api/v1/courses/admin/quiz-questions/{id}`

- **الوصف الوظيفي**: حذف سؤال كويز من بنك الأسئلة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 13. `PUT` `/api/v1/courses/admin/courses/{id}/instructors`

- **الوصف الوظيفي**: تعيين وتحديث قائمة المدربين المعتمدين للكورس.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorIds` | Array<string (`uuid`)> | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "instructorIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorRefDtoIReadOnlyCollectionApiResponse`](#instructorrefdtoireadonlycollectionapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "نص توضيحي",
      "specialization": "نص توضيحي"
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 14. `GET` `/api/v1/courses/admin/instructors`

- **الوصف الوظيفي**: استعراض قائمة المعلمين المتاحين لتعيينهم للكورسات.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Search` | string | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorRefDtoPagginatedResultApiResponse`](#instructorrefdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`InstructorRefDto`](#instructorrefdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "specialization": "نص توضيحي"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="helpcenter"></a>📁 HelpCenter (مركز المساعدة)
> 💡 **نطاق العمل**: تصفح مقالات المساعدة والأسئلة الشائعة وتصنيفاتها للمستخدمين.

#### 🔹 1. `GET` `/api/v1/help-center/categories`

- **الوصف الوظيفي**: عرض تصنيفات مركز المساعدة المتاحة للمستخدمين.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpCategoryDtoIReadOnlyCollectionApiResponse`](#helpcategorydtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "نص توضيحي",
      "order": 1,
      "isActive": true,
      "articleCount": 1
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `GET` `/api/v1/help-center/articles`

- **الوصف الوظيفي**: البحث واستعراض مقالات المساعدة التابعة لتصنيف معين.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `CategoryId` | string (`uuid`) | لا | - |
| `Search` | string | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpArticleDtoPagginatedResultApiResponse`](#helparticledtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`HelpArticleDto`](#helparticledto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "categoryName": "نص توضيحي",
        "question": "نص توضيحي",
        "answer": "نص توضيحي",
        "order": 1,
        "isActive": true,
        "createdAt": "2026-11-09T10:00:00Z",
        "updatedAt": "2026-11-09T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/help-center/articles/{id}`

- **الوصف الوظيفي**: قراءة المحتوى الكامل لمقال مساعدة محدد.
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpArticleDtoApiResponse`](#helparticledtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `categoryId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `categoryName` | string | 🔴 **إلزامي (Required)** | - |
| `question` | string | 🔴 **إلزامي (Required)** | - |
| `answer` | string | 🔴 **إلزامي (Required)** | Plain text. Newlines are preserved; the front end is responsible for rendering them. |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `updatedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "categoryName": "نص توضيحي",
    "question": "نص توضيحي",
    "answer": "نص توضيحي",
    "order": 1,
    "isActive": true,
    "createdAt": "2026-11-09T10:00:00Z",
    "updatedAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="helpcenteradmin"></a>📁 HelpCenterAdmin (إدارة مركز المساعدة (المسؤول))
> 💡 **نطاق العمل**: إضافة وتعديل وحذف تصنيفات ومقالات مركز المساعدة وقاعدة المعرفة.

#### 🔹 1. `GET` `/api/v1/help-center/admin/categories`

- **الوصف الوظيفي**: استعراض كافة تصنيفات مركز المساعدة للإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpCategoryDtoIReadOnlyCollectionApiResponse`](#helpcategorydtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "نص توضيحي",
      "order": 1,
      "isActive": true,
      "articleCount": 1
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/help-center/admin/categories`

- **الوصف الوظيفي**: إضافة تصنيف جديد لمركز المساعدة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `name` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "name": "نص توضيحي",
  "order": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`HelpCategoryDtoApiResponse`](#helpcategorydtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `PUT` `/api/v1/help-center/admin/categories/{id}`

- **الوصف الوظيفي**: تعديل تصنيف مركز مساعدة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `name` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "نص توضيحي",
  "order": 1,
  "isActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpCategoryDtoApiResponse`](#helpcategorydtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `name` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `articleCount` | integer (`int32`) | 🔴 **إلزامي (Required)** | Number of articles that are still active inside this category. |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "نص توضيحي",
    "order": 1,
    "isActive": true,
    "articleCount": 1
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `DELETE` `/api/v1/help-center/admin/categories/{id}`

- **الوصف الوظيفي**: حذف تصنيف مساعدة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `GET` `/api/v1/help-center/admin/articles`

- **الوصف الوظيفي**: استعراض مقالات مركز المساعدة للإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `CategoryId` | string (`uuid`) | لا | - |
| `Search` | string | لا | - |
| `IsActive` | boolean | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpArticleDtoPagginatedResultApiResponse`](#helparticledtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`HelpArticleDto`](#helparticledto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "categoryName": "نص توضيحي",
        "question": "نص توضيحي",
        "answer": "نص توضيحي",
        "order": 1,
        "isActive": true,
        "createdAt": "2026-11-09T10:00:00Z",
        "updatedAt": "2026-11-09T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `POST` `/api/v1/help-center/admin/articles`

- **الوصف الوظيفي**: إضافة مقال جديد في مركز المساعدة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `categoryId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `question` | string | 🔴 **إلزامي (Required)** | - |
| `answer` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "question": "نص توضيحي",
  "answer": "نص توضيحي",
  "order": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`HelpArticleDtoApiResponse`](#helparticledtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 7. `PUT` `/api/v1/help-center/admin/articles/{id}`

- **الوصف الوظيفي**: تعديل مقال مساعدة موجود.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `question` | string | 🔴 **إلزامي (Required)** | - |
| `answer` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "question": "نص توضيحي",
  "answer": "نص توضيحي",
  "order": 1,
  "isActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`HelpArticleDtoApiResponse`](#helparticledtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `categoryId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `categoryName` | string | 🔴 **إلزامي (Required)** | - |
| `question` | string | 🔴 **إلزامي (Required)** | - |
| `answer` | string | 🔴 **إلزامي (Required)** | Plain text. Newlines are preserved; the front end is responsible for rendering them. |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `updatedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "categoryName": "نص توضيحي",
    "question": "نص توضيحي",
    "answer": "نص توضيحي",
    "order": 1,
    "isActive": true,
    "createdAt": "2026-11-09T10:00:00Z",
    "updatedAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 8. `DELETE` `/api/v1/help-center/admin/articles/{id}`

- **الوصف الوظيفي**: حذف مقال مساعدة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="instructorapplications"></a>📁 InstructorApplications (طلبات انضمام المدربين (المسؤول))
> 💡 **نطاق العمل**: استعراض ومراجعة طلبات الانضمام المقدمة من المعلمين واعتمادها أو رفضها مع كتابة الأسباب.

#### 🔹 1. `GET` `/api/v1/instructor-applications`

- **الوصف الوظيفي**: استعراض طلبات التقديم للتدريس بنظام الصفحات وحالة الطلب.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Status` | `InstructorApplicationStatus`<br>*(Enum: `0`: Pending (قيد المراجعة) | `1`: Approved (مقبول) | `2`: Rejected (مرفوض))* | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorApplicationDtoPagginatedResultApiResponse`](#instructorapplicationdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`InstructorApplicationDto`](#instructorapplicationdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "email": "نص توضيحي",
        "phoneNumber": "نص توضيحي",
        "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "countryName": "نص توضيحي",
        "specialization": "نص توضيحي",
        "bio": "نص توضيحي",
        "yearsOfExperience": 1,
        "education": "نص توضيحي",
        "linkedInUrl": "نص توضيحي",
        "hourlyRate": 10.5,
        "status": 0,
        "submittedAt": "2026-11-09T10:00:00Z",
        "reviewedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "reviewedAt": "2026-11-09T10:00:00Z",
        "reviewNotes": "نص توضيحي",
        "certificates": [
          "..."
        ]
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `GET` `/api/v1/instructor-applications/{id}`

- **الوصف الوظيفي**: الاطلاع على التفاصيل الكاملة لطلب التقديم والشهادات المرفوعة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorApplicationDtoApiResponse`](#instructorapplicationdtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `phoneNumber` | string | 🔴 **إلزامي (Required)** | - |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `specialization` | string | 🔴 **إلزامي (Required)** | - |
| `bio` | string | 🔴 **إلزامي (Required)** | - |
| `yearsOfExperience` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `education` | string | 🔴 **إلزامي (Required)** | - |
| `linkedInUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `hourlyRate` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `status` | `InstructorApplicationStatus`<br>*(Enum: `0`: Pending (قيد المراجعة) | `1`: Approved (مقبول) | `2`: Rejected (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `InstructorApplicationStatus` |
| `submittedAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `reviewedByUserId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `reviewedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `reviewNotes` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `certificates` | Array<[`InstructorCertificateDto`](#instructorcertificatedto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "نص توضيحي",
    "email": "نص توضيحي",
    "phoneNumber": "نص توضيحي",
    "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryName": "نص توضيحي",
    "specialization": "نص توضيحي",
    "bio": "نص توضيحي",
    "yearsOfExperience": 1,
    "education": "نص توضيحي",
    "linkedInUrl": "نص توضيحي",
    "hourlyRate": 10.5,
    "status": 0,
    "submittedAt": "2026-11-09T10:00:00Z",
    "reviewedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "reviewedAt": "2026-11-09T10:00:00Z",
    "reviewNotes": "نص توضيحي",
    "certificates": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "filePath": "نص توضيحي",
        "originalFileName": "نص توضيحي",
        "type": 0
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `POST` `/api/v1/instructor-applications/approve`

- **الوصف الوظيفي**: الموافقة على طلب انضمام المعلم وترقية حسابه إلى مدرب معتمد.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `applicationId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `reviewNotes` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewNotes": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `POST` `/api/v1/instructor-applications/reject`

- **الوصف الوظيفي**: رفض طلب المعلم مع إرسال سبب الرفض.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `applicationId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `reviewNotes` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewNotes": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="instructoravailabilityadmin"></a>📁 InstructorAvailabilityAdmin (إدارة أوقات المدربين (المسؤول))
> 💡 **نطاق العمل**: تحكم المسؤول في جدول وتوفر مواعيد المدربين وساعات العمل الأسبوعية.

#### 🔹 1. `GET` `/api/v1/instructor-availability/pagginated`

- **الوصف الوظيفي**: استعراض مواعيد وتوفر المدربين من قبل الإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `InstructorId` | string (`uuid`) | لا | - |
| `Day` | `DayOfWeek`<br>*(Enum: `0`: Sunday (الأحد) | `1`: Monday (الإثنين) | `2`: Tuesday (الثلاثاء) | `3`: Wednesday (الأربعاء) | `4`: Thursday (الخميس) | `5`: Friday (الجمعة) | `6`: Saturday (السبت))* | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorAvailabilityRowDtoPagginatedResultApiResponse`](#instructoravailabilityrowdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`InstructorAvailabilityRowDto`](#instructoravailabilityrowdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorFullName": "نص توضيحي",
        "specialization": "نص توضيحي",
        "day": 1,
        "dayName": "نص توضيحي",
        "startTime": "نص توضيحي",
        "endTime": "نص توضيحي"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/instructor-availability/instructors/{instructorId}/weekly-slots`

- **الوصف الوظيفي**: إضافة فترة أسبوعية متاحة لمدرب معين من قبل الإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `instructorId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `day` | `DayOfWeek`<br>*(Enum: `0`: Sunday (الأحد) | `1`: Monday (الإثنين) | `2`: Tuesday (الثلاثاء) | `3`: Wednesday (الأربعاء) | `4`: Thursday (الخميس) | `5`: Friday (الجمعة) | `6`: Saturday (السبت))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `DayOfWeek` |
| `startTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |
| `endTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "day": 0,
  "startTime": "نص توضيحي",
  "endTime": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`AvailabilitySlotDtoApiResponse`](#availabilityslotdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `PUT` `/api/v1/instructor-availability/weekly-slots/{id}`

- **الوصف الوظيفي**: تعديل فترة أسبوعية لمدرب بواسطة الإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `day` | `DayOfWeek`<br>*(Enum: `0`: Sunday (الأحد) | `1`: Monday (الإثنين) | `2`: Tuesday (الثلاثاء) | `3`: Wednesday (الأربعاء) | `4`: Thursday (الخميس) | `5`: Friday (الجمعة) | `6`: Saturday (السبت))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `DayOfWeek` |
| `startTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |
| `endTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "day": 0,
  "startTime": "نص توضيحي",
  "endTime": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`AvailabilitySlotDtoApiResponse`](#availabilityslotdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `day` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `dayName` | string | 🔴 **إلزامي (Required)** | - |
| `startTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |
| `endTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "day": 1,
    "dayName": "نص توضيحي",
    "startTime": "نص توضيحي",
    "endTime": "نص توضيحي"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `DELETE` `/api/v1/instructor-availability/weekly-slots/{id}`

- **الوصف الوظيفي**: حذف فترة أسبوعية لمدرب.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="instructorprofile"></a>📁 InstructorProfile (الملف التعريفي للمدرب)
> 💡 **نطاق العمل**: عرض وتحديث بيانات المدرب، التخصصات، السيرة الذاتية، اللغات، والبيانات البنكية.

#### 🔹 1. `GET` `/api/v1/instructor-profile`

- **الوصف الوظيفي**: جلب تفاصيل الملف المهني للمدرب الحالي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorProfileDtoApiResponse`](#instructorprofiledtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `profile` | [`UserProfileDto`](#userprofiledto) | ⚪ اختياري (Optional) | - |
| `vacationMode` | boolean | 🔴 **إلزامي (Required)** | - |
| `specialization` | string | 🔴 **إلزامي (Required)** | - |
| `bio` | string | 🔴 **إلزامي (Required)** | - |
| `yearsOfExperience` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `education` | string | 🔴 **إلزامي (Required)** | - |
| `linkedInUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `hourlyRate` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `applicationStatus` | `InstructorApplicationStatus`<br>*(Enum: `0`: Pending (قيد المراجعة) | `1`: Approved (مقبول) | `2`: Rejected (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `InstructorApplicationStatus` |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `certificates` | Array<[`InstructorCertificateDto`](#instructorcertificatedto)> | 🔴 **إلزامي (Required)** | - |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `updatedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "profile": {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "نص توضيحي",
      "fullName": "نص توضيحي",
      "profilePictureUrl": "نص توضيحي",
      "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "countryName": "نص توضيحي",
      "timezoneId": "نص توضيحي",
      "language": 1,
      "roles": [
        "نص توضيحي"
      ],
      "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createdAt": "2026-11-09T10:00:00Z"
    },
    "vacationMode": true,
    "specialization": "نص توضيحي",
    "bio": "نص توضيحي",
    "yearsOfExperience": 1,
    "education": "نص توضيحي",
    "linkedInUrl": "نص توضيحي",
    "hourlyRate": 10.5,
    "applicationStatus": 0,
    "isActive": true,
    "certificates": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "filePath": "نص توضيحي",
        "originalFileName": "نص توضيحي",
        "type": 0
      }
    ],
    "createdAt": "2026-11-09T10:00:00Z",
    "updatedAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `PUT` `/api/v1/instructor-profile`

- **الوصف الوظيفي**: تحديث بيانات الملف المهني للمدرب (الخبرات، اللغات، النبذة، الحساب البنكي).
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `fullName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `language` | `Language`<br>*(Enum: `1`: En (الإنجليزية) | `2`: Ar (العربية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `Language` |
| `profilePictureUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `specialization` | string | 🔴 **إلزامي (Required)** | - |
| `bio` | string | 🔴 **إلزامي (Required)** | - |
| `yearsOfExperience` | integer (`int32`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `education` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `linkedInUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `hourlyRate` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "fullName": "نص توضيحي",
  "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "language": 1,
  "profilePictureUrl": "نص توضيحي",
  "specialization": "نص توضيحي",
  "bio": "نص توضيحي",
  "yearsOfExperience": 1,
  "education": "نص توضيحي",
  "linkedInUrl": "نص توضيحي",
  "hourlyRate": 10.5
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorProfileDtoApiResponse`](#instructorprofiledtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `profile` | [`UserProfileDto`](#userprofiledto) | ⚪ اختياري (Optional) | - |
| `vacationMode` | boolean | 🔴 **إلزامي (Required)** | - |
| `specialization` | string | 🔴 **إلزامي (Required)** | - |
| `bio` | string | 🔴 **إلزامي (Required)** | - |
| `yearsOfExperience` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `education` | string | 🔴 **إلزامي (Required)** | - |
| `linkedInUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `hourlyRate` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `applicationStatus` | `InstructorApplicationStatus`<br>*(Enum: `0`: Pending (قيد المراجعة) | `1`: Approved (مقبول) | `2`: Rejected (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `InstructorApplicationStatus` |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `certificates` | Array<[`InstructorCertificateDto`](#instructorcertificatedto)> | 🔴 **إلزامي (Required)** | - |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `updatedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "profile": {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "نص توضيحي",
      "fullName": "نص توضيحي",
      "profilePictureUrl": "نص توضيحي",
      "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "countryName": "نص توضيحي",
      "timezoneId": "نص توضيحي",
      "language": 1,
      "roles": [
        "نص توضيحي"
      ],
      "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createdAt": "2026-11-09T10:00:00Z"
    },
    "vacationMode": true,
    "specialization": "نص توضيحي",
    "bio": "نص توضيحي",
    "yearsOfExperience": 1,
    "education": "نص توضيحي",
    "linkedInUrl": "نص توضيحي",
    "hourlyRate": 10.5,
    "applicationStatus": 0,
    "isActive": true,
    "certificates": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "filePath": "نص توضيحي",
        "originalFileName": "نص توضيحي",
        "type": 0
      }
    ],
    "createdAt": "2026-11-09T10:00:00Z",
    "updatedAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="liveclasses"></a>📁 LiveClasses (الفصول الافتراضية المباشرة — LiveKit)
> 💡 **نطاق العمل**: تكامل الفصول الحية 1:1 عبر LiveKit Cloud: توليد توكن الدخول، بدء الجلسة، الانضمام، المغادرة، والإنهاء. الصوت والصورة لا يمرّان على هذا السيرفر. دليل المزوِّد: [`docs/livekit-integration/README.md`](docs/livekit-integration/README.md)

#### 🔹 1. `GET` `/api/v1/live-classes/config`

- **الوصف الوظيفي**: جلب إعدادات بيئة البث المباشر (عنوان خادم LiveKit وحالة التهيئة).
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LiveClassConfigDtoApiResponse`](#liveclassconfigdtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `enabled` | boolean | 🔴 **إلزامي (Required)** | `false` لو بيانات المزوِّد فاضية في `appsettings` |
| `url` | string | 🔴 **إلزامي (Required)** | عنوان خادم البث — فارغ لما تكون غير مهيأة |
| `joinLeadMinutes` | integer (`int32`) | 🔴 **إلزامي (Required)** | دقائق فتح باب الدخول قبل الموعد |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "enabled": true,
    "url": "wss://xxxxxxxxxxxx.livekit.cloud",
    "joinLeadMinutes": 15
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/live-classes/{sessionId}/start`

- **الوصف الوظيفي**: بدء الفصل الافتراضي من قبل المحاضر وفتح القاعة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `sessionId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LiveClassTokenDtoApiResponse`](#liveclasstokendtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `roomId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `url` | string | 🔴 **إلزامي (Required)** | عنوان خادم البث (`wss://….livekit.cloud`) — يُمرَّر لـ `room.connect(url, token)` |
| `roomName` | string | 🔴 **إلزامي (Required)** | اسم الغرفة — مطابق لما داخل التوكن (للعرض فقط) |
| `identity` | string | 🔴 **إلزامي (Required)** | معرف المشارك داخل الغرفة — مطابق لـ `sub` في التوكن |
| `uid` | integer (`int32`) | 🔴 **إلزامي (Required)** | نفس القيمة الرقمية المخزّنة في جدول الحضور |
| `token` | string | 🔴 **إلزامي (Required)** | - |
| `expiresAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `role` | string | 🔴 **إلزامي (Required)** | - |
| `isHost` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "url": "wss://xxxxxxxxxxxx.livekit.cloud",
    "roomName": "live-20a712ad078d46aa9a9a193812405bb5",
    "identity": "547820205",
    "uid": 547820205,
    "token": "نص توضيحي",
    "expiresAtUtc": "2026-11-09T10:00:00Z",
    "role": "نص توضيحي",
    "isHost": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `POST` `/api/v1/live-classes/{sessionId}/join`

- **الوصف الوظيفي**: انضمام الطالب أو المعلم إلى الفصل المباشر والحصول على توكن الدخول.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `sessionId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LiveClassTokenDtoApiResponse`](#liveclasstokendtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `roomId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `url` | string | 🔴 **إلزامي (Required)** | عنوان خادم البث (`wss://….livekit.cloud`) — يُمرَّر لـ `room.connect(url, token)` |
| `roomName` | string | 🔴 **إلزامي (Required)** | اسم الغرفة — مطابق لما داخل التوكن (للعرض فقط) |
| `identity` | string | 🔴 **إلزامي (Required)** | معرف المشارك داخل الغرفة — مطابق لـ `sub` في التوكن |
| `uid` | integer (`int32`) | 🔴 **إلزامي (Required)** | نفس القيمة الرقمية المخزّنة في جدول الحضور |
| `token` | string | 🔴 **إلزامي (Required)** | - |
| `expiresAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `role` | string | 🔴 **إلزامي (Required)** | - |
| `isHost` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "url": "wss://xxxxxxxxxxxx.livekit.cloud",
    "roomName": "live-20a712ad078d46aa9a9a193812405bb5",
    "identity": "547820205",
    "uid": 547820205,
    "token": "نص توضيحي",
    "expiresAtUtc": "2026-11-09T10:00:00Z",
    "role": "نص توضيحي",
    "isHost": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `POST` `/api/v1/live-classes/{sessionId}/leave`

- **الوصف الوظيفي**: تسجيل مغادرة المشارك للفصل المباشر.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `sessionId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`BooleanApiResponse`](#booleanapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `403` | Forbidden | - |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": true,
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `POST` `/api/v1/live-classes/{sessionId}/end`

- **الوصف الوظيفي**: إنهاء الفصل المباشر بالكامل وإغلاق القاعة للجميع.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `sessionId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`BooleanApiResponse`](#booleanapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": true,
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `GET` `/api/v1/live-classes/{sessionId}`

- **الوصف الوظيفي**: جلب حالة الفصل الافتراضي الحالية وعدد الحضور.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `sessionId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`LiveClassDetailsDtoApiResponse`](#liveclassdetailsdtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `sessionId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `roomId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `url` | string | 🔴 **إلزامي (Required)** | عنوان خادم البث لجلسة الانضمام من الواجهة |
| `roomName` | string | 🔴 **إلزامي (Required)** | اسم الغرفة (`live-{sessionId}`) |
| `status` | string | 🔴 **إلزامي (Required)** | - |
| `canJoinNow` | boolean | 🔴 **إلزامي (Required)** | - |
| `joinOpensAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `startedAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `endedAtUtc` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isHost` | boolean | 🔴 **إلزامي (Required)** | - |
| `activeParticipantsCount` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `participants` | Array<[`ParticipantDto`](#participantdto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "url": "wss://xxxxxxxxxxxx.livekit.cloud",
    "roomName": "live-20a712ad078d46aa9a9a193812405bb5",
    "status": "نص توضيحي",
    "canJoinNow": true,
    "joinOpensAtUtc": "2026-11-09T10:00:00Z",
    "startsAtUtc": "2026-11-09T10:00:00Z",
    "endsAtUtc": "2026-11-09T10:00:00Z",
    "startedAtUtc": "2026-11-09T10:00:00Z",
    "endedAtUtc": "2026-11-09T10:00:00Z",
    "isHost": true,
    "activeParticipantsCount": 1,
    "participants": [
      {
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "role": "نص توضيحي",
        "uid": 547820205,
        "joinedAtUtc": "2026-11-09T10:00:00Z",
        "isOnline": true
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 7. `POST` `/api/v1/live-classes/{sessionId}/token`

- **الوصف الوظيفي**: توليد أو تجديد توكن الوصول (JWT) لدخول الجلسة الحية قبل انتهاء صلاحيته.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `sessionId` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`RefreshTokenDtoApiResponse`](#refreshtokendtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | object |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `token` | string | 🔴 **إلزامي (Required)** | - |
| `expiresAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "token": "نص توضيحي",
    "expiresAtUtc": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="lms-api"></a>📁 LMS.API (فحص النظام (Health Check))
> 💡 **نطاق العمل**: فحص الاتصال بجذور الـ API والتأكد من سلامة الخادم.

#### 🔹 1. `GET` `/`

- **الوصف الوظيفي**: فحص حالة عمل الواجهة البرمجية (Root Health / Welcome Endpoint).
- **صلاحية الوصول (Authorization)**: 🔓 **مفتوح للعامة** (Public Endpoint)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `400` | Validation failed | object |
| `401` | Unauthorized | object |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

### <a id="myavailability"></a>📁 MyAvailability (جدول توفر المعلم (My Availability))
> 💡 **نطاق العمل**: واجهات خاصة بالمعلم لإدارة فتراته الزمنية الأسبوعية، التقويم، ونمط الإجازة (Vacation Mode).

#### 🔹 1. `GET` `/api/v1/instructor-availability/my`

- **الوصف الوظيفي**: استرجاع الفترات الأسبوعية الخاصة بالمعلم الحالي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`MyAvailabilityDtoApiResponse`](#myavailabilitydtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `vacationMode` | boolean | 🔴 **إلزامي (Required)** | - |
| `weeklySlots` | Array<[`AvailabilitySlotDto`](#availabilityslotdto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "timezoneId": "نص توضيحي",
    "vacationMode": true,
    "weeklySlots": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "day": 1,
        "dayName": "نص توضيحي",
        "startTime": "نص توضيحي",
        "endTime": "نص توضيحي"
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/instructor-availability/my/weekly-slots`

- **الوصف الوظيفي**: إضافة فترة أوقات عمل أسبوعية جديدة للمعلم الحالي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `day` | `DayOfWeek`<br>*(Enum: `0`: Sunday (الأحد) | `1`: Monday (الإثنين) | `2`: Tuesday (الثلاثاء) | `3`: Wednesday (الأربعاء) | `4`: Thursday (الخميس) | `5`: Friday (الجمعة) | `6`: Saturday (السبت))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `DayOfWeek` |
| `startTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |
| `endTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "day": 0,
  "startTime": "نص توضيحي",
  "endTime": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`AvailabilitySlotDtoApiResponse`](#availabilityslotdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `PUT` `/api/v1/instructor-availability/my/weekly-slots/{id}`

- **الوصف الوظيفي**: تعديل فترة توفر أسبوعية للمعلم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `day` | `DayOfWeek`<br>*(Enum: `0`: Sunday (الأحد) | `1`: Monday (الإثنين) | `2`: Tuesday (الثلاثاء) | `3`: Wednesday (الأربعاء) | `4`: Thursday (الخميس) | `5`: Friday (الجمعة) | `6`: Saturday (السبت))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `DayOfWeek` |
| `startTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |
| `endTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "day": 0,
  "startTime": "نص توضيحي",
  "endTime": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`AvailabilitySlotDtoApiResponse`](#availabilityslotdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `day` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `dayName` | string | 🔴 **إلزامي (Required)** | - |
| `startTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |
| `endTime` | string (`date-span`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "day": 1,
    "dayName": "نص توضيحي",
    "startTime": "نص توضيحي",
    "endTime": "نص توضيحي"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `DELETE` `/api/v1/instructor-availability/my/weekly-slots/{id}`

- **الوصف الوظيفي**: حذف فترة توفر أسبوعية للمعلم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `PUT` `/api/v1/instructor-availability/my/vacation-mode`

- **الوصف الوظيفي**: تفعيل أو إلغاء تفعيل وضع الإجازة وتحديد التواريخ.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `enabled` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "enabled": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `GET` `/api/v1/instructor-availability/my/calendar`

- **الوصف الوظيفي**: استعراض تقويم الأيام والمواعيد للمعلم خلال شهر معين.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `from` | string (`date`) | لا | - |
| `to` | string (`date`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`AvailabilityCalendarDtoApiResponse`](#availabilitycalendardtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `from` | string (`date`) | 🔴 **إلزامي (Required)** | - |
| `to` | string (`date`) | 🔴 **إلزامي (Required)** | - |
| `timezoneId` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `days` | Array<[`AvailabilityDayDto`](#availabilitydaydto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "from": "2026-11-09",
    "to": "2026-11-09",
    "timezoneId": "نص توضيحي",
    "days": [
      {
        "date": "2026-11-09",
        "available": [
          "..."
        ],
        "blocked": [
          "..."
        ],
        "booked": [
          "..."
        ]
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 7. `PUT` `/api/v1/instructor-availability/my/calendar/day`

- **الوصف الوظيفي**: تعديل أو قفل توفر المعلم في يوم محدد بالتقويم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `date` | string (`date`) | 🔴 **إلزامي (Required)** | - |
| `open` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "date": "2026-11-09",
  "open": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 8. `PUT` `/api/v1/instructor-availability/my/calendar/window`

- **الوصف الوظيفي**: تحديث نافذة الحجز وفترات التوفر عبر نطاق تاريخي.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `open` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "open": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="myinstructorapplication"></a>📁 MyInstructorApplication (متابعة طلب الانضمام (للمعلم))
> 💡 **نطاق العمل**: متابعة حالة طلب التدريس وإعادة التقديم وتعديل المرفقات في حال الرفض.

#### 🔹 1. `GET` `/api/v1/instructor-applications/my-status`

- **الوصف الوظيفي**: استعلام المعلم عن حالة طلب انضمامه وملاحظات الإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`MyInstructorApplicationDtoApiResponse`](#myinstructorapplicationdtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `applicationId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `status` | `InstructorApplicationStatus`<br>*(Enum: `0`: Pending (قيد المراجعة) | `1`: Approved (مقبول) | `2`: Rejected (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `InstructorApplicationStatus` |
| `hourlyRate` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `emailConfirmed` | boolean | 🔴 **إلزامي (Required)** | - |
| `submittedAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `reviewedAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `reviewNotes` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `certificatesCount` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `hasIntroVideo` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": 0,
    "hourlyRate": 10.5,
    "isActive": true,
    "emailConfirmed": true,
    "submittedAt": "2026-11-09T10:00:00Z",
    "reviewedAt": "2026-11-09T10:00:00Z",
    "reviewNotes": "نص توضيحي",
    "certificatesCount": 1,
    "hasIntroVideo": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/instructor-applications/resubmit`

- **الوصف الوظيفي**: إعادة تقديم طلب الانضمام بعد معالجة أسباب الرفض.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `multipart/form-data`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `Specialization` | string | 🔴 **إلزامي (Required)** | - |
| `Bio` | string | 🔴 **إلزامي (Required)** | - |
| `YearsOfExperience` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `Education` | string | ⚪ اختياري (Optional) | - |
| `LinkedInUrl` | string | ⚪ اختياري (Optional) | - |
| `HourlyRate` | number (`double`) | ⚪ اختياري (Optional) | - |
| `Certificates` | Array<string (`binary`)> | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "Specialization": "نص توضيحي",
  "Bio": "نص توضيحي",
  "YearsOfExperience": 1,
  "Education": "نص توضيحي",
  "LinkedInUrl": "نص توضيحي",
  "HourlyRate": 10.5,
  "Certificates": [
    "<ملف ثنائي Binary File>"
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | - |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="onboarding"></a>📁 Onboarding (التهيئة وتحديد المستوى (Onboarding))
> 💡 **نطاق العمل**: أسئلة تحديد المستوى والاهتمامات للمتعلمين الجدد، حفظ الإجابات، واقتراح المدربين الأنسب.

#### 🔹 1. `GET` `/api/v1/onboarding/questions`

- **الوصف الوظيفي**: جلب أسئلة استبيان التهيئة وتحديد المستوى للطلاب الجدد.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`OnboardingQuestionDtoIReadOnlyCollectionApiResponse`](#onboardingquestiondtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "key": "نص توضيحي",
      "title": "نص توضيحي",
      "type": 0,
      "order": 1,
      "minValue": 10.5,
      "maxValue": 10.5,
      "step": 10.5,
      "isActive": true,
      "options": [
        {
          "id": "...",
          "label": "...",
          "value": "...",
          "order": "...",
          "isActive": "..."
        }
      ]
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/onboarding/answers`

- **الوصف الوظيفي**: حفظ إجابات استبيان التهيئة للطالب.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `answers` | Array<[`OnboardingAnswerInput`](#onboardinganswerinput)> | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "answers": [
    {
      "questionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "optionIds": [
        "3fa85f64-5717-4562-b3fc-2c963f66afa6"
      ],
      "numericValue": 10.5
    }
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/onboarding/my-answers`

- **الوصف الوظيفي**: استرجاع الإجابات التي سجلها الطالب مسبقاً في التهيئة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`OnboardingAnswerGroupDtoIReadOnlyCollectionApiResponse`](#onboardinganswergroupdtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "questionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "key": "نص توضيحي",
      "title": "نص توضيحي",
      "answers": [
        {
          "id": "...",
          "optionId": "...",
          "optionLabel": "...",
          "optionValue": "...",
          "numericValue": "..."
        }
      ]
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `GET` `/api/v1/onboarding/instructors`

- **الوصف الوظيفي**: اقتراح أفضل المعلمين المناسبين لإجابات واهتمامات الطالب.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `MinPrice` | number (`double`) | لا | - |
| `MaxPrice` | number (`double`) | لا | - |
| `Specialization` | string | لا | - |
| `CountryId` | string (`uuid`) | لا | - |
| `Day` | `DayOfWeek`<br>*(Enum: `0`: Sunday (الأحد) | `1`: Monday (الإثنين) | `2`: Tuesday (الثلاثاء) | `3`: Wednesday (الأربعاء) | `4`: Thursday (الخميس) | `5`: Friday (الجمعة) | `6`: Saturday (السبت))* | لا | Recurring availability filter. Times are instructor-local and match a single slot
that covers the requested window. |
| `After` | string (`date-span`) | لا | - |
| `Before` | string (`date-span`) | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`InstructorCardDtoPagginatedResultApiResponse`](#instructorcarddtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`InstructorCardDto`](#instructorcarddto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "specialization": "نص توضيحي",
        "bio": "نص توضيحي",
        "yearsOfExperience": 1,
        "education": "نص توضيحي",
        "linkedInUrl": "نص توضيحي",
        "hourlyRate": 10.5,
        "countryName": "نص توضيحي",
        "introVideoPath": "نص توضيحي",
        "certificatePaths": [
          "..."
        ],
        "approvedAt": "2026-11-09T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="onboardingadmin"></a>📁 OnboardingAdmin (إدارة أسئلة التهيئة (المسؤول))
> 💡 **نطاق العمل**: إنشاء وتحديث وحذف أسئلة استبيان التهيئة والخيارات المتعددة التابعة لها.

#### 🔹 1. `GET` `/api/v1/onboarding/admin/questions`

- **الوصف الوظيفي**: استعراض جميع أسئلة التهيئة للإدارة مع خياراتها.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`OnboardingQuestionDtoIReadOnlyCollectionApiResponse`](#onboardingquestiondtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "key": "نص توضيحي",
      "title": "نص توضيحي",
      "type": 0,
      "order": 1,
      "minValue": 10.5,
      "maxValue": 10.5,
      "step": 10.5,
      "isActive": true,
      "options": [
        {
          "id": "...",
          "label": "...",
          "value": "...",
          "order": "...",
          "isActive": "..."
        }
      ]
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/onboarding/admin/questions`

- **الوصف الوظيفي**: إضافة سؤال تهيئة جديد (نوعه: اختيار واحد، متعدد، أو شريط).
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `key` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `type` | `OnboardingQuestionType`<br>*(Enum: `0`: SingleSelect (اختيار واحد) | `1`: MultiSelect (اختيار متعدد) | `2`: Slider (شريط تمرير))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `OnboardingQuestionType` |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `minValue` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `maxValue` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `step` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `options` | Array<[`OnboardingOptionInput`](#onboardingoptioninput)> | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "key": "نص توضيحي",
  "title": "نص توضيحي",
  "type": 0,
  "order": 1,
  "minValue": 10.5,
  "maxValue": 10.5,
  "step": 10.5,
  "options": [
    {
      "label": "نص توضيحي",
      "value": "نص توضيحي",
      "order": 1
    }
  ]
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`OnboardingQuestionDtoApiResponse`](#onboardingquestiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `PUT` `/api/v1/onboarding/admin/questions/{id}`

- **الوصف الوظيفي**: تعديل بيانات وترتيب سؤال تهيئة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `minValue` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `maxValue` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `step` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "نص توضيحي",
  "order": 1,
  "isActive": true,
  "minValue": 10.5,
  "maxValue": 10.5,
  "step": 10.5
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`OnboardingQuestionDtoApiResponse`](#onboardingquestiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `key` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string | 🔴 **إلزامي (Required)** | - |
| `type` | `OnboardingQuestionType`<br>*(Enum: `0`: SingleSelect (اختيار واحد) | `1`: MultiSelect (اختيار متعدد) | `2`: Slider (شريط تمرير))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `OnboardingQuestionType` |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `minValue` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `maxValue` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `step` | number (`double`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `options` | Array<[`OnboardingOptionDto`](#onboardingoptiondto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "key": "نص توضيحي",
    "title": "نص توضيحي",
    "type": 0,
    "order": 1,
    "minValue": 10.5,
    "maxValue": 10.5,
    "step": 10.5,
    "isActive": true,
    "options": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "label": "نص توضيحي",
        "value": "نص توضيحي",
        "order": 1,
        "isActive": true
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `DELETE` `/api/v1/onboarding/admin/questions/{id}`

- **الوصف الوظيفي**: حذف سؤال تهيئة من النظام.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `POST` `/api/v1/onboarding/admin/questions/{id}/options`

- **الوصف الوظيفي**: إضافة خيار جديد تابع لسؤال تهيئة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `questionId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `label` | string | 🔴 **إلزامي (Required)** | - |
| `value` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "questionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "label": "نص توضيحي",
  "value": "نص توضيحي",
  "order": 1
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`OnboardingOptionDtoApiResponse`](#onboardingoptiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 6. `PUT` `/api/v1/onboarding/admin/options/{id}`

- **الوصف الوظيفي**: تعديل خيار لسؤال تهيئة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `label` | string | 🔴 **إلزامي (Required)** | - |
| `value` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "label": "نص توضيحي",
  "value": "نص توضيحي",
  "order": 1,
  "isActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`OnboardingOptionDtoApiResponse`](#onboardingoptiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `label` | string | 🔴 **إلزامي (Required)** | - |
| `value` | string | 🔴 **إلزامي (Required)** | - |
| `order` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "label": "نص توضيحي",
    "value": "نص توضيحي",
    "order": 1,
    "isActive": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 7. `DELETE` `/api/v1/onboarding/admin/options/{id}`

- **الوصف الوظيفي**: حذف خيار لسؤال تهيئة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="reschedulerequests"></a>📁 RescheduleRequests (طلبات إعادة الجدولة)
> 💡 **نطاق العمل**: الرد على طلبات إعادة جدولة الجلسات من قبل الطرف المعني (قبول أو رفض).

#### 🔹 1. `POST` `/api/v1/reschedule-requests/{id}/approve`

- **الوصف الوظيفي**: الموافقة على طلب إعادة جدولة الجلسة وتحديث موعدها الجديد.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoApiResponse`](#sessiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorFullName` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `type` | `SessionType`<br>*(Enum: `0`: OneToOne (فردية 1-1) | `1`: Group (جماعية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionType` |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `SessionStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Completed (مكتمل) | `3`: Cancelled (ملغي) | `4`: NoShow (لم يحضر))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionStatus` |
| `price` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `seatCapacity` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `seatsBooked` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `cancelledAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `cancelReason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingStatus` | `BookingStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Cancelled (ملغي) | `3`: Declined (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `BookingStatus` |
| `live` | [`LiveSessionInfoDto`](#livesessioninfodto) | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorFullName": "نص توضيحي",
    "title": "نص توضيحي",
    "type": 0,
    "startsAtUtc": "2026-11-09T10:00:00Z",
    "endsAtUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "price": 10.5,
    "seatCapacity": 1,
    "seatsBooked": 1,
    "cancelledAt": "2026-11-09T10:00:00Z",
    "cancelReason": "نص توضيحي",
    "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "myBookingStatus": 0,
    "live": {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "نص توضيحي",
      "canJoinNow": true,
      "joinOpensAtUtc": "2026-11-09T10:00:00Z",
      "startedAtUtc": "2026-11-09T10:00:00Z",
      "endedAtUtc": "2026-11-09T10:00:00Z"
    }
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/reschedule-requests/{id}/decline`

- **الوصف الوظيفي**: رفض طلب إعادة جدولة الجلسة مع ذكر السبب.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`RescheduleRequestDtoApiResponse`](#reschedulerequestdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `sessionId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `requestedByUserId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `requestedByType` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `fromUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `toUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `RescheduleRequestStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Approved (موافق عليه) | `2`: Declined (مرفوض) | `3`: Withdrawn (مسحوب))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `RescheduleRequestStatus` |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "requestedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "requestedByType": 0,
    "fromUtc": "2026-11-09T10:00:00Z",
    "toUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "createdAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="sessions"></a>📁 Sessions (الجلسات والحجوزات (Sessions))
> 💡 **نطاق العمل**: استعراض وحجز الجلسات الفردية أو الجماعية، تأكيد أو إلغاء الحجز، وطلب إعادة الجدولة.

#### 🔹 1. `GET` `/api/v1/sessions`

- **الوصف الوظيفي**: استعراض الجلسات العامة بنظام الصفحات مع الفلترة والتاريخ.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Tab` | `SessionTab`<br>*(Enum: `0`: Upcoming (القادمة) | `1`: Completed (المكتملة) | `2`: Cancelled (الملغية))* | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoPagginatedResultApiResponse`](#sessiondtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`SessionDto`](#sessiondto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorFullName": "نص توضيحي",
        "title": "نص توضيحي",
        "type": 0,
        "startsAtUtc": "2026-11-09T10:00:00Z",
        "endsAtUtc": "2026-11-09T10:00:00Z",
        "status": 0,
        "price": 10.5,
        "seatCapacity": 1,
        "seatsBooked": 1,
        "cancelledAt": "2026-11-09T10:00:00Z",
        "cancelReason": "نص توضيحي",
        "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "myBookingStatus": 0,
        "live": {
          "roomId": "...",
          "status": "...",
          "canJoinNow": "...",
          "joinOpensAtUtc": "...",
          "startedAtUtc": "...",
          "endedAtUtc": "..."
        }
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/sessions`

- **الوصف الوظيفي**: حجز جلسة تعليمية جديدة من قبل الطالب مع المعلم المختار.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `availabilityInstanceId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "availabilityInstanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`SessionDtoApiResponse`](#sessiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/sessions/my-bookings`

- **الوصف الوظيفي**: استعراض حجوزات الطالب (القادمة، المكتملة، الملغية).
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Tab` | `SessionTab`<br>*(Enum: `0`: Upcoming (القادمة) | `1`: Completed (المكتملة) | `2`: Cancelled (الملغية))* | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoPagginatedResultApiResponse`](#sessiondtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`SessionDto`](#sessiondto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "instructorFullName": "نص توضيحي",
        "title": "نص توضيحي",
        "type": 0,
        "startsAtUtc": "2026-11-09T10:00:00Z",
        "endsAtUtc": "2026-11-09T10:00:00Z",
        "status": 0,
        "price": 10.5,
        "seatCapacity": 1,
        "seatsBooked": 1,
        "cancelledAt": "2026-11-09T10:00:00Z",
        "cancelReason": "نص توضيحي",
        "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "myBookingStatus": 0,
        "live": {
          "roomId": "...",
          "status": "...",
          "canJoinNow": "...",
          "joinOpensAtUtc": "...",
          "startedAtUtc": "...",
          "endedAtUtc": "..."
        }
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `GET` `/api/v1/sessions/bookable-slots`

- **الوصف الوظيفي**: استعراض الفترات الزمنية الشاغرة والقابلة للحجز لدى معلم محدد.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `InstructorId` | string (`uuid`) | لا | - |
| `From` | string (`date`) | لا | - |
| `To` | string (`date`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`BookableSlotDtoIReadOnlyCollectionApiResponse`](#bookableslotdtoireadonlycollectionapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "availabilityInstanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "date": "2026-11-09",
      "startsAtUtc": "2026-11-09T10:00:00Z",
      "endsAtUtc": "2026-11-09T10:00:00Z"
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 5. `GET` `/api/v1/sessions/{id}`

- **الوصف الوظيفي**: الاطلاع على تفاصيل جلسة تعليمية معينة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDetailDtoApiResponse`](#sessiondetaildtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `session` | [`SessionDto`](#sessiondto) | ⚪ اختياري (Optional) | - |
| `bookings` | Array<[`SessionBookingDto`](#sessionbookingdto)> | 🔴 **إلزامي (Required)** | Empty for a learner caller: the roster of who is in the session belongs to the instructor. |
| `rescheduleRequests` | Array<[`RescheduleRequestDto`](#reschedulerequestdto)> | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "session": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "instructorFullName": "نص توضيحي",
      "title": "نص توضيحي",
      "type": 0,
      "startsAtUtc": "2026-11-09T10:00:00Z",
      "endsAtUtc": "2026-11-09T10:00:00Z",
      "status": 0,
      "price": 10.5,
      "seatCapacity": 1,
      "seatsBooked": 1,
      "cancelledAt": "2026-11-09T10:00:00Z",
      "cancelReason": "نص توضيحي",
      "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "myBookingStatus": 0,
      "live": {
        "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "status": "نص توضيحي",
        "canJoinNow": true,
        "joinOpensAtUtc": "2026-11-09T10:00:00Z",
        "startedAtUtc": "2026-11-09T10:00:00Z",
        "endedAtUtc": "2026-11-09T10:00:00Z"
      }
    },
    "bookings": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "learnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "learnerFullName": "نص توضيحي",
        "status": 0,
        "cancelledAt": "2026-11-09T10:00:00Z",
        "cancelReason": "نص توضيحي"
      }
    ],
    "rescheduleRequests": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "requestedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "requestedByType": 0,
        "fromUtc": "2026-11-09T10:00:00Z",
        "toUtc": "2026-11-09T10:00:00Z",
        "status": 0,
        "createdAt": "2026-11-09T10:00:00Z"
      }
    ]
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `POST` `/api/v1/sessions/{id}/confirm`

- **الوصف الوظيفي**: تأكيد حجز الجلسة التعليمية.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoApiResponse`](#sessiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorFullName` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `type` | `SessionType`<br>*(Enum: `0`: OneToOne (فردية 1-1) | `1`: Group (جماعية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionType` |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `SessionStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Completed (مكتمل) | `3`: Cancelled (ملغي) | `4`: NoShow (لم يحضر))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionStatus` |
| `price` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `seatCapacity` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `seatsBooked` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `cancelledAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `cancelReason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingStatus` | `BookingStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Cancelled (ملغي) | `3`: Declined (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `BookingStatus` |
| `live` | [`LiveSessionInfoDto`](#livesessioninfodto) | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorFullName": "نص توضيحي",
    "title": "نص توضيحي",
    "type": 0,
    "startsAtUtc": "2026-11-09T10:00:00Z",
    "endsAtUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "price": 10.5,
    "seatCapacity": 1,
    "seatsBooked": 1,
    "cancelledAt": "2026-11-09T10:00:00Z",
    "cancelReason": "نص توضيحي",
    "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "myBookingStatus": 0,
    "live": {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "نص توضيحي",
      "canJoinNow": true,
      "joinOpensAtUtc": "2026-11-09T10:00:00Z",
      "startedAtUtc": "2026-11-09T10:00:00Z",
      "endedAtUtc": "2026-11-09T10:00:00Z"
    }
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 7. `POST` `/api/v1/sessions/{id}/decline`

- **الوصف الوظيفي**: رفض حجز الجلسة التعليمية.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoApiResponse`](#sessiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorFullName` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `type` | `SessionType`<br>*(Enum: `0`: OneToOne (فردية 1-1) | `1`: Group (جماعية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionType` |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `SessionStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Completed (مكتمل) | `3`: Cancelled (ملغي) | `4`: NoShow (لم يحضر))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionStatus` |
| `price` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `seatCapacity` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `seatsBooked` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `cancelledAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `cancelReason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingStatus` | `BookingStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Cancelled (ملغي) | `3`: Declined (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `BookingStatus` |
| `live` | [`LiveSessionInfoDto`](#livesessioninfodto) | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorFullName": "نص توضيحي",
    "title": "نص توضيحي",
    "type": 0,
    "startsAtUtc": "2026-11-09T10:00:00Z",
    "endsAtUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "price": 10.5,
    "seatCapacity": 1,
    "seatsBooked": 1,
    "cancelledAt": "2026-11-09T10:00:00Z",
    "cancelReason": "نص توضيحي",
    "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "myBookingStatus": 0,
    "live": {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "نص توضيحي",
      "canJoinNow": true,
      "joinOpensAtUtc": "2026-11-09T10:00:00Z",
      "startedAtUtc": "2026-11-09T10:00:00Z",
      "endedAtUtc": "2026-11-09T10:00:00Z"
    }
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 8. `POST` `/api/v1/sessions/{id}/cancel`

- **الوصف الوظيفي**: إلغاء حجز جلسة تعليمية من قبل الطالب مع توضيح السبب.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `reason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoApiResponse`](#sessiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorFullName` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `type` | `SessionType`<br>*(Enum: `0`: OneToOne (فردية 1-1) | `1`: Group (جماعية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionType` |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `SessionStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Completed (مكتمل) | `3`: Cancelled (ملغي) | `4`: NoShow (لم يحضر))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionStatus` |
| `price` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `seatCapacity` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `seatsBooked` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `cancelledAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `cancelReason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingStatus` | `BookingStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Cancelled (ملغي) | `3`: Declined (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `BookingStatus` |
| `live` | [`LiveSessionInfoDto`](#livesessioninfodto) | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorFullName": "نص توضيحي",
    "title": "نص توضيحي",
    "type": 0,
    "startsAtUtc": "2026-11-09T10:00:00Z",
    "endsAtUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "price": 10.5,
    "seatCapacity": 1,
    "seatsBooked": 1,
    "cancelledAt": "2026-11-09T10:00:00Z",
    "cancelReason": "نص توضيحي",
    "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "myBookingStatus": 0,
    "live": {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "نص توضيحي",
      "canJoinNow": true,
      "joinOpensAtUtc": "2026-11-09T10:00:00Z",
      "startedAtUtc": "2026-11-09T10:00:00Z",
      "endedAtUtc": "2026-11-09T10:00:00Z"
    }
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 9. `POST` `/api/v1/sessions/{id}/reschedule`

- **الوصف الوظيفي**: طلب إعادة جدولة موعد الجلسة إلى تاريخ ووقت آخر.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `fromUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `toUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fromUtc": "2026-11-09T10:00:00Z",
  "toUtc": "2026-11-09T10:00:00Z"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`RescheduleRequestDtoApiResponse`](#reschedulerequestdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

### <a id="teacheravailability"></a>📁 TeacherAvailability (إتاحة المعلم السريعة)
> 💡 **نطاق العمل**: تفعيل أو تعطيل وضع الإجازة السريع للمعلم.

#### 🔹 1. `PUT` `/api/v1/teacher/availability/vacation-mode`

- **الوصف الوظيفي**: التحكم السريع في وضع إجازة المعلم وتجميد استقبال الحجوزات.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `enabled` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "enabled": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`TeacherVacationModeResultDtoApiResponse`](#teachervacationmoderesultdtoapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `vacationModeEnabled` | boolean | 🔴 **إلزامي (Required)** | - |
| `hasConflicts` | boolean | 🔴 **إلزامي (Required)** | - |
| `conflictCount` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `message` | string | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "vacationModeEnabled": true,
    "hasConflicts": true,
    "conflictCount": 1,
    "message": "نص توضيحي"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="teacherreschedulerequests"></a>📁 TeacherRescheduleRequests (طلبات إعادة الجدولة للمعلم)
> 💡 **نطاق العمل**: عرض كافة طلبات إعادة الجدولة الخاصة بحصص المعلم مع إمكانية سحب الطلب.

#### 🔹 1. `GET` `/api/v1/teacher/reschedule-requests`

- **الوصف الوظيفي**: استعراض كافة طلبات إعادة الجدولة الواردة والصادرة للمعلم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Status` | `RescheduleRequestStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Approved (موافق عليه) | `2`: Declined (مرفوض) | `3`: Withdrawn (مسحوب))* | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`TeacherRescheduleRequestDtoPagginatedResultApiResponse`](#teacherreschedulerequestdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`TeacherRescheduleRequestDto`](#teacherreschedulerequestdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sessionTitle": "نص توضيحي",
        "currentStartsAtUtc": "2026-11-09T10:00:00Z",
        "currentEndsAtUtc": "2026-11-09T10:00:00Z",
        "proposedStartsAtUtc": "2026-11-09T10:00:00Z",
        "proposedEndsAtUtc": "2026-11-09T10:00:00Z",
        "status": 0,
        "requestedByType": 0,
        "isRequestedByMe": true,
        "createdAt": "2026-11-09T10:00:00Z",
        "participants": [
          "..."
        ]
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/teacher/reschedule-requests/{id}/withdraw`

- **الوصف الوظيفي**: سحب وإلغاء طلب إعادة الجدولة من قبل المعلم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`RescheduleRequestDtoApiResponse`](#reschedulerequestdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `sessionId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `requestedByUserId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `requestedByType` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `fromUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `toUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `RescheduleRequestStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Approved (موافق عليه) | `2`: Declined (مرفوض) | `3`: Withdrawn (مسحوب))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `RescheduleRequestStatus` |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "requestedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "requestedByType": 0,
    "fromUtc": "2026-11-09T10:00:00Z",
    "toUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "createdAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="teachersessions"></a>📁 TeacherSessions (جلسات المعلم)
> 💡 **نطاق العمل**: إدارة المعلم لجلساته التعليمية، الاطلاع على قائمة الطلاب المشاركين، وإلغاء أو جدولة الجلسة.

#### 🔹 1. `POST` `/api/v1/teacher/sessions/{id}/cancel`

- **الوصف الوظيفي**: إلغاء المعلم لجلسة تعليمية محجوزة مع توضيح سبب الإلغاء.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `reason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionDtoApiResponse`](#sessiondtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `instructorFullName` | string | 🔴 **إلزامي (Required)** | - |
| `title` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `type` | `SessionType`<br>*(Enum: `0`: OneToOne (فردية 1-1) | `1`: Group (جماعية))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionType` |
| `startsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `endsAtUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `status` | `SessionStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Completed (مكتمل) | `3`: Cancelled (ملغي) | `4`: NoShow (لم يحضر))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `SessionStatus` |
| `price` | number (`double`) | 🔴 **إلزامي (Required)** | - |
| `seatCapacity` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `seatsBooked` | integer (`int32`) | 🔴 **إلزامي (Required)** | - |
| `cancelledAt` | string (`date-time`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `cancelReason` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `myBookingStatus` | `BookingStatus`<br>*(Enum: `0`: Pending (قيد الانتظار) | `1`: Confirmed (مؤكد) | `2`: Cancelled (ملغي) | `3`: Declined (مرفوض))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `BookingStatus` |
| `live` | [`LiveSessionInfoDto`](#livesessioninfodto) | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "instructorFullName": "نص توضيحي",
    "title": "نص توضيحي",
    "type": 0,
    "startsAtUtc": "2026-11-09T10:00:00Z",
    "endsAtUtc": "2026-11-09T10:00:00Z",
    "status": 0,
    "price": 10.5,
    "seatCapacity": 1,
    "seatsBooked": 1,
    "cancelledAt": "2026-11-09T10:00:00Z",
    "cancelReason": "نص توضيحي",
    "myBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "myBookingStatus": 0,
    "live": {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "نص توضيحي",
      "canJoinNow": true,
      "joinOpensAtUtc": "2026-11-09T10:00:00Z",
      "startedAtUtc": "2026-11-09T10:00:00Z",
      "endedAtUtc": "2026-11-09T10:00:00Z"
    }
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `POST` `/api/v1/teacher/sessions/{id}/reschedule-requests`

- **الوصف الوظيفي**: إرسال المعلم طلباً للطالب لإعادة جدولة الجلسة لموعد آخر.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `fromUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |
| `toUtc` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fromUtc": "2026-11-09T10:00:00Z",
  "toUtc": "2026-11-09T10:00:00Z"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`RescheduleRequestDtoApiResponse`](#reschedulerequestdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 3. `GET` `/api/v1/teacher/sessions/{id}/participants`

- **الوصف الوظيفي**: استعراض قائمة الطلاب المشاركين في الجلسة وحالة حضورهم.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`SessionParticipantDtoIReadOnlyCollectionApiResponse`](#sessionparticipantdtoireadonlycollectionapiresponse) |
| `400` | Validation failed | object |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": [
    {
      "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "learnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "نص توضيحي",
      "profilePictureUrl": "نص توضيحي",
      "bookingStatus": 0,
      "bookedAt": "2026-11-09T10:00:00Z",
      "attendanceStatus": 1,
      "joinedAtUtc": "2026-11-09T10:00:00Z",
      "leftAtUtc": "2026-11-09T10:00:00Z",
      "durationMinutes": 1,
      "attendanceNotes": "نص توضيحي"
    }
  ],
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

### <a id="users"></a>📁 Users (إدارة المستخدمين (المسؤول))
> 💡 **نطاق العمل**: استعراض المستخدمين، البحث والفلترة المتقدمة، إنشاء حساب مستخدم، وتعديل بياناته أو كلمة مروره.

#### 🔹 1. `GET` `/api/v1/users/pagginated`

- **الوصف الوظيفي**: استعراض قائمة المستخدمين والبحث والفلترة حسب نوع المستخدم والحالة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**معاملات البحث والتصفية (Query Parameters):**
| المعامل | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `Search` | string | لا | - |
| `Role` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | لا | - |
| `IsActive` | boolean | لا | - |
| `PageNumber` | integer (`int32`) | لا | - |
| `PageSize` | integer (`int32`) | لا | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`UserDtoPagginatedResultApiResponse`](#userdtopagginatedresultapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `items` | Array<[`UserDto`](#userdto)> | ⚪ اختياري (Optional) | - |
| `pageNumber` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `pageSize` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalPages` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `totalCount` | integer (`int32`) | ⚪ اختياري (Optional) | - |
| `hasPreviousPage` | boolean | ⚪ اختياري (Optional) | - |
| `hasNextPage` | boolean | ⚪ اختياري (Optional) | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "نص توضيحي",
        "email": "نص توضيحي",
        "phoneNumber": "نص توضيحي",
        "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "countryName": "نص توضيحي",
        "role": 0,
        "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "isActive": true,
        "profilePictureUrl": "نص توضيحي",
        "createdAt": "2026-11-09T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 2. `GET` `/api/v1/users/{id}`

- **الوصف الوظيفي**: جلب بيانات وتفاصيل مستخدم معين للإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`UserDtoApiResponse`](#userdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `phoneNumber` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `role` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `instructorId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `profilePictureUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "نص توضيحي",
    "email": "نص توضيحي",
    "phoneNumber": "نص توضيحي",
    "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryName": "نص توضيحي",
    "role": 0,
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isActive": true,
    "profilePictureUrl": "نص توضيحي",
    "createdAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 3. `DELETE` `/api/v1/users/{id}`

- **الوصف الوظيفي**: حذف مستخدم من النظام نهائياً أو تعطيل حسابه.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 4. `POST` `/api/v1/users`

- **الوصف الوظيفي**: إنشاء حساب مستخدم جديد وتحديد دوره ومواصفاته من قبل المسؤول.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `password` | string | 🔴 **إلزامي (Required)** | - |
| `phoneNumber` | string | 🔴 **إلزامي (Required)** | - |
| `countryId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `role` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "fullName": "نص توضيحي",
  "email": "نص توضيحي",
  "password": "نص توضيحي",
  "phoneNumber": "نص توضيحي",
  "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "role": 0,
  "isActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | Success | object |
| `201` | Created | [`UserDtoApiResponse`](#userdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "message": "نص توضيحي",
  "data": null,
  "statusCode": 1
}
```

---

#### 🔹 5. `PUT` `/api/v1/users/update`

- **الوصف الوظيفي**: تعديل وتحديث بيانات مستخدم من خلال لوحة المسؤول.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `fullName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `email` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `phoneNumber` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `role` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `isActive` | boolean *(nullable)* | ⚪ اختياري (Optional) | nullable |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "نص توضيحي",
  "email": "نص توضيحي",
  "phoneNumber": "نص توضيحي",
  "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "role": 0,
  "isActive": true
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`UserDtoApiResponse`](#userdtoapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مخطط كائن البيانات المرجع في الاستجابة (`data` Payload Schema):**

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `id` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `fullName` | string | 🔴 **إلزامي (Required)** | - |
| `email` | string | 🔴 **إلزامي (Required)** | - |
| `phoneNumber` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `countryName` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `role` | `UserType`<br>*(Enum: `0`: Learner (طالب) | `1`: Instructor (معلم / مدرب) | `2`: Admin (مسؤول النظام))* | ⚪ اختياري (Optional) | قيمة عددية من تعداد `UserType` |
| `instructorId` | string (`uuid`) *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `isActive` | boolean | 🔴 **إلزامي (Required)** | - |
| `profilePictureUrl` | string *(nullable)* | ⚪ اختياري (Optional) | nullable |
| `createdAt` | string (`date-time`) | 🔴 **إلزامي (Required)** | - |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "نص توضيحي",
    "email": "نص توضيحي",
    "phoneNumber": "نص توضيحي",
    "countryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "countryName": "نص توضيحي",
    "role": 0,
    "instructorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isActive": true,
    "profilePictureUrl": "نص توضيحي",
    "createdAt": "2026-11-09T10:00:00Z"
  },
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---

#### 🔹 6. `POST` `/api/v1/users/{id}/change-password`

- **الوصف الوظيفي**: تغيير كلمة مرور أي مستخدم مباشرة من قبل الإدارة.
- **صلاحية الوصول (Authorization)**: 🔒 **يتطلب تسجيل دخول** (`Bearer JWT Token`)

**متغيرات المسار (Path Parameters):**
| المتغير | النوع | إلزامي | الوصف |
| :--- | :--- | :-: | :--- |
| `id` | string (`uuid`) | نعم | - |

**الهيدرز المطلوبة (Header Parameters):**
| الهيدر | النوع | إلزامي | الوصف والخيارات |
| :--- | :--- | :-: | :--- |
| `Accept-Language` | Enum (En) | لا | Response language En. |

**جسم الطلب (Request Body):**
- **نوع المحتوى (Content-Type)**: `application/json`

| الحقل (Field) | النوع (Type) | الحالة (Requirement) | الوصف / ملاحظات |
| :--- | :--- | :--- | :--- |
| `userId` | string (`uuid`) | 🔴 **إلزامي (Required)** | - |
| `newPassword` | string | 🔴 **إلزامي (Required)** | - |
| `confirmNewPassword` | string | 🔴 **إلزامي (Required)** | - |

**مثال جسم الطلب (Request Payload Example):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "newPassword": "نص توضيحي",
  "confirmNewPassword": "نص توضيحي"
}
```

**الاستجابات المتوقعة (Responses):**
| الرمز (Status Code) | الحالة | نوع البيانات العائدة (Schema) |
| :-: | :--- | :--- |
| `200` | OK | [`StringApiResponse`](#stringapiresponse) |
| `400` | Bad Request | [`StringApiResponse`](#stringapiresponse) |
| `401` | Unauthorized | [`StringApiResponse`](#stringapiresponse) |
| `403` | Forbidden | [`StringApiResponse`](#stringapiresponse) |
| `404` | Not Found | [`StringApiResponse`](#stringapiresponse) |
| `500` | Server error | object |

**مثال استجابة الخادم الكاملة (Response Example):**
```json
{
  "success": true,
  "isSuccess": true,
  "errors": {},
  "data": "نص توضيحي",
  "message": "نص توضيحي",
  "statusCode": 1
}
```

---
