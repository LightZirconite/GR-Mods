# Changelog

All notable changes to GR Mods will be documented in this file.

## [1.1.0] - 2025-11-02

### 🎉 Major Features Added

#### Performance & User Experience
- **Real-time Progress Bar**: Shows percentage, transfer speed (MB/s), and estimated time remaining
- **Disk Space Check**: Automatically verifies available space before transfer (with 10% safety margin)
- **Integrity Verification**: Validates essential game files after transfer to ensure nothing is missing

#### Safety & Reliability
- **Launcher Detection**: Checks if Steam, Epic Games Launcher, or Rockstar Games Launcher are running before transfer
- **Mod Detection**: Detects and warns about installed mods (ScriptHookV, OpenIV, mods folder, etc.)
- **Cancellation Support**: Infrastructure added for canceling transfers (CancellationToken)

#### Interface Improvements
- **Integrated Log Viewer**: Dedicated window to view logs with actions:
  - 🔄 Refresh logs
  - 🗑️ Clear all logs
  - 📋 Copy to clipboard
  - 📂 Open log file
- **Windows Notifications**: Toast notification and window flash when transfer completes
- **Mods Warning**: Displays detected mods with warning about platform compatibility

#### Updates & Maintenance
- **Auto-Update System**: Checks GitHub Releases for new versions at startup
- **Version Checking**: Compares current version with latest release
- **Direct Download Link**: Opens GitHub releases page to download updates

### 🔧 Technical Improvements

- Refactored `CopyDirectory` to `CopyDirectoryWithProgress` with IProgress<T> support
- Added `ProgressInfo` class for detailed transfer statistics
- Implemented `UpdateChecker` service for GitHub API integration
- Created `NotificationService` for Windows toast notifications
- Added `LogViewerWindow` as separate WPF window
- Updated .csproj to version 1.1.0 with System.Text.Json package

### 📚 Documentation

- Updated README.md with all new features
- Added comprehensive usage instructions
- Documented new log viewer functionality
- Created CHANGELOG.md

### 🐛 Bug Fixes

- Fixed UI blocking during file transfers with proper async/await pattern
- Improved error handling with specific exception messages
- Better rollback mechanism if transfer fails

---

## [1.0.0] - 2025-10-XX

### Initial Release

#### Core Features
- Automatic GTA V detection across all platforms
- Dynamic Steam library detection via Windows Registry
- Support for moving game between different drives
- Multiple installation detection with warnings
- Rollback system on errors
- Administrator rights requirement
- Modern WPF interface with platform logos
- Detailed logging system

#### Supported Platforms
- Steam
- Rockstar Games Launcher
- Epic Games Store

#### Build System
- Automated build scripts (PowerShell)
- Professional installer with Inno Setup
- Debug and Release build configurations
