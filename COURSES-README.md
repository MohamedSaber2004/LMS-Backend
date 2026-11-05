# دليل الـ Frontend — المكتبة المسجّلة (Courses)

الملف ده مرجع كامل لخاصية **الكورسات المسجّلة** اللي بتتقدّم قبل الـ live sessions: مكتبة الكورسات → دروس (فيديو / study book / كويز) → القفل بالـ enrollment → تقدم المتعلّم → نتيجة الكويز.

**النطاق:** Courses وLessons وQuiz (تأليف الأدمن) + قراءة المتعلّم/الضيف + Enrollment + Progress + Quiz attempts.
**آخر تحديث:** 2026-11-05
**Base URL:** `https://lms-api.runasp.net/api/v1/`

> 📌 **إسناد المدرّسين (جديد):** اتضاف جدول ربط `CourseInstructors` (Many-to-Many) مع `PUT /courses/admin/courses/{id}/instructors` و`GET /courses/admin/instructors` و`instructors[]` في كل responses الكورسات — قسم 4.5. الـ migration بتاعته `20261105114012_AddCourseInstructors` **لسه ما اتطبقتش** على الداتابيز، فالقسم ده مشتقّ من الكود مش من call حي.

> ⚠️ **حالة النشر:** الـ migrations `20261105090251_AddCoursesTables` و`20261105100738_AddCourseCoverUrl` **اتطبقت على الداتابيز** يوم 2026-11-05، لكن الكود **لسه ما اتنشرش** على `https://lms-api.runasp.net` — يعني الـ host ده هيرد **404** على `/api/v1/courses/*` لحد ما الـ deploy يحصل. الـ responses الموثّقة هنا اتاخدَت من نسخة محلية شغّالة على نفس الداتابيز.
>
> 📌 **فيه seed جاهز ومُنفّذ:** `--seed-courses` (قسم 7) أضاف **4 كورسات + 12 درس + 11 سؤال (44 اختيار)** على الداتابيز، منهم كورس واحد مسودّة (`isActive: false`). أي شاشة ديمو لاقيّة بيانات دلوقتي، والتأليف من endpoints الأدمن (قسم 4) ما زال المسار العادي للمحتوى الحقيقي. أي شاشة ديمو لاقيّة بيانات دلوقتي، والتأليف من endpoints الأدمن (قسم 4) ما زال المسار العادي للمحتوى الحقيقي.

---

## 1. قواعد مشتركة

### 1.1 الـ envelope

كل الـ responses بنفس الشكل المتعارف عليه:

```json
{
  "success": true,
  "isSuccess": true,
  "message": "Courses retrieved successfully.",
  "data": { }
}
```

`message` نص مترجم من `messages.en.json` تحت عقدة `Courses` — استخدمه للـ toast كما هو، ما تكتبش نصوص في الـ frontend.

### 1.2 الأدوار والتوكن

| المجموعة | الوصول |
|---|---|
| `GET /courses/pagginated`, `GET /courses/{id}`, `GET /courses/lessons/{id}/content` | **AllowAnonymous** (ضيف أو مسجّل) |
| `GET /courses/my`, `POST /courses/{id}/enroll`, `PUT /courses/lessons/{id}/progress`, `POST /courses/lessons/{id}/quiz` | **Learner** ( bearer إلزامي) |
| `POST/PUT/DELETE /courses/admin/*`, `GET /courses/admin/*` | **Admin** ( bearer إلزامي) |

الثلاث endpoints العامين بيدعموا توكن متعلّم **اختياري**: لو بعت `Authorization: Bearer <learner_token>` على `GET /courses/{id}` أو `GET /courses/lessons/{id}/content`، الرد هيتضمّن حالته هو (`isEnrolled`, `progress`, `lastAttempt`). من غير توكن = حالة ضيف.

- الدور غلط → **403**.
- متعلّم مسجّل لكن ملوش صف `Learner` → **403** (نفس قاعدة `/sessions`).

### 1.3 الـ paging

`pagginated` بياخد `pageNumber` (افتراضي 1) و`pageSize` (افتراضي 20، أقصى **50**). لو `pageSize > 50` → **400**. الرد:

```json
{ "items": [], "totalCount": 3, "pageNumber": 1, "pageSize": 20, "totalPages": 1, "hasPreviousPage": false, "hasNextPage": false }
```

