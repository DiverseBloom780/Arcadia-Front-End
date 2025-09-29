Phase 1a – Core Front End Tasks

1. Core Engine Setup

Use C++ as base language.

Implement Direct3D11 + OpenGL rendering backends.

Integrate FFmpeg for video playback.

Add a resource manager for images, videos, and audio.

2. Input & Config System

Support controller + keyboard input.

Config system via JSON/INI/XML.

Enable hot-reload for configs.

3. User Interface

Wheel Navigation:

Display logos, boxart, cartart, layered for depth.

Smooth GPU scrolling.

Keyboard/controller navigation.

Hub Navigation:

Grid/shelf view for systems/consoles.

Transitions between Hub ↔ Wheel.

Include placeholder graphics for missing media.

4. Media System

Load/caching for images (PNG/JPG).

Support themes, boxart, cartart, logos, fanart, gameplay videos.

Auto-assign media by filename matching.

Graceful fallback placeholders.

5. Emulator Launching

Direct process launching, no AutoHotkey.

Emulator profiles per system.

Config-driven launch parameters.

Error handling for missing/broken executables.

6. PC Launcher Detection (Installed Games Only)

Detect Steam, Epic Games, GOG Galaxy installed games.

Merge detected PC games with emulator library.

Only detection + launch; no cloud integration or downloads.

7. Theme Engine v1

Scripting support (Lua or Python).

Variables: position, rotation, scale, fade.

Transition effects: fade, slide, zoom.

Layer support for all media types (logos, boxart, cartart, videos, fanart).

8. Stability & Polish

Optimize render loop (locked 60fps).

Async media loading.

Test large libraries (10k+ ROMs).

Build release package (Windows/Linux).

Phase 1b – AI Smart Wizard Tasks

1. First-Time Setup

Scan system for emulators, ROMs, media.

Auto-build configs.

Minimal typed prompts for the user.

2. Directory & File Awareness

Track emulators, ROMs, media folders, config folders.

Detect missing/corrupted files.

Update paths automatically if files/folders are moved.

3. File Repair & Media Handling

Redownload/regenerate missing logos, boxart, cartart, videos.

Rebuild emulator configs.

Detect/correct invalid ROM formats.

Auto-rename files to match media.

Clean duplicates.

4. BIOS Handling (No Bundled BIOS)

Detect required BIOS files per emulator.

Validate presence + hash if present.

Provide typed instructions for user:

Filename

Correct folder path

Region/version

Alert user if BIOS is missing before launch.

5. Typed Command Interface

Users interact via typed commands only.

Examples: fix mame, check bios psx, download media genesis.

6. Continuous Helper

Runs silently in background.

Alerts user only when action is required.

Maintains consistent, updated, functional library.
