# Arcadia Quick Start Guide

Welcome to **Arcadia**, the ultimate frontend launcher for your games and emulators. This guide will help you get started quickly and make the most of Arcadia's powerful features.

## Installation

Download the latest release of Arcadia from the GitHub releases page. Extract the ZIP file to a location of your choice, such as `C:\Arcadia`. Run `Arcadia.exe` to launch the application. On first launch, Arcadia will create its configuration files and scan for installed PC games.

## First Launch

When you first launch Arcadia, the application will automatically scan your system for installed games from Steam, GOG, and Epic Games. This process may take a few minutes depending on the size of your game library. Once the scan is complete, you will see your games displayed in the game wheel interface.

## Navigating the Interface

The Arcadia interface is designed for easy navigation with both keyboard and gamepad. Use the **Up** and **Down** arrow keys (or **W** and **S**) to scroll through your game library. Press **Enter** or **Space** to launch the selected game. Press **F1** to open the Settings menu where you can configure emulators, media, and other options. Press **F2** to open the Search function for quickly finding specific games. Press **Escape** to exit Arcadia.

## Adding Emulators

Arcadia supports direct launching of standalone emulators without the need for AutoHotkey scripts. To add an emulator, press **F1** to open Settings and navigate to the **Emulators** section. Click **Add Emulator** to manually configure an emulator, or use **Auto-Download** to automatically download and set up supported emulators. Specify the emulator executable path, supported platforms, and file extensions. Configure the command-line template for launching ROMs. Add ROM directories for Arcadia to scan.

### Supported Emulators

Arcadia supports a wide range of emulators including RetroArch, Dolphin for GameCube and Wii, PCSX2 for PlayStation 2, RPCS3 for PlayStation 3, Cemu for Wii U, Yuzu and Ryujinx for Nintendo Switch, MAME for arcade games, and many more. Each emulator can have multiple configuration profiles for different performance settings or input methods.

## TeknoParrot Setup

TeknoParrot is a powerful emulator for arcade games, and Arcadia provides comprehensive support for it. To set up TeknoParrot, first install TeknoParrot to a standard location such as `C:\TeknoParrot`. In Arcadia Settings, navigate to **TeknoParrot** and set the ROMs folder path. Click **Scan ROMs** to automatically detect games and generate GameProfiles. Arcadia will automatically detect connected hardware like steering wheels, lightguns, and joysticks, and configure input mappings based on the game type.

### Device Configuration

Arcadia automatically detects and configures the following devices. For racing games, steering wheels and pedals are mapped with optional force feedback support. For shooting games, lightguns such as AimTrak, Sinden, and Gun4IR are configured with calibration utilities. For fighting and arcade games, joysticks, arcade sticks, and gamepads are automatically mapped. You can manually adjust input configurations through the Smart Wizard or Settings menu.

## Media Management

Arcadia automatically downloads missing media assets for your games, including box art, logos, cart art, fan art, and video previews. This process runs in the background and does not interfere with your gameplay. You can monitor the progress in the Settings menu under **Media**. To manually refresh media for a specific game, select the game and press **F5**.

## Smart Wizard

The Smart Wizard is your AI-powered assistant for setup, configuration, and troubleshooting. Press **F3** to open the Smart Wizard terminal. You can ask questions like "How do I add a new emulator?", "Fix my TeknoParrot configuration", or "Download missing artwork for all games". The Smart Wizard runs locally and does not require an internet connection for basic assistance.

## Updates

Arcadia includes an automatic update system that checks for new releases on GitHub. When an update is available, you will see a notification with a changelog detailing new features and bug fixes. Click **Download Update** to automatically download and install the latest version. Arcadia will restart after the update is applied.

## Customization

Arcadia offers extensive customization options to tailor the experience to your preferences. In the Settings menu, you can change the wheel orientation between vertical, horizontal, and curved. Select different themes and visual styles. Enable or disable video backgrounds and gameplay previews. Adjust animation speed and transition effects. Configure input mappings for keyboard and gamepad.

## Tips and Tricks

To quickly search for a game, press **F2** and start typing the game name. Use the Smart Wizard (F3) to automate repetitive tasks like controller configuration. Enable the Attract Mode in Settings to automatically cycle through game previews when idle. Create custom playlists and collections to organize your library by genre, publisher, or completion status. Press **F11** to toggle fullscreen mode.

## Troubleshooting

If games are not detected, ensure that Steam, GOG, or Epic Games is properly installed and that games are fully downloaded. Press **F5** to manually refresh the game library. If an emulator fails to launch, verify the emulator executable path in Settings and check that the ROM file exists and is in a supported format. For TeknoParrot issues, use the Smart Wizard to validate and repair GameProfiles. If media is not downloading, check your internet connection and ensure that media scraping is enabled in Settings.

## Getting Help

For additional help and support, consult the full README and Developer Guide in the Docs folder. Visit the Arcadia GitHub repository for the latest updates and community discussions. Open an issue on GitHub if you encounter bugs or have feature requests. Join the Arcadia community forums to share tips and configurations with other users.

---

Enjoy your gaming experience with **Arcadia**!