### 1.4 المستوى (Level)

`level` enum برقم صحيح:

| القيمة | الاسم |
|---|---|
| 0 | Beginner |
| 1 | Elementary |
| 2 | Intermediate |
| 3 | Advanced |

 أي قيمة برقم دول → **400** (`Course level is not valid.`).

---

## 2. نموذج البيانات

```
Course ──< Lesson ──< QuizQuestion ──< QuizOption (IsCorrect)
Course >──< Instructor  (CourseInstructors: Many-to-Many، جدول ربط بس)
Learner ──< CourseEnrollment >── Course
Learner ──< LessonProgress >── Lesson     (LastWatchedSeconds, IsCompleted, CompletedAt)
Learner ──< QuizAttempt >── Lesson ──< QuizAttemptAnswer (QuestionId, SelectedOptionId)
```

- **enrollment** = البوابة. مفيش جدول اشتراك أو دفع في الشريحة دي.
- **إسناد المدرّسين = نسبة عرض بس:** الكورس يقدر يتأسند له **أكتر من مدرّس** (`instructors[]`)، والمدرّس يقدر يبقى في أكتر من كورس. الإسناد **ما بيعطيش** المدرّس صلاحية تحرير الدروس أو الكويزات — التأليففضل للأدمن وحده.
- **النشر ما بيشتارش مدرس:** `isActive: true` لي كورس من غير أي مدرّسين عادي، و`instructors[]` بترجع `[]` ففاضي.
- **`Course.CoverUrl`** nullable: توكن صورة (`0_….jpg`) أو لينك خارجي أو `/static/…` من الـ seed أو `null`.
- **درس واحد بيتحكم في القفل:** `lesson.isFreeDemo = true` → مفتوح للضيف؛ أي درس تاني → محتاج enrollment في الـ course بتاعه.
- **الكويز ما بيقفلش حاجة:** الدرجة بتتحفظ وبس. مفيش "راسب" ومفيش محاولة ثانية ممنوعة.
- التواريخ كلها **UTC** و`*At` في الـ responses UTC كما هو.

---

## 3. endpoints المتعلّم / الضيف

### 3.1 `GET /courses/pagginated` — مكتبة الكورسات

`[AllowAnonymous]` — كروت الشاشة الرئيسية.

Query: `search` (≤100 حرف على العنوان والوصف)، `level`، `instructorId` (اختياري — يرجّع الكورسات المسنّد لها المدرّس ده)، `pageNumber`، `pageSize`.

`data.items[]`:
```json
{ "id": "guid", "title": "Egyptian Arabic 101", "description": "...", "coverUrl": "/static/courses/egyptian-arabic-101.svg", "level": 0, "order": 1, "lessonCount": 4, "instructors": [ { "id": "guid", "userId": "guid", "fullName": "Maria N.", "specialization": "Egyptian Arabic" } ], "isActive": true }
```
الكورسات غير المنشورة، والكورسات اللي ملهاش دروس منشورة، **ما بيظهروش خالص** هنا.

`coverUrl` ممكن تكون `null` (الأدمن ما حدّدش غلاف) → حط placeholder في الـ frontend. لو القيمة بدأت بـ `/` فهي مسار على **نفس origin الـ API** (`https://lms-api.runasp.net/static/…`)؛ ولو `http…` فهي صورة خارجية من الأدمن استخدمها زي ما هي.

### 3.2 `GET /courses/{id}` — تفاصيل الكورس وقائمة الدروس

`[AllowAnonymous]` — شاشة الكورس.

```json
{
  "id": "guid", "title": "...", "description": "...", "coverUrl": "/static/courses/egyptian-arabic-101.svg",
  "level": 0, "order": 1,
  "isEnrolled": false,
  "instructors": [ { "id": "guid", "userId": "guid", "fullName": "Maria N.", "specialization": "Egyptian Arabic" } ],
  "lessons": [
    { "id": "guid", "title": "Greetings", "order": 1, "isFreeDemo": true, "hasStudyBook": false }
  ]
}
```
⚠️ قائمة الدروس **مفيهاش `videoUrl`** — `hasStudyBook` بوليان بس. ده مقصود: الـ content بيجي من 3.3.

