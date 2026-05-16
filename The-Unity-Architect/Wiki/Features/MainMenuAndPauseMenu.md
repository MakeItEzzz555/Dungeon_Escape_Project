# Main Menu And Pause Menu

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Start the game, pause gameplay, resume gameplay, open settings, close settings, and quit. |
| Related systems | [UI And HUD](../Systems/UIAndHUD.md), [Input](../Systems/Input.md) |

## Current Behavior

### Main Menu

1. Player starts in `Main Menu`.
2. `PlayGame()` loads `Level 1`.
3. Gameplay music is started through `AudioManager`.
4. Settings panel exposes audio sliders.

### Pause Menu

1. During gameplay, pressing `ESC` toggles the pause menu.
2. `PauseGame()` activates the menu, unlocks/shows cursor, opens the main pause panel, closes settings, and sets `Time.timeScale = 0`.
3. `ResumeGame()` hides the menu, restores `Time.timeScale = 1`, and restores cursor state.
4. Settings panel exposes audio sliders.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/UI_Menu/MainMenu.cs` | Menu/pause behavior. |
| `Assets/Scripts/FollowPlayer.cs` | Owns `ESC` pause toggle. |
| `Assets/Scenes/Main Menu.unity` | Main menu scene. |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | Gameplay pause menu and HUD canvas. |

## Known Gaps

- Pause input currently lives in `FollowPlayer`, which couples camera follow to menu control.
- Some button OnClick wiring has previously been fragile and should be verified after manual prefab edits.
