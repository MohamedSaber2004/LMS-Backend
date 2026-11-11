# صلاحيات الأدمن (Sub Admins) —handoff للفرونت

التاريخ: 2026-11-11 · الباك إند فقط (Fronت بره الريبو ده)

## الفكرة في سطرين

الأدمن الرئيسي (`role = Admin` أو `UserTypes` فيها bit الـ Admin) عنده كل الصلاحيات **ضمنيًا** من غير أي سطر في أي جدول.
أي حساب تاني لازم ياخد **أكواد صلاحيات** صراحة، والأكواد دي بتتخزن في جدول `AdminPermissionGrants` وبتدخل الـ JWT كـ claim اسمه `perm` (القيمة رقم).

كل فيتشر ليه بلوك من 100، والآخرين بيشوفوا أكشن مختلف فيه: `x01 = View`، `x02 = Add`، `x03 = Update`، `x04 = Delete`. يعني إضافة/تعديل/حذف تلات صلاحيات منفصلة مش حاجة واحدة.

## الأدوار (UserType)

`UserType` بقى أربع قيم، والأرقام دي بتروح جوه الـ payload بتاع `POST /api/v1/users` و `PUT /api/v1/users/update` في حقل `role` (رقر مش اسم — مفيش enum-as-string converter):

| قيمة | الدور | بتعمل إيه |
|---|---|---|
| 0 | `Learner` | متعلم، مفيش الأدمن بورد |
| 1 | `Instructor` | مدرس، مفيش الأدمن بورد |
| 2 | `Admin` | أدمن رئيسي: كل الأكواد ضمنيًا، ومفيش حاجة لازم تتقننله |
| 4 | `SubAdmin` | **الحساب اللي يقبل أكواد صلاحيات** — الدور لوحده مابيديش أي صلاحية |

`4` مش `3` لأن الأرقام دي بتستخدم كـ **bits** في claim اسمه `UserTypes` (الحساب المدرس + الأدمن بيطلع `3`، والمدرس + الأدمن الفرعي بيطلع `5`). الأرقام دي مش بتتخزن في أي عمود في الداتابيز — الأدور في جداول Identity (`AspNetRoles` / `AspNetUserRoles`) بالاسم.

قواعد الربط اللي اتعملت دلوقتي:
- **الـ grants مش بيتقنن لأي حساب غير `SubAdmin`** (غير الأدمن الرئيسي). `PUT /api/v1/admin/users/{id}/permissions` على حساب `Learner` أو `Instructor` بيرجع 400 والرسالة بيسمّي الأدور اللي فعلاً عليهن الحساب: `Permission codes can only be assigned to SubAdmin accounts. This account has the following role(s): Instructor.`
- **لو الدور اتغيّر من `SubAdmin` لدور تاني، الـ grants بتتمسح** في نفس الـ request، ومعاها `TokenVersion` بيرتفع وكل الـ refresh tokens بتلغي — يعني مفيش learner بيوصله أي صلاحية قديمة.
- **دور `SubAdmin` بيتعمل نفسه في `AspNetRoles`** أول حد ياخده من `AddUser`/`UpdateUser` (السيرفر المنشور ما بيشغّلش الـ seeder، فالـ role row مكانش موجود — اتأكدت إن الحيّ فيه `Admin` و `Instructor` و `Learner` بس).
- **الـ registration مقفول قدام الدور ده:** `POST /api/v1/authentication/register` بيقبل `0` أو `1` وبس، فمفيش حد يخلق نفسه sub-admin.
- **الدور الجديد فتح باب تصعيد، فاتقفل:** اللي معاه `UsersAdd`/`UsersUpdate` بيمنع من إسناد دور `Admin` ومن لمس حساب الأدمن الرئيسي — التفاصيل في قسم "قواعد منع التصعيد" تحت.
- التوكن بيضيف `UserTypes` فيه bit الـ 4 للحساب ده، وبيضيف أكواد الـ `perm` من الجدول — مفيش أي كود بيعامل `SubAdmin` كأنه `Admin`.


## جدول الأكواد (40 كود / 14 فيتشر)