`isEnrolled` = `false` للضيف وللمتعلّم اللي لسه ما التحقش. الكورس غير المنشور → **404**.

### 3.3 `GET /courses/lessons/{id}/content` — تشغيل الدرس

`[AllowAnonymous]` — شاشة الدرس (Video / Study book / Quiz).

```json
{
  "courseId": "guid", "lessonId": "guid", "courseTitle": "Egyptian Arabic 101",
  "title": "Greetings", "order": 1, "isFreeDemo": false,
  "videoUrl": "1_2f7c9e64-3a5b-4d1f-9a2e-6f1c0b7d8e91.mp4",
  "studyBookUrl": "3_7ab1e0c4-58d6-4f2b-b0c9-9d3e2f6a1b55.pdf",
  "progress": { "lastWatchedSeconds": 252, "isCompleted": false, "completedAt": null },
  "quizQuestions": [
    { "id": "guid", "text": "How are you?", "order": 1,
      "options": [ { "id": "guid", "text": "Keefak", "order": 1 } ] }
  ],
  "lastAttempt": { "score": 2, "totalQuestions": 3, "submittedAtUtc": "2026-11-05T10:00:00Z",
    "answers": [ { "questionId": "guid", "selectedOptionId": "guid" } ] }
}
```
- `videoUrl` و`studyBookUrl` **بيجوا خام**: إما توكن رفع (`{place}_{guid}.ext`) أو لينك خارجي كتبه الأدمن. التوكن بيتشغّل مباشرة من `/files/{videoUrl}` → `GET /files/1_2f7c….mp4`، والمسار ده بيدعم **HTTP Range** (فـ seek في الـ `<video>` شغال) ومفتوح **من غير توكن**. لو القيمة لينك خارجي (`http…`) استخدمها زي ما هي.
- `progress` و`lastAttempt` = `null` للضيف أو لو المتعلّم ما فتحش الدرس قبل كده / ما حلّش الكويز.
- `quizQuestions` **مفيهاش** تحديد الإجابة الصح (الـ `isCorrect` بيظهر في endpoints الأدمن فقط).
- `studyBookUrl` = `null` → اخفي تبويب Study book.
- **القفل:** لو `isFreeDemo: false` والمتعلّم ملهوش enrollment → **403** ورسالتها `Enroll in this course to open this lesson.` استخدمها للـ CTA. الضيف على درس غير ديمو → **403** كمان.
- الدرس المحذوف، أو درس في كورس غير منشور → **404**.

### 3.4 `GET /courses/my` — كورساتي

`[RoleAuthorize(Learner)]` — Continue watching.

Query: `pageNumber`, `pageSize`. الترتيب: الأحدث التحاقًا الأول.

`data.items[]`:
```json
{
  "courseId": "guid", "title": "...", "description": "...", "coverUrl": "/static/courses/arabic-for-work.svg", "level": 0,
  "lessonCount": 4, "completedLessonCount": 1,
  "instructors": [ { "id": "guid", "userId": "guid", "fullName": "Maria N.", "specialization": "Egyptian Arabic" } ],
  "lastLessonId": "guid", "lastLessonTitle": "Numbers",
  "lastWatchedSeconds": 120, "lastLessonIsCompleted": false,
  "enrolledAtUtc": "2026-11-05T09:00:00Z"
}
```
⚠️ `lastLesson*` و`lastWatchedSeconds` بتكون `null` لو المتعلّم التحق بس ما فتحش أي درس. شريط التقدم = `completedLessonCount / lessonCount` (احسبه في الـ frontend — مفيش نسبة مئوية جاية من الـ API).

### 3.5 `POST /courses/{id}/enroll` — الالتحاق

`[RoleAuthorize(Learner)]` — مفيش body. بياخد الـ `courseId` من الـ route.

`data` = نفس شكل **3.2** بـ `isEnrolled: true`.

- الالتحاق مجاني و**idempotent**: لو متحجز قبل كده، الرد **200** بنفس القائمة ورسالة `You are already enrolled in this course.` (ما فيش 409).
- كورس غير منشور أو محذوف → **404**.

### 3.6 `PUT /courses/lessons/{id}/progress` — حفظ موضع المشاهدة

`[RoleAuthorize(Learner)]` — `lessonId` من الـ route، والـ body:

