# Arcadia Frontend Launcher

**Arcadia** is a comprehensive, high-performance frontend launcher for games and emulators, inspired by Hyperspin Attraction. It provides a visually stunning, customizable interface with direct emulator launching, PC game integration (Steam, GOG, Epic Games), TeknoParrot support, and unique AI-driven features.

## Features

### Core Features

- **Game Wheel UI**: Classic Hyperspin-style wheel with vertical, horizontal, and angled/curved scrolling options
- **Direct Emulator Launching**: Launch standalone emulators directly without AutoHotkey scripts
- **PC Launcher Integration**: Automatic detection and launching of Steam, GOG, and Epic Games titles
- **TeknoParrot Support**: Automatic ROM scanning, GameProfile generation, and intelligent device configuration
- **Media Management**: Automatic downloading of artwork, logos, cart art, themes, and video previews
- **Auto-Update System**: GitHub integration with changelog display for seamless updates

### Unique Enhancements

- **Smart Wizard (AI Assistant)**: Local text-based command interface for setup, configuration, and troubleshooting
- **Dynamic Playlists**: Auto-generated smart collections based on genre, publisher, decade, and completion status
- **Instant Media Generation**: AI-driven generation of missing box art, logos, and preview videos with style filters
- **Intelligent Search**: Fast, unified search across all systems with advanced filtering
- **Smart Save/State Manager**: Unified interface for save states across emulators with auto-backup
- **Game Preview Mode**: Hover-based gameplay previews and arcade-style attract mode
- **Plugin Ecosystem**: Official SDK for custom themes, shaders, and import/export tools
- **Smart Multiplayer Helper**: Automatic detection and configuration for multiplayer games

## System Architecture

Arcadia is built as a modular C# WPF application with the following components:

```
Arcadia/
├── Source/
│   ├── Arcadia.Core/           # Core models, database, and game launcher
│   ├── Arcadia.UI/             # WPF UI with DirectX rendering
│   ├── Arcadia.Emulators/      # Emulator integration and management
│   ├── Arcadia.Launchers/      # Steam, GOG, Epic Games, TeknoParrot integration
│   ├── Arcadia.Media/          # Media scraping and management
│   ├── Arcadia.SmartWizard/    # AI assistant for setup and troubleshooting
│   └── Arcadia.Updater/        # GitHub-based auto-update system
├── Assets/                     # Media assets, themes, and resources
├── Config/                     # Configuration files
├── Docs/                       # Documentation
└── Scripts/                    # Build and deployment scripts
```

## Technology Stack

- **Framework**: .NET 8.0 (Windows)
- **UI**: WPF (Windows Presentation Foundation)
- **Rendering**: Direct3D11 via SharpDX
- **Database**: SQLite for game library management
- **Updates**: Octokit for GitHub integration
- **Language**: C# 12

## Installation

### Prerequisites

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime
- Visual Studio 2022 (for development)

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/Arcadia.git
   cd Arcadia
   ```

2. Open the solution in Visual Studio 2022:
   ```bash
   start Arcadia.sln
   ```

3. Restore NuGet packages:
   ```
   Tools > NuGet Package Manager > Restore NuGet Packages
   ```

4. Build the solution:
   ```
   Build > Build Solution (Ctrl+Shift+B)
   ```

5. Run the application:
   ```
   Debug > Start Debugging (F5)
   ```

## Configuration

### First Launch

On first launch, Arcadia will:

1. Create a game database in `%AppData%\Arcadia\games.db`
2. Scan for installed PC games (Steam, GOG, Epic Games)
3. Import detected games into the library
4. Display the main game wheel interface

### Adding Emulators

1. Press **F1** to open Settings
2. Navigate to **Emulators**
3. Click **Add Emulator** or use **Auto-Download** for supported emulators
4. Configure emulator paths and command-line templates
5. Add ROM directories for scanning

### TeknoParrot Configuration

1. Install TeknoParrot to a standard location (e.g., `C:\TeknoParrot`)
2. In Arcadia Settings, navigate to **TeknoParrot**
3. Set the ROMs folder path
4. Click **Scan ROMs** to automatically generate GameProfiles
5. Arcadia will detect connected hardware and configure input mappings

### GitHub Auto-Update Configuration

1. Open `Config/settings.json`
2. Set the GitHub repository details:
   ```json
   {
     "UpdateSettings": {
       "GitHubOwner": "yourusername",
       "GitHubRepository": "Arcadia",
       "CheckForUpdatesOnStartup": true
     }
   }
   ```
3. Arcadia will check for updates on startup and display a changelog

## Controls

| Key | Action |
|-----|--------|
| **↑/↓** or **W/S** | Navigate game wheel |
| **Enter** or **Space** | Launch selected game |
| **F1** | Open Settings |
| **F2** | Open Search |
| **F3** | Open Smart Wizard |
| **F5** | Refresh game library |
| **F11** | Toggle fullscreen |
| **Escape** | Exit Arcadia |

## PC Launcher Integration

### Steam

Arcadia automatically detects Steam games by:
- Reading the Steam installation path from the Windows Registry
- Parsing `libraryfolders.vdf` to find all Steam library locations
- Reading `appmanifest_*.acf` files to extract game information
- Launching games via `steam://rungameid/<appid>` URI