| كود | اسم الـ permission | الفيتشر | الأكشن | الـ endpoints اللي بتتحكم فيها |
|---|---|---|---|---|
| 101 | `UsersRead` | Platform users | View | `GET /api/v1/users/pagginated`, `GET /api/v1/users/{id}` |
| 102 | `UsersAdd` | Platform users | Add | `POST /api/v1/users` |
| 103 | `UsersUpdate` | Platform users | Update | `PUT /api/v1/users/update`, `POST /api/v1/users/{id}/change-password` |
| 104 | `UsersDelete` | Platform users | Delete | `DELETE /api/v1/users/{id}` |
| 201 | `PermissionsRead` | Admin permission management | View | `GET /api/v1/admin/permissions`, `GET /api/v1/admin/users/{id}/permissions` |
| 203 | `PermissionsUpdate` | Admin permission management | Update | `PUT /api/v1/admin/users/{id}/permissions` |
| 301 | `CoursesRead` | Courses, lessons and quizzes | View | `GET /api/v1/courses/admin/courses`, `GET /api/v1/courses/admin/courses/{id}`, `GET /api/v1/courses/admin/instructors` |
| 302 | `CoursesAdd` | Courses, lessons and quizzes | Add | `POST /api/v1/courses/admin/courses`, `POST /api/v1/courses/admin/lessons`, `POST /api/v1/courses/admin/quiz-questions` |
| 303 | `CoursesUpdate` | Courses, lessons and quizzes | Update | `PUT /api/v1/courses/admin/courses/{id}`, `PUT /api/v1/courses/admin/lessons/{id}`, `POST /api/v1/courses/admin/lessons/{id}/session`, `PUT /api/v1/courses/admin/quiz-questions/{id}`, `PUT /api/v1/courses/admin/courses/{id}/instructors` |
| 304 | `CoursesDelete` | Courses, lessons and quizzes | Delete | `DELETE /api/v1/courses/admin/courses/{id}`, `DELETE /api/v1/courses/admin/lessons/{id}`, `DELETE /api/v1/courses/admin/quiz-questions/{id}` |
| 401 | `HelpCenterRead` | Help center | View | `GET /api/v1/help-center/admin/categories`, `GET /api/v1/help-center/admin/articles` |
| 402 | `HelpCenterAdd` | Help center | Add | `POST /api/v1/help-center/admin/categories`, `POST /api/v1/help-center/admin/articles` |
| 403 | `HelpCenterUpdate` | Help center | Update | `PUT /api/v1/help-center/admin/categories/{id}`, `PUT /api/v1/help-center/admin/articles/{id}` |
| 404 | `HelpCenterDelete` | Help center | Delete | `DELETE /api/v1/help-center/admin/categories/{id}`, `DELETE /api/v1/help-center/admin/articles/{id}` |
| 501 | `OnboardingRead` | Learner onboarding questions | View | `GET /api/v1/onboarding/admin/questions` |
| 502 | `OnboardingAdd` | Learner onboarding questions | Add | `POST /api/v1/onboarding/admin/questions`, `POST /api/v1/onboarding/admin/questions/{id}/options` |
| 503 | `OnboardingUpdate` | Learner onboarding questions | Update | `PUT /api/v1/onboarding/admin/questions/{id}`, `PUT /api/v1/onboarding/admin/options/{id}` |
| 504 | `OnboardingDelete` | Learner onboarding questions | Delete | `DELETE /api/v1/onboarding/admin/questions/{id}`, `DELETE /api/v1/onboarding/admin/options/{id}` |
| 601 | `PackagesRead` | Learner packages and hour adjustments | View | `GET /api/v1/admin/packages` |
| 603 | `PackagesUpdate` | Learner packages and hour adjustments | Update | `POST /api/v1/admin/packages/{id}/adjust` (تعديل الساعات — حساس ماليًا) |
| 701 | `PackageTiersRead` | Package tiers | View | `GET /api/v1/admin/packages/tiers` |
| 702 | `PackageTiersAdd` | Package tiers | Add | `POST /api/v1/admin/packages/tiers` |
| 703 | `PackageTiersUpdate` | Package tiers | Update | `PUT /api/v1/admin/packages/tiers/{id}` |
| 704 | `PackageTiersDelete` | Package tiers | Delete | `DELETE /api/v1/admin/packages/tiers/{id}` |
| 801 | `SessionsRead` | Platform sessions | View | `GET /api/v1/admin/sessions`, `GET /api/v1/admin/sessions/{id}` |
| 803 | `SessionsUpdate` | Platform sessions | Update | `POST /api/v1/admin/sessions/{id}/cancel` (إلغاء إداري + refund — حساس ماليًا) |
| 901 | `TeachersRead` | Teachers | View | `GET /api/v1/admin/teachers` |
| 903 | `TeachersUpdate` | Teachers | Update | `POST /api/v1/admin/teachers/{id}/toggle-status` |
| 1001 | `InstructorApplicationsRead` | Instructor applications | View | `GET /api/v1/instructor-applications`, `GET /api/v1/instructor-applications/{id}` |
| 1003 | `InstructorApplicationsUpdate` | Instructor applications | Update | `POST /api/v1/instructor-applications/approve`, `POST /api/v1/instructor-applications/reject` (الـ id جوه الـ body) |
| 1103 | `InstructorPricingUpdate` | Instructor pricing | Update | `PUT /api/v1/instructor-applications/{id}/pricing` (سعر الساعة والعمولة — حساس ماليًا) |
| 1201 | `InstructorAvailabilityRead` | Teacher availability | View | `GET /api/v1/instructor-availability/pagginated` |
| 1202 | `InstructorAvailabilityAdd` | Teacher availability | Add | `POST /api/v1/instructor-availability/instructors/{instructorId}/weekly-slots` |
| 1203 | `InstructorAvailabilityUpdate` | Teacher availability | Update | `PUT /api/v1/instructor-availability/weekly-slots/{id}` |
| 1204 | `InstructorAvailabilityDelete` | Teacher availability | Delete | `DELETE /api/v1/instructor-availability/weekly-slots/{id}` |
| 1301 | `CountriesRead` | Countries | View | `GET /api/v1/countries/pagginated` |
| 1302 | `CountriesAdd` | Countries | Add | `POST /api/v1/countries` |
| 1303 | `CountriesUpdate` | Countries | Update | `PUT /api/v1/countries/update` |
| 1304 | `CountriesDelete` | Countries | Delete | `DELETE /api/v1/countries/{id}` |
| 1401 | `FinancesRead` | Financial overview | View | `GET /api/v1/admin/finances/overview` **و** `GET /api/v1/admin/teachers/{id}/wallet` |