```json
{ "lastWatchedSeconds": 252, "isCompleted": false }
```
`data`:
```json
{ "lastWatchedSeconds": 252, "isCompleted": false, "completedAt": null }
```
- السطر بيتعمل لأول مرة وبيتحدّث بعدها (سطر واحد لكل درس/متعلّم).
- `isCompleted: true` بيثبّت `completedAt` **لأول مرة فقط** — ما بيرجعش `null` لو رجّعت `false`.
- `lastWatchedSeconds` سالب → **400**.
- درس مقفول → **403** (نفس 3.3).
- استدعِها من الـ player كل ~15 ثانية مش كل ثانية (كل نداء = `PUT` على الداتابيز).

### 3.7 `POST /courses/lessons/{id}/quiz` — تسليم الكويز

`[RoleAuthorize(Learner)]` — `lessonId` من الـ route، والـ body:

```json
{ "answers": [ { "questionId": "guid", "selectedOptionId": "guid" } ] }
```
`data` (كود **201**):
```json
{ "score": 2, "totalQuestions": 3, "submittedAtUtc": "2026-11-05T10:12:00Z",
  "answers": [ { "questionId": "guid", "selectedOptionId": "guid" } ] }
```
- `score` = عدد الأسئلة اللي إجاباتها صحيحة، و`totalQuestions` = عدد أسئلة الدرس **المنشورة** (مش عدد المرسال). يعني سؤال من غير إجابة = صفر له.
- الأسئلة اللي سبتها من غير إجابة ما بتظهرش في `answers`؛ الـ frontend يقارن بـ `quizQuestions` من 3.3.
- **الدرجة ما بتقفلش حاجة** — افتح الدرس اللي بعده على طول.
- أخطاء الـ 400: `answers` فاضي / نفس `questionId` متكرر / `questionId` مش بتاع الدرس / `selectedOptionId` مش بتاع السؤال.
- إعادة المحاولة = attempt جديد (القديم بيفضل في السجل، و`lastAttempt` دايمًا الأحدث).

---

## 4. endpoints الأدمن (تأليف المحتوى)

كلها `Authorization: Bearer <admin_token>` + `[RoleAuthorize(UserType.Admin)]`.

| Method | Route | الوظيفة |
|---|---|---|
| GET | `/courses/admin/courses` | كل الكورسات (غير المنشورة كمان) — `search`, `level`, `isActive`, paging |
| GET | `/courses/admin/courses/{id}` | الكورس بشجرته الكاملة (الدروس + الأسئلة + **الإجابة الصح**) |
| POST | `/courses/admin/courses` | إنشاء كورس — `multipart/form-data` (غلاف) |
| PUT | `/courses/admin/courses/{id}` | تعديل كورس — `multipart/form-data` (غلاف) |
| DELETE | `/courses/admin/courses/{id}` | حذف ناعم |
| POST | `/courses/admin/lessons` | إنشاء درس جوه كورس |
| PUT | `/courses/admin/lessons/{id}` | تعديل درس |
| DELETE | `/courses/admin/lessons/{id}` | حذف درس |
| POST | `/courses/admin/quiz-questions` | إنشاء سؤال + اختياراته |
| PUT | `/courses/admin/quiz-questions/{id}` | تعديل سؤال + اختياراته |
| DELETE | `/courses/admin/quiz-questions/{id}` | حذف سؤال واختياراته |

### 4.1 الكورس — `multipart/form-data`

الكورس بقى بياخد غلاف، فـ الـ JSON body راح ومكانه `FormData` (نفس سياسة الدرس في **4.2**).

```
POST /courses/admin/courses          Content-Type: multipart/form-data
  title      = Egyptian Arabic 101   (≤ 200, إلزامي)
  description = …                     (≤ 20000, إلزامي)
  coverFile  = <binary .jpg|.jpeg|.png|.gif|.bmp|.webp>   ≤ 5 MB
  coverUrl   = https://…/cover.jpg                        (≤ 500)
  level      = 0                      (Beginner|Elementary|Intermediate|Advanced)
  order      = 1                      (≥ 0)
```

`PUT /courses/admin/courses/{id}` = نفس الحقول + `isActive` (افتراضي `true`)؛ الـ `id` من الـ route.