### GOG

Arcadia automatically detects GOG games by:
- Scanning the Windows Registry for installed GOG games
- Reading game metadata from registry keys
- Launching games via `goggalaxy://openGameView/<gameId>` URI or direct executable

### Epic Games

Arcadia automatically detects Epic Games by:
- Scanning manifest files in `%ProgramData%\Epic\EpicGamesLauncher\Data\Manifests`
- Parsing JSON manifests to extract game information
- Launching games via `com.epicgames.launcher://apps/<appName>?action=launch` URI

## TeknoParrot Integration

Arcadia provides comprehensive TeknoParrot support:

### Automatic GameProfile Generation

When you scan a ROMs folder, Arcadia:
1. Detects executable files
2. Determines the game type (Racing, Shooting, Fighting, Sports, Other)
3. Generates a valid `GameProfile.xml` following TeknoParrot's schema
4. Creates appropriate input configurations based on game type

### Device Detection and Configuration

Arcadia automatically detects and configures:
- **Steering Wheels**: Logitech, Thrustmaster, Fanatec (with force feedback)
- **Lightguns**: AimTrak, Sinden, Gun4IR (with calibration)
- **Joysticks**: Arcade sticks, flight sticks
- **Gamepads**: Xbox, DualShock 4, generic controllers

### Validation

Before launching a TeknoParrot game, Arcadia:
1. Validates the XML schema
2. Checks that all file paths exist
3. Verifies input bindings
4. Flags errors and suggests fixes via the Smart Wizard

## Smart Wizard

The Smart Wizard is a local AI assistant that helps with:

- **Setup**: Guide through initial configuration
- **Emulator Configuration**: Optimize emulator settings
- **File Repair**: Fix missing or corrupt files
- **Controller Mapping**: Automate controller configuration
- **TeknoParrot Troubleshooting**: Repair profiles and input mappings

To access the Smart Wizard, press **F3** or type commands in the integrated terminal.

## Media Management

Arcadia automatically downloads missing media assets:

1. **Artwork**: Box art, cart art, logos, fan art
2. **Themes**: Visual themes for systems and games
3. **Video Previews**: 30-second gameplay clips

Media is scraped from online databases and cached locally in `Assets/Media/`.

## Plugin Development

Arcadia supports a plugin ecosystem via the official SDK. Plugins can:

- Add custom themes and shaders
- Implement import/export tools
- Extend the Smart Wizard
- Add new media scrapers

See `Docs/PluginSDK.md` for development guidelines.

## Troubleshooting

### Games Not Detected

1. Ensure Steam/GOG/Epic Games is installed and games are properly installed
2. Press **F5** to refresh the game library
3. Manually add games via Settings > Games > Add Game

### Emulator Launch Fails

1. Verify emulator executable path in Settings
2. Check ROM file path and format
3. Consult the Smart Wizard (F3) for diagnostics

### TeknoParrot Games Not Working

1. Ensure TeknoParrot is installed and accessible
2. Validate GameProfiles via Settings > TeknoParrot > Validate
3. Use the Smart Wizard to repair configurations

## Roadmap


- [ ] Cloud save synchronization
- [ ] Achievement tracking
- [ ] Social features (friends, game recommendations)
- [ ] Mobile companion app
- [ ] Linux and macOS support (via Avalonia UI)

## Contributing

Contributions are welcome! Please read `CONTRIBUTING.md` for guidelines.

## License

Arcadia is licensed under the MIT License. See `LICENSE` for details.

## Acknowledgments

- Inspired by **Hyperspin** and **Attract-Mode**
- Built with **SharpDX** for Direct3D11 rendering
- Uses **Octokit** for GitHub integration
- Special thanks to the emulation and arcade communities

## Contact

For support, feature requests, or bug reports, please open an issue on GitHub or contact the development team.

---

**Arcadia** - The Ultimate Frontend Launcher for Games and Emulators