## الـ endpoints الجديدة (3)

### 1) `GET /api/v1/admin/permissions`

الكتالوج كامل + صلاحيات الحساب اللي بطلب. ده اللي الفرونت يقرأ منه عشان يرسم القوائم والـ checkboxes.

```json
{
  "success": true, "isSuccess": true, "statusCode": 200, "message": "Permission catalog retrieved successfully.",
  "errors": {},
  "data": {
    "isSuperAdmin": false,
    "myPermissions": [301, 303, 801],
    "features": [
      {
        "feature": "Courses",
        "featureName": "Courses, lessons and quizzes",
        "permissions": [
          { "code": 301, "name": "CoursesRead", "action": "View" },
          { "code": 302, "name": "CoursesAdd", "action": "Add" },
          { "code": 303, "name": "CoursesUpdate", "action": "Update" },
          { "code": 304, "name": "CoursesDelete", "action": "Delete" }
        ]
      }
    ]
  }
}
```

`isSuperAdmin: true` معناها `myPermissions` = كل الأكواد الـ 40 (حتى لو مفيش أي سطر في الجدول).
الفيتشرات عدد أبنائها مش ثابت: منه 4 (CRUD كامل)، منه 2، و `InstructorPricing` و `Finances` كود واحد بس — ارسم القوائم من الرد مش من فرض مسبق.

### 2) `GET /api/v1/admin/users/{id}/permissions`

```json
{ "data": { "userId": "…", "isSuperAdmin": false, "permissions": [301, 302, 801] } }
```

لو المستخدم ده Admin أصلاً: `isSuperAdmin: true` و `permissions` = كل الأكواد.

### 3) `PUT /api/v1/admin/users/{id}/permissions`

Body = الاستبدال الكامل (مش إضافة):

```json
{ "permissions": [301, 302, 801] }
```

- `[]` فاضية = سحب كل الصلاحيات (مش خطأ).
- الرد نفس شكل الـ GET.
- الأكواد دي بتتعملها **replace** مش patch — عشان كده مفيش `PermissionsAdd` / `PermissionsDelete`؛ PUT واحد بيسوي كل حاجة (بيمسح اللي مش في القائمة وبيضيف اللي مش موجودة).

## خطوات عمل Sub Admin من الأدمن بورد