**قواعد الغلاف:**
- الغلاف **اختياري** في الـ POST: `coverFile` أو `coverUrl` أو لا شيء (`coverUrl: null` في الرد). لو الاتنين اتبعتوا، **الـ file بيكسب** والـ `coverUrl` بيتم تجاهله.
- في الـ PUT: الحقل **مش موجود** → الغلاف الحالي بيضل زي ما هو؛ **فاضي** (`""`) → **امسح** الغلاف؛ فيه لينك → استبدل.
- امتداد صورة غلط أو حجم أكتر من **5 MB** → **400** قبل أي رفع على الديسك.
- بعد PUT ناجح استبدل غلاف قديم بصورة مرفوعة، **الملف القديم بيتمسح من الديسك**؛ لو الغلاف القديم كان لينك خارجي ما بيمسحش.

الرد = عنصر قائمة زي **3.1** (`201` عند الإنشاء، `200` عند التعديل) — يعني بيرجع `coverUrl` **بالقيمة الخام**: توكن (`0_<guid>.jpg`) أو اللينك الخارجي. التوكن بيتشغّل من `/files/{token}` زي الفيديو في **3.3**.

⚠️ مفيش unique constraint على `order` — قيمته للترتيب فقط، وممكن تتكرر (الـ seed خلّيها 1..4 بدون تكرار).

### 4.2 الدرس — `multipart/form-data`

الدرس **مالهوش JSON body**. الرفع بيصير في نفس الـ endpoint (نفس سياسة `POST /authentication/register` و`/instructor-application/resubmit`)، فـ `FormData` هو اللي يتبعت.

```
POST /courses/admin/lessons          Content-Type: multipart/form-data
  courseId      = guid                (إلزامي)
  title         = Greetings           (≤ 200)
  videoFile     = <binary .mp4|.avi|.mkv|.mov|.wmv>   ≤ 1 GB
  videoUrl      = https://…/greetings.mp4             (≤ 500)
  studyBookFile = <binary .pdf|.doc|.docx|.xls|.xlsx|.txt|.zip|.rar>  ≤ 50 MB
  studyBookUrl  = https://…/greetings.pdf             (≤ 500)
  order         = 1                   (≥ 0)
  isFreeDemo    = true
```

`PUT /courses/admin/lessons/{id}` = نفس الحقول **من غير `courseId`** + `isActive` (افتراضي `true`)؛ الـ `id` من الـ route.

**قواعد المصدر (IMPORTANT):**
- **`videoFile` أو `videoUrl` — واحد منهم إلزامي في الـ POST** (`A lesson video is required…`). لو الاتنين اتبعتوا، **الـ file بيكسب** والـ `videoUrl` بيتم تجاهله.
- في الـ **PUT** مفيش إلزام: ما تبعتش `videoFile` ولا `videoUrl` → **الفيديو الحالي بيفضل زي ما هو** (مهمش تعيد رفع 500 ميجا عشان تغيّر العنوان).
- `studyBookUrl` في الـ PUT: الحقل **مش موجود** → احتفظ بالحالي؛ الحقل **فاضي** (`""`) → **امسح** الملزمة (`hasStudyBook: false`)؛ حقل فيه لينك → استبدل.
- امتداد غلط أو حجم أكبر من الحد → **400** قبل ما حاجة تترفع على الديسك. مفيش حد للحجم على اللينكات الخارجية.
- بعد PUT ناجح استبدلت فيه رِفعة قديمة بواحدة جديدة، **الملف القديم بيتمسح من الديسك**. لو الدرس كان شايل لينك خارجي، اللينك بيستنى (مفيش حاجة تتمسح).
- لو الرفع فشل بعد ما الفيديو رفع، الـ handler بيمسح اللي اترفع (مفيش ملفات يتيمة من جهة الـ API).

**اللي بيرجع:** `videoUrl` و`studyBookUrl` في الـ `lessonAdminDto` = **التوكن الخام** (`1_….mp4` / `3_….pdf`) مش URL كامل. الـ frontend يبني `/files/{token}` وقت التشغيل (زي **3.3**)، ومتخزنش الـ URL الكامل في العمود ده تاني.
الرد كله = `lessonAdminDto`: كل حقول الدرس + `isActive` + `quizQuestions[]` (`201` عند الإنشاء، `200` عند التعديل).
⚠️ **نقل درس لكورس تاني مش مدعوم** — احذفه وأنشئه في الكورس الجديد (نفس سياسة مقالات مركز المساعدة).

