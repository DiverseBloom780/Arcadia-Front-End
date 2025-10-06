# Research Summary: Frontend Launchers

This document summarizes the research conducted on Hyperspin Attraction and other existing frontend launchers, with a focus on identifying core functionalities, UI/UX elements, game launching mechanisms, and potential areas for innovation as outlined in the user's requirements for 'Arcadia'.

## Hyperspin Attraction Overview

Hyperspin is a highly customizable graphical user interface (GUI) primarily used for managing and launching arcade and console game collections. Key characteristics observed from the search results include:

*   **User-friendly Interface:** Emphasizes visual appeal with customizable themes and graphics.
*   **Game Wheel UI:** A prominent feature, allowing users to browse games via a rotating wheel. The user's request specifically mentions vertical, horizontal, and angled/curved scrolling options, along with smooth animations.
*   **Media Support:** Strong integration of various media types such as box art, cart art, logos, themes, and video previews (e.g., Emumovies).
*   **Emulator Support:** Designed to work with a wide range of emulators.
*   **Configuration Tools:** Provides tools for setting up and configuring systems and games.
*   **Community-driven:** Benefits from a strong community that contributes themes and content.
*   **Game Launching:** Traditionally, Hyperspin often relies on external tools like RocketLauncher and AutoHotkey (AHK) scripts for launching emulators and games. A key user requirement for Arcadia is to **avoid .ahk scripts** and directly launch emulators.

## Other Frontend Launchers

Research into other frontend launchers like Attract-Mode, LaunchBox/Big Box, and Playnite reveals common features and alternative approaches:

*   **Attract-Mode:** Described as an open-source, cross-platform frontend for command-line emulators. It focuses on hiding the underlying OS and providing an arcade-like experience. It's known for its ease of configuration and good looks.
*   **LaunchBox/Big Box:** A popular commercial frontend that offers extensive game management features, media scraping, and a visually rich interface (Big Box). It supports a wide array of emulators and PC games.
*   **Playnite:** An open-source, universal video game library manager that integrates games from various PC launchers (Steam, GOG, Epic Games, Xbox App, Itch.io) and emulators. It's highly praised for its plugin ecosystem, customization, and ability to consolidate diverse game libraries. Playnite's approach to integrating multiple PC launchers and emulated games is particularly relevant to the Arcadia project.

## Key Takeaways and Innovation Opportunities for Arcadia

Based on the user's detailed requirements and the research, Arcadia aims to combine the visual appeal and customizable UI of Hyperspin with the direct launching capabilities and broad integration of modern frontends like Playnite, while introducing unique AI-driven features.

**Core Requirements & Design Principles:**

1.  **Direct Emulator Launching:** A critical departure from Hyperspin's traditional AHK reliance. Arcadia will directly launch standalone emulators, and also feature auto-download and unpack functionality for emulators (user provides BIOS).
2.  **PC Launcher Integration:** Seamless detection and listing of games from Steam, Epic Games, and GOG, without bundling the launchers themselves or relying on cloud libraries.
3.  **TeknoParrot Support:** A major feature, including automatic scanning of ROMs, generation of `GameProfiles` (XML) and `GamePaths`, and intelligent device/control configuration based on connected hardware (steering wheels, lightguns, joysticks, gamepads). This requires strict adherence to TeknoParrot's XML schema and a validation layer.
4.  **Rendering Engine:** Use Direct3D11 + OpenGL, explicitly avoiding Qt.
5.  **Game Wheel UI:** Implement the classic Hyperspin-style wheel with user-selectable vertical, horizontal, and angled/curved scrolling, ensuring smooth animations.
6.  **Media Management:** Automatic media downloads (artwork, logos, cart art, themes, video previews) running in the background to fill missing assets.

**Unique Features (Arcadia Enhancements):**

1.  **Smart Wizard (AI Assistant):** A local, text-based command interface to assist with setup, configure emulators, fix missing/corrupt files, automate controller/path/artwork setup, and repair TeknoParrot configurations.
2.  **Dynamic Playlists & Smart Collections:** Auto-generated and always up-to-date playlists based on genre, publisher, decade, completion status.
3.  **Instant Media Generation (AI-Driven):** Generate missing box art, logos, fan art, and preview videos with style filters (retro neon, modern flat, realistic, minimalist).
4.  **Intelligent Search + Filters:** Fast, comprehensive search across all systems and launchers with advanced filtering options (multiplayer count, arcade-only, year/genre).
5.  **Smart Save/State Manager:** Unified UI for save states across emulators, instant resume, and auto-backup.
6.  **Game Preview Mode (Attract Feature):** Hover-based 30-second gameplay previews and an option for auto-cycling games like an arcade attract mode.
7.  **Plugin Ecosystem:** Support for custom themes, shaders, import/export tools, with an official SDK.
8.  **Smart Multiplayer Helper:** Detects multiplayer-capable games, creates co-op/4-player playlists, and pre-configures multiple controllers.

This research confirms the feasibility of the user's vision by drawing inspiration from existing solutions while focusing on direct launching, comprehensive PC game integration, and innovative AI-driven features for an enhanced user experience.
