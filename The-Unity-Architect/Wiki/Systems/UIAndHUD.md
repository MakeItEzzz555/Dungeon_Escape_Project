# UI And HUD System

Last audited: 2026-05-19

## Ownership

The UI layer is presentation-only. It shows menu panels, pause controls, HP images, HUD counters, UX messages, and audio sliders. It should not own gameplay state.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/UI_Menu/MainMenu.cs` | Main menu and pause menu behavior. |
| `Assets/Scripts/UI_Menu/AudioSettingsUI.cs` | Music/SFX slider binder. |
| `Assets/Scripts/UI_Menu/Play_Level.cs` | Dormant/empty legacy menu script. No scene/prefab references found in latest audit. |
| `Assets/Scripts/UI_Menu/Exit_Game.cs` | Dormant/empty legacy menu script. No scene/prefab references found in latest audit. |
| `Assets/Scripts/Managers/HUDManager.cs` | Gameplay HUD counters and UX messages. |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | HUD and pause menu canvas prefab used by gameplay scenes. |
| `Assets/Scenes/Main Menu.unity` | Menu scene with UGUI menu/settings panels. |

## Main Menu

`MainMenu.PlayGame()` loads `Level 1` when called from `Main Menu`. Outside the main menu scene, it calls `ResumeGame()`.

The Main Menu scene contains:

- `MainMenu` script.
- `settings_panel`, initially inactive.
- `MusicSlider` and `SFXSlider`.
- `AudioSettingsUI`.
- EventSystem using `InputSystemUIInputModule`.

## Pause Menu

The gameplay pause menu is inside `HUD_Canvas.prefab`.

Known children/components from prefab inspection:

- `HUD_Canvas` root canvas, sorting order `0`.
- `GraphicRaycaster`.
- `HUDManager`.
- `HUD_HP` panel with child HP images, when assigned or discoverable by name.
- `MainMenu` script.
- `MainMenu` panel, initially inactive.
- `settings_panel`, initially inactive.
- `Resume_bttn`, `Settings_bttn`, `Exit_bttn`, `Close_bttn`.
- `MusicSlider`, `SFXSlider`.
- `AudioSettingsUI` with both sliders assigned.

## HUD

`HUDManager` owns:

- HP image binding and display.
- HUD reveal state.
- Coin discovered state.
- Key discovered state.
- Coin count text.
- Key count text.
- UX notification fade panel and text.

`HUDManager` subscribes to the current player `Health.OnHealthChanged` event on start and after scene loads. It also checks the bound player health each frame so late player/HUD binding cannot leave HP stale. HP images under `HUD_HP` are visible from scene start and disabled from right to left as HP decreases.

`HUDManager` recalculates total coins on scene load using either `manualMaxCoins` or the count of objects tagged `Coin`.

## Callers

| Caller | UI API Used |
|:-------|:------------|
| `Collect_coins` | `RegisterCoinCollection()`, `ShowUXMessage("Coin Collected")` |
| `Collect_keys` | `ShowUXMessage("Key Collected")` |
| `GlobalQuestManager` | `UpdateKeys()` |
| `CheckpointDoor` | `ShowUXMessage("Requires Key")` |
| `InteractiveGate` | `ShowUXMessage("Gate Opened")`, `ShowUXMessage("Gate Closed")` |
| `open_chest` | `ShowUXMessage("Chest Opened")` |

## Known Risks

- `HUDManager` is presentation, but gameplay systems call it directly.
- Several UGUI images/text objects have `RaycastTarget` enabled; harmless for normal hierarchy but can block input if placed above buttons.
- `Exit_bttn` previously had a broken target in one inspection pass. Verify after manual scene/prefab edits.