### 4.3 السؤال والاختيارات

```json
POST /courses/admin/quiz-questions
{
  "lessonId": "guid", "text": "How are you?", "order": 1,
  "options": [
    { "text": "Keefak", "isCorrect": true,  "order": 1 },
    { "text": "Shokran", "isCorrect": false, "order": 2 }
  ]
}
```
`PUT /courses/admin/quiz-questions/{id}` = نفس الـ body **من غير `lessonId`** + `isActive`.
القواعد: `text ≤ 500`، عدد الاختيارات **من 2 إلى 10**، كل `option.text ≤ 300`، و**اختيار واحد صح بالظبط** (`Exactly one correct option…`).
⚠️ **الـ options بتستبدل بالكامل** في كل `PUT`: الـ ids القديمة بتتحذف ناعمًا وids جديدة بتتعمل. يعني بعد أي تعديل لازم تعيد قراءة السؤال من `GET /courses/admin/courses/{id}` قبل ما تستخدم ids تانية، والـ attempts القديمة بتفضل محتفظة بـ ids قديمة (نصوص الإجابات القديمة مش قابلة للحل من الـ ids دي).

### 4.4 الحذف

- حذف كورس لسه فيه دروس منشورة → **400** و`This course cannot be deleted because it contains one or more lessons.`
- حذف درس لسه فيه أسئلة → **400** و`This lesson cannot be deleted because it contains one or more quiz questions.`
- حذف سؤال → **200** واختياراته بتتحذف معاه.
- الحذف كله **ناعم** (soft delete): السجل بيفضل في الجداول والـ progress/attempts القديمة ما بتتمسحش.

### 4.5 إسناد المدرّسين — `application/json`

`PUT /courses/admin/courses/{id}/instructors` — body:
```json
{ "instructorIds": ["guid", "guid"] }
```
`data` = القائمة الجديدة كاملًا (`InstructorRefDto[]`)، و`message` = `Course instructors updated successfully.`

- **استبدال كامل (replace-set):** أي مدرس مش في البعثة بيتشيل من الكورس. `[]` = شيل كل الإسناد. idempotent — نفس البعثة مرتين تعطى نفس النتيجة.
- **مفيش دور ولا ترتيب:** الكل سواسية، و`instructors[]` بترجع بترتيب تاريخ الإسناد. مفيش حد أقصى.
- أخطاء الـ **400**: `Course was not found.` (كورس مش موجود أو محذوف) · `Instructor identifier is required.` (guid فاضي `00000000-…` في البعثة) · `The same instructor cannot be selected more than once.` · `One or more selected instructors are not approved, active instructors.`
- الرفض بيحصل **قبل** أي حفظ: بعثة فيها مدرّس واحد مرفوض = العملية كلها 400 والموجود ما بيتغيروش.
- `GET /courses/admin/instructors?search=&pageNumber=&pageSize=` → مصدر الـ picker: المدرّسين **المعتمدين النشطين** بس. `search` بيدور على `fullName` أو `specialization` (≤100 حرف).
- **شكل `InstructorRefDto`:** `{ "id": "guid", "userId": "guid", "fullName": "…", "specialization": "…" }` — `id` هو آي-دي سجل المدرّس (نفس اللي بيستقبله `instructor-availability` و`sessions`)، و`userId` هو آي-دي اليوزر. **متبدلش بينهم.**
- الإسناد **مالوش أي أثر** على الـ enrollment أو قفل الدروس — للعرض والفلاتر بس. المدرّس نفسه ما بيستقدرش يحرّر محتوى كورساته عن طريق الـ API ده.
- الفلاتر الجديدة: `GET /courses/pagginated?instructorId={guid}` و`GET /courses/admin/courses?instructorId={guid}`.

---

## 5. ترتيب التكامل المقترح (من prototype SOPHIA)

