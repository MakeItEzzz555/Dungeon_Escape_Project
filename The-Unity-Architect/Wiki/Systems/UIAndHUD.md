# UI And HUD System

Last audited: 2026-05-19

## Ownership

The UI layer shows menu panels, pause controls, HP images, HUD counters, UX messages, run results, and audio sliders. It should not own core gameplay simulation, but the current HUD also keeps lightweight per-attempt presentation stats for coins, timer display, and enemy-result display.

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

`MainMenu.QuitGame()` is scene-aware. From gameplay pause UI it restores `Time.timeScale`, cursor, and HUD suppression state, then loads `Main Menu`. From the `Main Menu` scene it quits the application; in the Unity Editor it stops Play Mode because `Application.Quit()` is ignored by the editor.

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
- `RunResultsPanel` panel, manually authored under `HUD_Canvas` and assigned to `HUDManager`.
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
- Run results panel text and mode-specific Continue/Respawn buttons.
- Per-attempt active gameplay timer for results display only.
- Per-attempt active enemy total and enemy death count for results display only.

`HUDManager` subscribes to the current player `Health.OnHealthChanged` event on start and after scene loads. It also checks the bound player health each frame so late player/HUD binding cannot leave HP stale. HP images under `HUD_HP` are visible from scene start and disabled from right to left as HP decreases.

`HUDManager` recalculates total coins on scene load using either `manualMaxCoins` or the count of objects tagged `Coin`.

`HUDManager` snapshots run results before completion or failure scene loading. It displays:

- current scene name plus `Passed` or `Failed`
- coins collected / total coins
- active gameplay time formatted as `MM:SS`
- enemies killed / total active enemies, or `N/A` if no active enemies exist

Pause and results are modal HUD states. When pause opens, `MainMenu` moves the pause menu to the last sibling under `HUD_Canvas` and asks `HUDManager` to suppress `HUD_Items` plus `HUD_HP` until resume. While `RunResultsPanel` is visible, `HUDManager` temporarily raises the HUD canvas sorting order, moves `RunResultsPanel` to the last sibling, activates its `ResultsPanel` child, deactivates its nested `settings_panel`, and suppresses `HUD_Items` plus `HUD_HP`. This keeps gameplay HUD visuals out of modal views and prevents their raycast-target graphics from blocking buttons.

`HUDManager` also auto-binds `Continue_bttn`, `Respawn_bttn`, and `Exit_bttn` under `RunResultsPanel` by exact name. Inspector wiring may still be used, but runtime binding prevents missing OnClick references from leaving result buttons inert after manual prefab edits.

`SceneTransitionManager` freezes `Time.timeScale` while run results are visible. The results panel uses unscaled UI fading, so buttons remain usable and audio continues while enemies, physics, and gameplay timers stop.

## Callers

| Caller | UI API Used |
|:-------|:------------|
| `Collect_coins` | `RegisterCoinCollection()`, `ShowUXMessage("Coin Collected")` |
| `Collect_keys` | `ShowUXMessage("Key Collected")` |
| `GlobalQuestManager` | `UpdateKeys()` |
| `CheckpointDoor` | `ShowUXMessage("Requires Key")` |
| `SceneTransitionManager` | `CreateRunStatsSnapshot()`, `ShowRunResults()`, `HideRunResultsImmediate()` |
| `InteractiveGate` | `ShowUXMessage("Gate Opened")`, `ShowUXMessage("Gate Closed")` |
| `open_chest` | `ShowUXMessage("Chest Opened")` |

## Known Risks

- `HUDManager` is presentation, but gameplay systems call it directly.
- `HUDManager` now owns lightweight run stats for presentation. A future `RunStatsTracker` could separate this from HUD presentation if the stats become gameplay-affecting.
- Several UGUI images/text objects have `RaycastTarget` enabled; harmless for normal hierarchy but can block input if placed above buttons.
- `Exit_bttn` previously had a broken target in one inspection pass. Verify after manual scene/prefab edits.
