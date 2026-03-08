# Wahee - Islamic WPF Desktop App

Wahee is a modern Islamic desktop application built with **.NET 8**, **WPF**, **EF Core**, and **SQLite**.
It focuses on daily spiritual productivity with prayer times, Quran reading, azkar, and lightweight desktop widgets.

## Highlights
- Prayer times with countdown and desktop widget
- Quran browser and Mushaf reading experience
- Random ayah widget with tafsir and audio playback
- Azkar and inspirational content
- Live Islamic radio streams
- Persistent app settings and widget state

## Tech Stack
- **UI:** WPF (.NET 8)
- **Architecture:** Clean Architecture style (Core / Infrastructure / UI)
- **Data:** Entity Framework Core + SQLite
- **DI:** Microsoft.Extensions.DependencyInjection
- **HTTP:** IHttpClientFactory-based services

## Project Structure
- `Wahee.Core`:
  Domain models, interfaces, shared constants, base view models/commands
- `Wahee.Infrastructure`:
  EF Core DbContext, repositories, external API services, seeding
- `Wahee.UI`:
  WPF views, widgets, MVVM view models, app startup and navigation

## Recent Updates
- Implemented MVVM view models for key screens
- Fixed prayer countdown parsing and dictionary safety issues
- Added secure HTTPS API usage where available
- Added AppData-based SQLite path for safer deployment
- Improved startup initialization sequence to avoid race/locking
- Refined widgets lifecycle handling and reuse safety
- Upgraded prayer widget to compact horizontal layout + pin (always-on-top)
- Reworked random ayah widget layout, tafsir visibility behavior, and pin support
- Added manual location and seasonal time mode settings (Auto/Winter/Summer)
- Fixed Mushaf surah jump behavior and added last-read-position restore

## Running Locally
### Requirements
- .NET SDK 8.0+
- Windows (WPF target)

### Build
```powershell
dotnet build .\Wahee.sln
```

### Run
```powershell
dotnet run --project .\Wahee.UI\Wahee.UI.csproj
```

## Configuration Notes
- SQLite DB path is created under `%AppData%\Wahee\wahee.db`
- External APIs used for prayer/location/radio/tafsir are network dependent
- If network is unavailable, fallback behavior is applied in key services

## Contributing
1. Create a feature branch
2. Keep commits focused and descriptive
3. Run build before opening PR
4. Add/adjust tests when introducing new logic

## License
This repository is currently maintained by the project owner. Add your preferred license file (`LICENSE`) if you plan to open-source formally.
