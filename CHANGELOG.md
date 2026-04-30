# Project Changelog

This file tracks all changes and developments made to the "Code Game" project (a 2D farming RPG inspired by Stardew Valley, built with MonoGame).

## [Unreleased]

### Added
- Centralized asset path config in `Scripts/Core/ContentPaths.cs` for easy updates to content loading.
- Character creation screen with name, farm name, favorite thing fields, and animal preference selector.
- API/storage layers for player profiles:
  - `Scripts/Services/Api/IPlayerProfileApi.cs`
  - `Scripts/Services/Api/PlayerProfileApi.cs`
  - `Scripts/Services/Storage/IPlayerProfileRepository.cs`
  - `Scripts/Services/Storage/PlayerProfileRepository.cs`
  - `Scripts/Domain/PlayerProfile.cs`
- `CharacterCreationService` now saves profile to a local JSON database and uploads via API.
- Steam multiplayer support with lobby creation/joining for co-op 2-4 players.
- Steam Cloud save integration for syncing save data across devices.
- "Load Game" button in BeginningScreen to load saved player profiles.
- Basic GameplayScreen to display loaded character information and provide back-to-menu functionality.

### Fixed
- Steam Cloud loading errors when Steam is not available - added proper exception handling in PlayerProfileRepository and CharacterCreationService
- Load Game button now works correctly and can load saved character profiles

## [0.1.0] - 2026-04-26

### Added
- Initial professional folder structure for production-ready development:
  - Organized `Assets/` for source files (Audio, Fonts, Sprites, Tilesets).
  - Organized `Content/` for processed assets.
  - Created `Scripts/` with subfolders: Core, Systems (Inventory, Time, Save, Dialogue), Entities (Player, NPCs, Animals), UI (HUD, Menus, Cutscenes), Screens (MainMenu, Gameplay, Levels).
  - Added `Data/` for game data files.
  - Added `Tests/` for unit/integration tests.
  - Moved existing `Game1.cs` and `Program.cs` to `Scripts/Core/`.
  - Created `.gitignore` with standard MonoGame exclusions (bin/, obj/, user files, IDE files, processed content).
- File naming conventions established (PascalCase for scripts, snake_case for assets).
- Asset organization strategy defined (tilesets, sprites, audio, fonts).
- Code splitting into decoupled systems (Player, NPC, Inventory, Time, Save, Dialogue).
- Scene/level management structure outlined (MainMenu, Gameplay, HUD, Cutscenes).
- Git best practices documented (.gitignore, branch strategy, commit conventions).

### Notes
- Project uses MonoGame with .NET 9.0.
- Follows clean architecture principles for maintainability.
- Ready for development of features: crop farming, animal raising, day/night cycle, NPC schedules, inventory, crafting, save/load.