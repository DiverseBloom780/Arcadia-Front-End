# Changelog

All notable changes to Arcadia will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features

- Cloud save synchronization
- Achievement tracking system
- Social features (friends, game recommendations)
- Mobile companion app
- Linux and macOS support via Avalonia UI

## [1.0.0] - 2025-10-05

### Added
- Initial release of Arcadia Frontend Launcher
- Game Wheel UI with vertical, horizontal, and curved scrolling options
- Direct emulator launching without AutoHotkey scripts
- Steam integration with automatic game detection
- GOG integration with automatic game detection
- Epic Games integration with automatic game detection
- TeknoParrot support with automatic GameProfile generation
- Device detection for steering wheels, lightguns, joysticks, and gamepads
- Automatic media downloading (box art, logos, cart art, video previews)
- GitHub-based auto-update system with changelog display
- Smart Wizard AI assistant for setup and troubleshooting
- Dynamic playlists and smart collections
- Intelligent search with advanced filtering
- Game preview mode with attract feature
- Plugin ecosystem with official SDK
- Smart multiplayer helper
- Dark theme UI
- SQLite-based game database
- Direct3D11 rendering via SharpDX
- Comprehensive configuration system
- Build and deployment scripts

### Core Modules
- **Arcadia.Core**: Game models, database, and launcher
- **Arcadia.UI**: WPF interface with DirectX rendering
- **Arcadia.Emulators**: Emulator integration and management
- **Arcadia.Launchers**: PC launcher integrations (Steam, GOG, Epic, TeknoParrot)
- **Arcadia.Media**: Media scraping and management
- **Arcadia.SmartWizard**: AI assistant for setup and troubleshooting
- **Arcadia.Updater**: GitHub-based auto-update system

### Technical Details
- Built on .NET 8.0 for Windows
- WPF for UI framework
- SharpDX for Direct3D11 rendering
- SQLite for game library management
- Octokit for GitHub integration
- Newtonsoft.Json for JSON parsing

### Documentation
- Comprehensive README with setup instructions
- Architecture documentation
- Configuration guide
- Plugin SDK documentation
- Troubleshooting guide

## [0.9.0] - 2025-09-15 (Beta)

### Added
- Beta release for testing
- Core game wheel functionality
- Basic Steam and GOG integration
- Initial TeknoParrot support

### Known Issues
- Media downloading may be slow on first launch
- Some TeknoParrot profiles may require manual adjustment
- Video previews not yet optimized for all formats

## [0.5.0] - 2025-08-01 (Alpha)

### Added
- Alpha release for internal testing
- Basic UI framework
- Game database implementation
- Initial launcher integration

---

For more information, visit the [Arcadia GitHub repository](https://github.com/yourusername/Arcadia).