1. `POST /api/v1/users` بـ `role: 4` و **`isActive: true`** (الحساب المتقفل بيطلع 403 في اللوجان زي أي حساب مقفول). لسه مفيش حاجة جديدة في الـ payload غير قيمة الدور.
2. `PUT /api/v1/admin/users/{id}/permissions` بالأكواد المطلوبة — الحساب لازم يكون ياخد دور `SubAdmin` في الخطوة 1 عشان القنن ده عدّي.
3. الحساب ده **لازم يعمل login** (أو refresh) بعد الخطوة 2 عشان الأكواد تدخل التوكن.

ولو عايز تحوّل حساب موجود لأدمن فرعي: `PUT /api/v1/users/update` بـ `role: 4` (بيشيل أي دور تاني منه)، وبعدين الـ PUT بتاع الصلاحيات.

> الادمن الرئيسي (super admin) يقدر يعمل الخطوتين دول كلها عادي. اللي اتقفل هو اللي بعده.

## قواعد منع التصعيد (جديدة)

المنطق: اللي معاه `UsersAdd` / `UsersUpdate` مش لازم يقدر يطلع أدمن كامل أو يلمس حسابه. كل القواعد دي شغالة في الـ **validators** فبتترجم لـ **400** (مش 403) ورسالتها في `errors`:

| الـ request | الشرط | الرسالة |
|---|---|---|
| `POST /api/v1/users` بـ `role: 2` | الطالب مش super admin | `Only a super admin can assign the Admin role.` |
| `PUT /api/v1/users/update` بـ `role: 2` | الطالب مش super admin | `Only a super admin can assign the Admin role.` |
| `PUT /api/v1/users/update` على حساب دوره `Admin` | الطالب مش super admin | `Only a super admin can manage an account that holds the Admin role.` |
| `DELETE /api/v1/users/{id}` على حساب دوره `Admin` | الطالب مش super admin | `Only a super admin can manage an account that holds the Admin role.` |
| `POST /api/v1/users/{id}/change-password` لحساب دوره `Admin` | الطالب مش super admin | `Only a super admin can manage an account that holds the Admin role.` |
| `PUT /api/v1/admin/users/{id}/permissions` وحساب الهدف دوره `Learner`/`Instructor` | — | `Permission codes can only be assigned to SubAdmin accounts. This account has the following role(s): {roles}.` |
| `POST /api/v1/authentication/register` بـ `role: 2` أو `role: 4` | دايمًا | `The selected role is invalid.` (التسجيل مسموح بـ 0 و 1 بس) |

- "super admin" هنا معناه الـ bit `2` موجود في `UserTypes` بتاع التوكن — يعني الحساب اللي دوره `Admin` في `AspNetRoles`. حساب `SubAdmin` (`4`) **مش** super admin، حتى لو معاه 39 كود من الـ 40.
- `role: 4` (SubAdmin) مسموح لأي حد معاه `UsersAdd` أو `UsersUpdate` — وده آمن لأن الدور لوحده مابيديش access، ومنح الأكواد محصور في اللي بيملكها.
- الفحص بيقرا الدور من `AspNetUserRoles` + `AspNetRoles.Name = 'Admin'` لحظة الطلب، مش من التوكن — فمفيش طريقة تلفّق الحساب بدور مؤقت في التوكن القديم.

## الفخاخ