1. شاشة الترحيب → `GET /courses/pagginated` بدون توكن.
2. دخول الكورس → `GET /courses/{id}` (توكن اختياري) → لو `isEnrolled: false` ازرار "Enroll" و`POST /courses/{id}/enroll`.
3. الضغط على درس: `isFreeDemo || isEnrolled` → `GET /courses/lessons/{id}/content`؛ غير كده اطلب Enrollment الأول (الـ API هيرد 403 كحماية نهائية).
4. أول درس ديمو في الكورس = `isFreeDemo: true` (الأدمن يعلّمه). الدرس ده وحده اللي الضيف يقدر يحل كويزه.
5. مشغّل الفيديو → `PUT .../progress` كل ~15 ثانية، و`isCompleted: true` عند النهاية.
6. الكويز → `POST .../quiz` → شاشة النتيجة (`score`/`totalQuestions`) وبعدها الدرس اللي بعده مباشرة مهما كانت الدرجة.
7. "Find a live tutor for this course" → شاشة `/instructors`؛ كمان تقدر تفلتر مكتبة الكورسات بمدرّس بعينه: `GET /courses/pagginated?instructorId={guid}`. الإسناد نفسه بيتم من الأدمن (قسم 4.5) و**بيدي نسبة عرض بس** — المدرّس ما بيرفعش دروس.

---

## 6. المصايد (خلاصة)

- كل حاجة عامة بتستبعد `isActive: false`؛ الاعتماد على `isActive` من قائمة الأدمن بس.
- الضيف على درس مقفول = **403** (مش 200 ببيانات ناقصة) — الـ 403 بيطلع من الـ handler، فـ validators الأخطاء الشكلية بس (`400`).
- `lessonCount` = عدد الدروس **المنشورة** داخل الكورس،مش الإجمالي.
- `GET /courses/{id}` لا يرجّع `videoUrl` — افتح الـ content endpoint.
- التوكن المخزّن في `Lessons.VideoUrl` / `StudyBookUrl` **مش URL كامل** — المسار الكامل بيتبنى في الـ frontend كـ `/files/{token}`. أي تعديل مستقبلي على شكل التخزين هيحتاج migrate للقيم دي.
- `/files` **مفتوح لأي حد** (ملوش لازمة توكن): الحماية الوحيدة إن الـ GUID ما يعرفش يتخمين. يعني أي شخص معاه اللينك يقدر يتفرج على درس مقفول — لو محتاج حماية حقيقية للفيديو لازم CDN/signed URLs، وده خارج الشريحة دي.
- حدود الرفع: **1 GB** للفيديو، **50 MB** للملزمة، **5 MB** لغلاف الكورس — وبتُطبَّق في الـ validator. الـ endpoints نفسها `[DisableRequestSizeLimit]` فالحماية التانية مش موجودة — متسيبش الأدمن يرفع على السيرفر بلا راقب.
- التوكن المخزّن للغلاف بنفس شكل الميديا (`0_<guid>.jpg` على مكان `Images`) → `/files/{token}`. أي فئات التوكنات (`0_` صورة / `1_` فيديو / `3_` مستند) مش بتتخفى عن بعض، فـ متعملش assumptions على الامتداد من العمود.
- ملفات الميديا بتترفع في `wwwroot/{Videos|Documents}`، والـ deploy بـ FTP **بياكل `wwwroot`** → ممكن تترفع الملفات وتختفي بعد النشر. الحل الدائم CDN/R2 برضه خارج الشريحة.
- `progress`/`lastAttempt` في الـ content مش موجودة للضيف → ما تعتمدش عليها في حساب شريط تقدم الضيف.
- التواريخ كلها UTC؛ ما تعملش إعادة تحويل (`Kind=Unspecified`).
- **`instructors[]` جاية مع الكورسات كلها** (`/courses/pagginated`، `/courses/{id}`، `/courses/my`، وقائمتي الأدمن وتفاصيله) — متجيش من endpoint منفصل، فمتعملش call زيادة لعرض "taught by".
- **الفهرس Unique على `(CourseId, InstructorId)` بس** — يعني نفس المدرّس مرة واحدة في الكورس، بس يقدر يتأسنَد تاني بعد ما يتشال (الحذف ناعم والفهرس بيستثني المحذوف). لو بعت نفس الـ guid مرتين في نفس الطلب → **400** مش dedupe صامت.
- `Accept-Language: En` — مفيش نسخة عربية من رسائل Courses لحد دلوقتي.

---

## 7. حالة الداتابيز والـ seed

