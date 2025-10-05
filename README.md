
# Arcadia Frontend Launcher – Project Summary

## Overview
**Arcadia** is a modern, high-performance frontend launcher designed to replace and improve upon the Arcade Scene. Built in C# with WPF and DirectX, Arcadia offers a visually stunning, highly customizable interface for managing and launching games from emulators, Steam, GOG, Epic Games, and TeknoParrot.

---

## Key Differentiators

- **Direct Launching Architecture**:  
  Arcadia launches emulators and games directly—no need for intermediary scripts (like AHK or RocketLauncher)—for faster, more reliable execution.

- **Modern PC Launcher Integration**:  
  Native support for Steam, GOG, and Epic; auto-detects installed games via registry and manifest parsing; launches games seamlessly using platform-specific URIs.

- **Advanced TeknoParrot Support**:  
  Auto-scans ROMs, generates valid GameProfile XMLs, auto-configures input devices, and supports a wide range of controllers with automatic detection and calibration.

- **AI-Powered Smart Wizard**:  
  Local AI assistant provides context-aware help, troubleshooting, and setup guidance—automating complex configuration and repair tasks.

- **Automatic Update System**:  
  GitHub-based updater with changelog display and one-click, non-disruptive updates that preserve user settings.

---

## Technical Architecture

- **Technology Stack**:  
  - .NET 8.0 (C#)
  - WPF (UI with custom controls/themes)
  - SharpDX for Direct3D11 rendering
  - SQLite for the embedded database
  - Octokit for GitHub integration

- **Modular Design**:
  - **Arcadia.Core**: Data models, DB services, launching logic
  - **Arcadia.UI**: WPF interface, DirectX rendering, controls
  - **Arcadia.Launchers**: Integration with Steam, GOG, Epic, TeknoParrot
  - **Arcadia.Emulators**: Emulator config/launching
  - **Arcadia.Media**: Media management, AI generation
  - **Arcadia.SmartWizard**: AI assistant for setup/troubleshooting
  - **Arcadia.Updater**: Automatic updates via GitHub

- **Data Flow**:
  - On startup: DB loads games → UI renders game wheel → User navigates/selects → Details & media load (cached or downloaded) → Launch command built/executed → Play stats updated.

---

## Core Features

- **Game Wheel UI**:  
  Custom WPF canvas with smooth, animated circular/linear arrangements and rich visual effects.

- **Direct Emulator Launching**:  
  Launch via ProcessStartInfo with variable substitution and multi-profile support; BIOS validation included.

- **PC Launcher Integration**:  
  - **Steam**: Detect via libraryfolders.vdf & appmanifest files; launch via steam:// URIs.
  - **GOG**: Detect via registry; launch via goggalaxy:// or direct.
  - **Epic**: Detect via manifest JSONs; launch via epicgames:// URIs.

- **TeknoParrot Integration**:  
  ROM scanning, game type detection, XML profile generation, device detection/configuration, and XML validation.

- **Media Management**:  
  Background service for media scraping (TheGamesDB, ScreenScraper, Emumovies), caching, and AI-driven asset generation.

- **Smart Wizard**:  
  Command-line AI assistant with local knowledge base for automated fixes and guided setup.

- **Auto-Update System**:  
  Octokit queries GitHub for new releases, displays changelog, and handles update installation while preserving user data.

---

## Unique Features

- **Dynamic Playlists & Smart Collections**:  
  Auto-generate and update playlists by genre, publisher, decade, completion status, and custom tags.

- **Instant Media Generation**:  
  AI-powered creation of missing box art, logos, fan art, and gameplay videos.

- **Intelligent Search & Filtering**:  
  Fast, indexed full-text search and advanced filters (platform, genre, player count, etc.).

- **Smart Save/State Manager**:  
  Unified management of emulator save files, instant resume, backups, and planned cloud sync.

- **Game Preview Mode**:  
  Auto-play gameplay videos, attract mode cycling, and customizable delays for arcade cabinets.

- **Plugin Ecosystem**:  
  .NET plugin architecture (IPlugin) with official SDK; supports custom themes, media scrapers, and new commands.

- **Smart Multiplayer Helper**:  
  Auto-detects multiplayer games/controllers and pre-configures input mappings, with recommendations based on connected devices.

---

## Development Status & Next Steps

- **Current Implementation**:  
  - All core modules functional: game detection, launching, and UI.
  - PC launcher and TeknoParrot integration complete.
  - Database and update system operational.

- **Planned Features**:  
  - Expand Smart Wizard with NLP.
  - Integrate AI-driven media generation models/APIs.
  - Release Plugin SDK & documentation.
  - More emulator/VR/cloud save/achievement support.

- **Testing & Optimization**:  
  - Performance profiling (rendering, queries, media loading)
  - Compatibility/user acceptance testing.

---

## Deployment & Distribution

- **Build Process**:  
  Automated PowerShell script restores packages, builds, publishes, and packages the app for distribution.

- **Installation**:  
  Users download and extract from GitHub Releases; config and DB initialized on first launch.

- **Updates**:  
  Notified in-app; new version downloaded, installed, and data preserved.

---

## Conclusion

Arcadia advances frontend launcher technology with direct game launching, seamless modern PC integration, AI-driven features, and a modular extensible architecture. Its robust TeknoParrot support, automatic media management, and intelligent search make it uniquely powerful for gaming enthusiasts and arcade builders.

**The project is ready for initial release with all core features implemented. Future development will focus on AI features, plugin ecosystem expansion, and advanced user tools.**