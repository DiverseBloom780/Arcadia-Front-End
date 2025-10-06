# Arcadia Developer Guide

This guide provides comprehensive information for developers who want to contribute to Arcadia, extend its functionality, or understand its internal architecture.

## Table of Contents

1. [Development Environment Setup](#development-environment-setup)
2. [Project Structure](#project-structure)
3. [Core Architecture](#core-architecture)
4. [Building and Debugging](#building-and-debugging)
5. [Adding New Features](#adding-new-features)
6. [Plugin Development](#plugin-development)
7. [Testing](#testing)
8. [Contributing](#contributing)

## Development Environment Setup

To set up your development environment for Arcadia, you will need the following tools and dependencies installed on your Windows machine.

### Required Software

The development of Arcadia requires Visual Studio 2022 or later with the .NET desktop development workload installed. This provides access to the .NET 8.0 SDK, WPF designer, and C# compiler. You will also need Git for version control to clone the repository and manage your contributions.

### Optional Tools

For enhanced productivity, consider installing ReSharper or Rider for advanced C# refactoring and code analysis. Visual Studio Code with the C# extension can serve as a lightweight alternative editor. SQL Browser for SQLite is useful for inspecting the game database during development.

### Cloning the Repository

Begin by cloning the Arcadia repository from GitHub to your local machine. Navigate to your desired directory and execute the clone command. Once cloned, open the solution file in Visual Studio 2022.

### Installing Dependencies

All NuGet packages will be automatically restored when you first open the solution in Visual Studio. If you encounter any issues, manually restore packages through the NuGet Package Manager.

## Project Structure

The Arcadia solution is organized into multiple projects, each responsible for a specific aspect of the application. Understanding this modular structure is essential for effective development.

### Arcadia.Core

This project contains the core business logic, data models, and services. Key components include the Game and Emulator models, which represent games and emulators in the system. The GameDatabase service manages SQLite operations for storing and retrieving game data. The GameLauncher service handles the launching of games across different platforms and emulators.

### Arcadia.UI

The UI project implements the WPF-based user interface with DirectX rendering. The MainWindow provides the primary game wheel interface and navigation. Custom controls and themes are defined for visual consistency. DirectX integration via SharpDX enables high-performance rendering.

### Arcadia.Launchers

This project handles integration with external game launchers and platforms. The SteamIntegration service detects and imports Steam games. The GOGIntegration service handles GOG Galaxy games. The EpicGamesIntegration service manages Epic Games Store titles. The TeknoParrotIntegration service provides comprehensive TeknoParrot support with automatic profile generation.

### Arcadia.Emulators

The Emulators project manages standalone emulator configurations and launching. It includes an emulator database with command-line templates, auto-download functionality for supported emulators, and BIOS management and validation.

### Arcadia.Media

This project handles media scraping, caching, and generation. It provides scrapers for online databases like TheGamesDB and ScreenScraper. Local media cache management ensures efficient storage. AI-driven media generation creates missing assets when needed.

### Arcadia.SmartWizard

The SmartWizard project implements the AI assistant for setup and troubleshooting. It includes a local knowledge base about Arcadia's structure and configuration. Natural language command processing allows intuitive interaction. Automated troubleshooting and repair functions help users resolve issues quickly.

### Arcadia.Updater

The Updater project manages GitHub-based automatic updates. It uses the Octokit library for GitHub API integration. Version comparison and update detection ensure users stay current. Changelog parsing and display inform users of new features. Automatic download and installation streamline the update process.

## Core Architecture

Arcadia follows a layered architecture with clear separation of concerns. The presentation layer consists of WPF views and view models. The business logic layer contains services and game management logic. The data access layer uses SQLite for persistence. The integration layer connects to external platforms and emulators.

### Data Flow

When a user navigates the game wheel, the MainWindow retrieves game data from the GameDatabase. Selected game details are displayed in the UI with media loaded from the local cache. When a game is launched, the GameLauncher determines the appropriate launch method based on the game's LaunchType. The launcher constructs and executes the launch command, updating play statistics in the database.

### Event-Driven Architecture

Arcadia uses an event-driven approach for UI updates and system notifications. Game selection changes trigger UI updates. Media download completion events refresh the display. Update availability notifications prompt the user.

## Building and Debugging

To build Arcadia, open the solution in Visual Studio 2022 and select the desired configuration (Debug or Release). Restore NuGet packages if prompted, then build the solution using the Build menu or keyboard shortcut. The compiled application will be output to the bin directory of the Arcadia.UI project.

### Debugging

Set breakpoints in the code where you want to pause execution. Start debugging by pressing F5 or using the Debug menu. Use the Visual Studio debugger to inspect variables, step through code, and diagnose issues. The Immediate Window allows you to execute code and evaluate expressions during debugging.

### Common Build Issues

If you encounter errors related to missing NuGet packages, ensure all packages are restored. For SharpDX-related errors, verify that the SharpDX NuGet packages are correctly installed. SQLite errors may require checking that the System.Data.SQLite package is properly referenced.

## Adding New Features

When adding new features to Arcadia, follow these best practices to maintain code quality and consistency.

### Adding a New Launcher Integration

Create a new class in the Arcadia.Launchers project following the naming convention of existing integrations. Implement detection logic to find the launcher installation. Parse the launcher's game library format. Create Game objects with appropriate metadata. Implement the launch logic using URIs or direct executable calls. Add configuration options to the settings file. Update the MainWindow to include the new launcher in the scanning process.

### Adding a New Emulator

Create an Emulator configuration in the Arcadia.Emulators project. Define the command-line template with appropriate variables. Specify supported platforms and file extensions. Add BIOS requirements if applicable. Implement auto-download functionality if the emulator supports it. Test the emulator with various ROMs to ensure compatibility.

### Adding UI Features

Create new WPF controls or windows in the Arcadia.UI project. Follow the existing dark theme styling for consistency. Implement view models for data binding if needed. Add keyboard and gamepad input handling. Ensure the feature is accessible from the main interface. Test the feature at different resolutions and aspect ratios.

## Plugin Development

Arcadia supports a plugin ecosystem that allows developers to extend functionality without modifying the core application. Plugins can add custom themes, implement new media scrapers, create import/export tools, and extend the Smart Wizard with additional commands.

### Plugin Structure

A plugin is a .NET assembly (DLL) that implements the IPlugin interface. Plugins are loaded dynamically at runtime from the Plugins directory. Each plugin has a manifest file describing its metadata and dependencies.

### Creating a Plugin

Start by creating a new Class Library project targeting .NET 8.0. Add a reference to the Arcadia.Core project. Implement the IPlugin interface with methods for initialization, execution, and cleanup. Compile the plugin and place the DLL in the Plugins directory. Create a manifest JSON file with plugin metadata. Restart Arcadia to load the plugin.

### Plugin API

Plugins have access to the Arcadia API, which provides methods for accessing the game database, registering custom commands, adding UI elements, and subscribing to events.

## Testing

Arcadia uses a combination of unit tests, integration tests, and manual testing to ensure quality and reliability.

### Unit Testing

Unit tests are written using xUnit and cover core business logic in Arcadia.Core. Tests verify game database operations, launcher integration logic, and emulator configuration parsing. Run unit tests through Visual Studio's Test Explorer.

### Integration Testing

Integration tests verify the interaction between components. They test end-to-end game launching, media downloading and caching, and update checking and installation. Integration tests may require external dependencies like Steam or GOG to be installed.

### Manual Testing

Manual testing is essential for UI and user experience validation. Test the game wheel navigation and selection, verify game launching across different platforms, check media loading and display, and test settings and configuration changes. Validate the Smart Wizard's responses and suggestions.

## Contributing

Contributions to Arcadia are welcome and encouraged. To contribute, fork the repository on GitHub and create a new branch for your feature or bug fix. Make your changes following the coding standards and best practices outlined in this guide. Write tests for new functionality and ensure all existing tests pass. Commit your changes with clear, descriptive commit messages. Push your branch to your fork and submit a pull request to the main repository. Provide a detailed description of your changes and the problem they solve.

### Code Style

Arcadia follows standard C# coding conventions. Use PascalCase for class names and public members. Use camelCase for private fields and local variables. Add XML documentation comments to public APIs. Keep methods focused and concise. Use meaningful variable and method names.

### Pull Request Guidelines

Ensure your code builds without errors or warnings. All unit tests must pass. Include tests for new features or bug fixes. Update documentation if your changes affect user-facing functionality. Keep pull requests focused on a single feature or fix. Respond to code review feedback promptly.

---

For questions or assistance, please open an issue on GitHub or contact the development team. Thank you for contributing to Arcadia!