- **أي تغيير في الصلاحيات بيطير الجلسة.** بنرفع `TokenVersion` وبنعمل revoke لكل الـ refresh tokens للحساب ده. ده مقصود: الصلاحيات متخزنة في التوكن فمفيش طريقة تسحب صلاحية من توكن شغال. لو الفرونت عمله PUT لنفسه، هيتسجل خروج.
- **التوكنات القديمة مفيهاش `perm`.** أي حساب مش Admin super ومفتوح دلوقتي، أول request بعد الديبلوي هيرجع 403 لحد ما يسجل دخول من جديد. حساب الـ Admin الرئيسي مش متأثر (بيعدي ضمنيًا).
- **الترقيم اتغيّر في النسخة دي.** `x02` كان معناه "manage" كله، دلوقتي بقى "Add" بس. أي قائمة أكواد محفوظة عند الفرونت أو في سكرابت لازم تتبني من `GET /api/v1/admin/permissions` مش من ثابت مكتوب.
- **`401` مش `403`:** من غير توكن → 401 (`You are not authorized to perform this action.`). بتوكن من غير الصلاحية → 403 والرسالة بيسمّي الفيتشر والأكشن الناقص بالظبط: `You do not have permission to access this feature. Required permission: Courses, lessons and quizzes (Update).` — الاعتماد على الـ `statusCode` أحسن من مطابقة النص.
- **`Add` مش بيدي `Update`:** الفرق مادي في شوية endpoints بتشتغل POST وهي في الأصل تعديل، فالاتفاق إنها اتحسبت Update: `POST /api/v1/admin/packages/{id}/adjust`، `POST /api/v1/admin/sessions/{id}/cancel`، `POST /api/v1/admin/teachers/{id}/toggle-status`، `POST /api/v1/instructor-applications/approve|reject`، `POST /api/v1/users/{id}/change-password`، `POST /api/v1/courses/admin/lessons/{id}/session`. متقيسش على الـ verb.
- **`read` مش بيدي أي كتابة:** اللي معاه 301 بس هيقدروا يشوفوا الكورسات ويعملوا 403 على أي Add/Update/Delete.
- **بلاغات الـ availability بتتفحص في الاتنين:** الفلتر على الـ endpoint + جوه الـ handler/validator (عشان الأدمن بيكتب بالنيابة عن المدرس)، فالصلاحية لازم تكون مطلوبة في المستويين بنفس الكود.
- **`[AllowAnonymous]` بيتخطى الفحص:** زي `GET /api/v1/countries` و `GET /api/v1/countries/{id}` — دول لسه مفتوحين للزيار من غير توكن.
- **`PermissionsUpdate` (203) عمليًا للأدمن الرئيسي بس:** أي حساب مش super ما يقدرش يمنح كود هو نفسه مش ممتله، فمفيش escalation ذاتي.
- **دور `SubAdmin` لوحده مابيديش حاجة:** حساب بـ `role: 4` ومن غير أي كود في الـ grants هيرجع 403 على الأدمن بورد كله — ده متوقع مش باگ. اللي بيفرق هو الأكواد.
- **تغيير الدور من `SubAdmin` لغيره بيمسح الأكواد:** الـ grants بتاعته بتتحذف في نفس الـ request (ومعاه بتطير جلسته). لو الفرونت حط "Edit role" في فورم الصلاحيات، المستخدم ده يفقد كل الأكواد مش بس مايشوفهمش.
- **`UserTypes` في التوكن بقى ممكن يكون `4` أو `5`:** أي منطق في الفرونت بيعمل مقارنة بالتساوي (`userTypes === 1` أو `=== 2`) هيفشل على الأدمن الفرعي — الفحص الصح هو الـ bits، أو الأحسن يعتمد على `GET /api/v1/admin/permissions` و `myPermissions`.
- **متحسبش إن الدور بيتخزن رقم في الداتابيز:** الأدور أسماء في `AspNetRoles`، والرقم `4` موجود في الـ enum والـ payload بس.

## قبل ما الشغل ده يشتغل on-line

1. **طبّق الـ migration:** `20261111104021_AddAdminPermissionGrants` — جدول جديد `AdminPermissionGrants` وبس، مفيش أي تعديل على جدول موجود. عمود الـ `Permission` رقمي، فالفصل لـ Add/Update/Delete **مفيش وراه أي migration جديدة**.
2. **مفيش migration جديدة للدور:** `UserType` مش متخزن في أي عمود (الأدور في `AspNetRoles` بالاسم)، فالإضافة دي كود وبس. سطر `SubAdmin` في `AspNetRoles` بيتعمل أوتوماتيك أول مرة حد ياخده من `AddUser`/`UpdateUser` — مش محتاج تشغّل الـ seeder على السيرفر. الـ **access** لسه الجدول هو مصدر الحقيقة فيه (الدور بوابة القنن بس)، فلو سطر الـ grants اتسحب الحساب فقد كل حاجة حتى لو الدور لسه موجود.
3. مستند الـ OpenAPI و مجموعة Postman اتولدوا من النسخة المنشورة، فلازم يتعملهم sync بعد الديبلوي:
   `.\scripts\Update-OpenApi.ps1 -BaseUrl https://lms-api.runasp.net` وبعدها `.\scripts\Update-PostmanCollection.ps1`.
   اللي هيتغير في الـ sync: سكيمة `UserType` بقى ليها 4 قيم (`SubAdmin = 4`)، ووصف الـ admin controllers بقى `Admin · SubAdmin (per granted permission code)`، و 400 جديدة في الـ user endpoints.
