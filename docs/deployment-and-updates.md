# Deployment & Update Plan

## 1) Manual Distribution (Current)
1. Publish a self-contained win-x64 build.
2. Package with Inno Setup installer.
3. Upload installer/zip to GitHub Releases.
4. Share one public download link with users.

## 2) Automatic Updates from GitHub Releases
- App now includes a GitHub release update checker (`UpdateService`).
- On startup, the app checks the latest release tag.
- If newer version exists, user can download and run installer automatically.
- Users can also manually trigger check from **About > فحص التحديثات**.

### Release format recommendation
- Use semantic tags: `v1.0.7`, `v1.0.8`, ...
- Attach `.exe` or `.msi` installer in release assets.
- Add release notes (what changed / fixed).

## 3) User Feedback Channel
- In-app actions in About page:
  - Report bug
  - Feature request
  - Rate app / ideas
- GitHub issue templates are included under `.github/ISSUE_TEMPLATE`.
- Suggest enabling GitHub Discussions for long-form feedback.

## Local Commands
```powershell
# Build and package zip
pwsh .\scripts\publish-release.ps1 -Version 1.0.7

# Build installer with Inno Setup (after editing version in Wahee.iss)
# Run in Inno Setup Compiler GUI: installer\Wahee.iss
```