**الـ migrations (اتطبقت على الداتابيز يوم 2026-11-05):**
- `20261105090251_AddCoursesTables` — جداول `Courses`, `Lessons`, `QuizQuestions`, `QuizOptions`, `CourseEnrollments`, `LessonProgress`, `QuizAttempts`, `QuizAttemptAnswers`.
- `20261105100738_AddCourseCoverUrl` — عمود `Courses.CoverUrl` (`nvarchar(500)`, nullable).
- `20261105114012_AddCourseInstructors` — **مُجهّزة ولسه ما اتطبقتش**: جدول `CourseInstructors` (`Id`, `CourseId`, `InstructorId` + أعمدة الأوديت) مع فهرس unique مشروط `(CourseId, InstructorId) WHERE IsDeleted = 0`. الإضافة additive بالظبط — مفيش أي سجل موجود بيتعدّل. لحد ما تتطبق، كل endpoints الإسناد و`instructors[]` هتفشل (الجدول مش موجود).

**الـ seed:** `LMS.Persistence/Seeding/CoursesSeeder.cs` بيتشغّل من `Program.cs` لما الـ args تشمل `--seed-courses` (وبينده كمان من `BaseSeeder.SeedAllAsync` مع `--seed`)، **واتنفّذ فعليًا** على الداتابيز:

| الكورس | `level` | `order` | دروس | أسئلة | الحالة |
|---|---|---|---|---|---|
| Egyptian Arabic 101: First Conversations | Beginner | 1 | 4 | 5 | منشور (الدرس الأول `isFreeDemo` + ملزمة) |
| Modern Standard Arabic Foundations | Elementary | 2 | 3 | 3 | منشور (الدرس الأول ديمو) |
| Arabic for Work: Meetings and Emails | Intermediate | 3 | 3 | 3 | منشور (الدرس الأول ديمو) |
| Ramadan and Culture Vocabulary | Beginner | 4 | 2 | 0 | **مسودّة** (`isActive: false`) |

= 4 كورسات + 12 درس + 11 سؤال + 44 اختيار (72 سطر). كل سؤال فيه **اختيار صح واحد بالظبط**، و`order` متسلسل من 1 من غير تكرار جوه كل كورس/درس/سؤال.

- **الـ covers:** `/static/courses/egyptian-arabic-101.svg` و`msa-foundations.svg` و`arabic-for-work.svg` و`ramadan-vocabulary.svg` — ملفات حقيقية في `LMS.API/wwwroot/static/courses/` وبتنزل مع الـ publish (عكس `wwwroot/{Videos|Documents}` اللي الـ FTP mirror بيستثنيها من الحذف).
- **مصادر الميديا:** لينكات خارجية عامة ومختبَرة (`media.w3.org`, `mdn.github.io`, `test-videos.co.uk`, PDF من `w3.org`) — **مفيش** توكنات `/files/…` في الـ seed لأن مفيش ملفات اترفعت. الـ handlers بتفرق بينهم عادي (القيمة لينك → استخدمها كما هي).
- **Idempotent:** إعادة تشغيل `--seed-courses` بترجع `0 content rows` وما بتكرارش حاجة (الفحص على العنوان بـ `IgnoreQueryFilters`).
- **نشاط المتعلّم مش متحفّظ:** الـ seeder بيكتب enrollments/progress/attempt لحساب `learner@gmail.com` فقط، والحساب ده **مش موجود** حاليًا → الرد قال `0 learner rows`. يعني `/courses/my` فارغة لأي متعلّم لحد ما تعمل enrollment حقيقي. لو عايز ديمو للـ progress، لازم حساب متعلّم فعلي (تسجيل عادي) — ما بنربطش بيانات وهمية بحسابات ناس حقيقية.
- أعمدة `CreatedBy/UpdatedBy` في أسطر الـ seed = `00000000-0000-0000-0000-000000000000` (نفس كل الـ seeders التانية في المشروع — مفيش HTTP context وقت التشغيل من الـ CLI).

**لإعادة التشغيل محليًا** (نفس الداتابيز دي، فاتصل بيها على مهل):

```
dotnet build LMS.API/LMS.API.csproj
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5412 \
  dotnet LMS.API/bin/Debug/net8.0/LMS.API.dll --seed-courses
```

الـ seed بيخلص قبل ما السيرفر يفتح البورت؛ اقفل البروسيس بعد سطر `Courses seeding finished:`.
