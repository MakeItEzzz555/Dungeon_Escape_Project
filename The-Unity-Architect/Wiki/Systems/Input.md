# Input System

Last audited: 2026-05-16

## Ownership

Input is split between gameplay scripts using legacy `UnityEngine.Input` and UGUI using the new Input System package through `InputSystemUIInputModule`.

## Project Settings

| Setting | Value |
|:--------|:------|
| `ProjectSettings/ProjectSettings.asset` `activeInputHandler` | `2` |
| Meaning | Both legacy input and the new Input System are enabled. |
| UI module | `UnityEngine.InputSystem.UI.InputSystemUIInputModule` |

## Gameplay Input Call Sites

| Script | Input |
|:-------|:------|
| `PlayerController` | `Input.GetAxisRaw("Horizontal")`, `Input.GetAxisRaw("Vertical")` |
| `PlayerDash` | `Input.GetKeyDown(KeyCode.LeftShift)` for dash |
| `PlayerCombat` | `Input.GetKeyDown(KeyCode.F)` for attack |
| `FollowPlayer` | `Input.GetKeyDown(KeyCode.Escape)` for pause menu toggle |
| `CheckpointDoor` | `Input.GetKeyDown(KeyCode.E)` while in range |
| `Lever_On` | `Input.GetKeyDown(KeyCode.E)` while in range |
| `door_interaction` | `Input.GetKeyDown(KeyCode.E)` while in range |
| `open_chest` | `Input.GetKeyDown(KeyCode.E)` while in range |

## UI Input Infrastructure

| Scene | EventSystem State |
|:------|:------------------|
| `Main Menu.unity` | Authored EventSystem with `InputSystemUIInputModule`. |
| `Level 1.unity` | Authored EventSystem with `InputSystemUIInputModule`. |
| `Level 2.unity` | Authored EventSystem under `GlobalManagers` with `InputSystemUIInputModule`. |

`GameBootstrapper.EnsureEventSystemExists()` is a fallback only. It must not replace authored EventSystems.

## Pause Menu Input Rules

- `FollowPlayer` owns the `ESC` toggle.
- `MainMenu.PauseGame()` unlocks and shows the cursor.
- `MainMenu.ResumeGame()` restores the previous cursor state.
- `MainMenu.PauseGame()` forces Input System UI update mode to `ProcessEventsInDynamicUpdate` so UGUI pointer input continues while `Time.timeScale == 0`.

## Known Risks

- The project is in a mixed input state. A future input refactor should choose either legacy input or a full Input System action-map architecture.
- UGUI works through EventSystems; gameplay input currently does not depend on the EventSystem.
- Missing or duplicated EventSystems can break menu hover/click/drag while `ESC` still works.
