# Wahee - وحي

تطبيق إسلامي لسطح المكتب (Windows) مبني بـ WPF و .NET 8، يقدّم تجربة يومية تشمل مواقيت الصلاة، القرآن الكريم، الأذكار، وإذاعات القرآن.

## المميزات

- مواقيت الصلاة حسب الموقع (تحديد المدينة تلقائيًا عبر IP).
- تنبيه الأذان وتشغيل ملف صوتي عند دخول وقت الصلاة.
- القرآن الكريم:
- تصفح السور والآيات.
- البحث في النص القرآني مع تطبيع للحروف العربية.
- إذاعات القرآن الكريم (Live) من API موقع MP3Quran.
- صفحة الأذكار.
- Widgets لسطح المكتب (آية اليوم + مواقيت الصلاة).
- تشغيل التطبيق في الخلفية مع System Tray.

## التقنيات المستخدمة

- `.NET 8`
- `WPF` (واجهة سطح المكتب)
- `Entity Framework Core 8` + `SQLite`
- `Dependency Injection` عبر `Microsoft.Extensions.DependencyInjection`
- `HttpClient` للتكامل مع APIs خارجية

## هيكل المشروع

- `Wahee.UI`: واجهة التطبيق (WPF) والصفحات/الـ Widgets.
- `Wahee.Core`: النماذج (Models) والواجهات (Interfaces).
- `Wahee.Infrastructure`: الخدمات، قاعدة البيانات، والـ repositories.
- `Quran-Data-version-2.0`: بيانات القرآن المستخدمة داخل التطبيق.

## المتطلبات

- Windows 10/11
- .NET SDK 8.0
- Visual Studio 2022 (اختياري لكن مفضل) أو `dotnet CLI`

## التشغيل محليًا

```bash
dotnet restore
dotnet build Wahee.sln
dotnet run --project Wahee.UI
```

> عند أول تشغيل سيتم إنشاء قاعدة البيانات تلقائيًا وتهيئة البيانات الأساسية.

## النشر (Publish)

```bash
dotnet publish Wahee.UI -c Release -r win-x64 --self-contained true
```

## مصادر البيانات الخارجية

- مواقيت الصلاة: `api.aladhan.com`
- تحديد الموقع عبر IP: `ip-api.com`
- إذاعات القرآن: `mp3quran.net`

## ملاحظات

- ملف صوت الأذان الافتراضي: `1027.mp3` في جذر المشروع.
- بعض خصائص التطبيق تعتمد على اتصال الإنترنت (المواقيت/الإذاعات/تحديثات البيانات).

## المطور

- Mohamed Gamal
- GitHub: <https://github.com/MohamedGamal-Ahmed>
