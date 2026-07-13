# RentConnect — البنية الكاملة (Repository + Unit of Work)

```
RentConnect.sln
src/
  RentConnect.Data/
    Models/          ← internal (Listing, ListingImage, ListingStatusHistory) - غير مرئية خارج المشروع
    Data/             ← internal (AppDbContext) - غير مرئي خارج المشروع
    Dtos/             ← public (ListingDto, ListingCreateDto, ListingUpdateDto...) - هذا فقط ما يُشارك
    Repositories/     ← IListingRepository (public) + ListingRepository (internal)
    UnitOfWork/       ← IUnitOfWork (public) + UnitOfWork (internal)
    Extensions/       ← AddRentConnectData() + EnsureDatabaseCreated()
  RentConnect.Api/     ← يحقن IUnitOfWork فقط
  RentConnect.Web/     ← يحقن IUnitOfWork فقط (Blazor Server)
```

**القاعدة الصارمة المطبّقة هنا:** `AppDbContext` وكيانات EF (`Listing`, `ListingImage`,
`ListingStatusHistory`) صارت **`internal`** — يعني حرفياً، لو حاولت تستخدمهم من مشروع
API أو Web، ما رح يعمل compile أساساً (الكومبايلر برفضهم لأنهم غير مرئيين من خارج
مشروع `RentConnect.Data`). هذا مش مجرد اتفاق أو تعليمات بالتوثيق — إنه مفروض إجبارياً
على مستوى الكود.

المسموح استخدامه من برا فقط:
- `IUnitOfWork` (بيحقن بمشروع API ومشروع Web بنفس الطريقة)
- `IListingRepository` (بتوصله عبر `unitOfWork.Listings`)
- DTOs من `RentConnect.Data.Dtos` (هذا الشكل الوحيد للبيانات اللي بيشوفه أي مشروع تاني)
- `ListingStatus` enum (قيمة مشتركة بسيطة، مش كيان قاعدة بيانات)

**كيف يشتغل نمط Unit of Work هون:**
عمليات المستودع (`AddAsync`, `UpdateAsync`, `DeleteAsync`, `UpdateStatusAsync`) تجهّز
التغيير بالذاكرة فقط (عبر EF Change Tracker) - **لا تُحفظ فعلياً** إلا بعد استدعاء
`unitOfWork.CompleteAsync()` صراحة من الكنترولر أو المكوّن:

```csharp
var created = await _unitOfWork.Listings.AddAsync(dto);
await _unitOfWork.CompleteAsync(); // هون فقط بيصير الحفظ الفعلي بقاعدة البيانات
```

---

## تشغيل مشروع Blazor (الواجهة كاملة الوظائف)

```bash
cd src/RentConnect.Web
dotnet restore
dotnet run
```

**تشغيل الـ API بشكل منفصل:**
```bash
cd src/RentConnect.Api
dotnet run
```

⚠️ كل مشروع (API و Web) عنده ملف SQLite منفصل بمجلده الخاص. لو حابب يشتركوا بنفس
قاعدة البيانات، وحّد مسار `ConnectionStrings:Sqlite` بكل الـ appsettings.json لمسار
مطلق واحد.

---

## ميزة اختيار الموقع بالخريطة

مكوّن `LocationPicker.razor` (بمجلد `RentConnect.Web/Components/Shared/`) — حقل نصي
للمنطقة + خريطة Leaflet.js تفاعلية حقيقية. الضغط على أي نقطة يسجّل خط الطول والعرض
تلقائياً بالإعلان عبر JS Interop (`wwwroot/js/locationPicker.js`). يحتاج اتصال إنترنت
عادي وقت التشغيل لتحميل Leaflet من CDN.

---

## الانتقال إلى SQL Server

عدّل بكل appsettings.json (Api و Web):
```json
"DatabaseProvider": "SqlServer",
"ConnectionStrings": { "SqlServer": "Server=...;Database=RentConnect;..." }
```

---

## Endpoints الحالية بالـ API

| Method | Route | الوصف |
|---|---|---|
| GET | /api/listings | كل الإعلانات (فلترة بـ region, status) |
| GET | /api/listings/{id} | تفاصيل إعلان واحد |
| POST | /api/listings | إضافة إعلان جديد |
| PUT | /api/listings/{id} | تعديل إعلان |
| PATCH | /api/listings/{id}/status | تحديث الحالة (متوفر/متأجّر/معلّق) |
| DELETE | /api/listings/{id} | حذف إعلان |
| GET | /api/listings/stale?days=7 | الإعلانات المتأخرة عن التحديث |

## نقاط مهمة

- الانتقال الحالي يستخدم `EnsureCreated()` — مناسب لمرحلة MVP. لاحقاً لما تصير عندك
  تعديلات متكررة على شكل الجداول، انتقل لـ EF Core Migrations
  (`dotnet ef migrations add` من داخل مشروع `RentConnect.Data`، مع تحديد
  `--startup-project ../RentConnect.Api`). كون الكيانات `internal` ما بيأثر على عمل
  أدوات EF Core (بتستخدم Reflection وليس مرجعية compile-time مباشرة).

