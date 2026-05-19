# Run Results Panel

## 1. Feature Summary

**Name:** Run Results Panel

**Objective:** Show a clean end-of-attempt summary after either level completion or player failure. The player should see the level result, collected coins, active gameplay time, and enemy kill progress before choosing to continue, respawn, adjust settings, or exit to the main menu.

**Scope:** This feature adds a reusable results overlay and changes completion/death flow from immediate scene loading into player-confirmed progression. It does not add leaderboards, persistent scoring, rankings, achievements, save files, or live in-game timer display.

The results panel is authored under the existing gameplay HUD canvas. It is not a separate scene or a separate canvas.

## 2. Business Rules

### Main Flow

#### Level Completion

1. The player reaches the checkpoint door and uses the required key interaction.
2. The checkpoint door validates that level completion requirements are met.
3. Gameplay locks, pause input is blocked, and the active gameplay timer stops.
4. The camera zooms toward the checkpoint door zoom target.
5. The screen fades fully to black.
6. The `RunResultsPanel` fades in over the black screen in completion mode.
7. Gameplay time is frozen while the panel waits for player input, but music continues.
8. The panel title shows the current level name and passed state.
9. The panel displays coins collected from the HUD coin source, active gameplay time, and enemy kill progress.
10. `Continue` restores time, then loads the checkpoint door target scene while the screen remains black.
11. The next scene restores through the existing fade-out and zoom-out transition.

#### Death Or Fall

1. The player dies from enemy damage, trap damage, or fall-zone failure.
2. The correct death or fall animation read time is respected.
3. Gameplay locks, pause input is blocked, and the active gameplay timer stops.
4. The camera zooms toward the player.
5. The screen fades fully to black.
6. The `RunResultsPanel` fades in over the black screen in respawn mode.
7. Gameplay time is frozen while the panel waits for player input, but music continues.
8. The panel title shows the current level name and failed state.
9. The panel displays coins collected during the failed attempt from the HUD coin source, active gameplay time, and enemy kill progress.
10. `Respawn` restores time, then reloads the active scene while the screen remains black.
11. The scene restores through the existing fade-out and zoom-out transition.
12. Attempt stats reset, and the timer starts again only when player control is restored.

### States

| State | Meaning |
|:------|:--------|
| Gameplay | Player can control the character and active gameplay time is counted. |
| Paused | Pause/settings UI is open and active gameplay time is not counted. |
| Completing | Checkpoint completion has been accepted and transition into results is running. |
| Failed | Death/fall has been accepted and transition into results is running. |
| ResultsVisible | The black screen remains active and `RunResultsPanel` is visible. |
| Continuing | Continue was pressed and the target scene is loading. |
| Respawning | Respawn was pressed and the active scene is reloading. |
| ExitingToMenu | Exit was pressed and the Main Menu scene is loading. |

### Failure Conditions

- If checkpoint requirements are not met, the results flow does not begin and the existing feedback message is shown.
- If no target scene is configured for completion, the flow must fail safely and log a clear error.
- If transition infrastructure is missing, the game should fail safely rather than leaving the player permanently locked.
- If the `RunResultsPanel` is not assigned or missing required text/button references, the system should log a warning and avoid null-reference crashes.

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| Results Flow Orchestrator | Coordinates zoom, fade, result display, player confirmation, scene loading, and control restoration. |
| Results Panel Presenter | Owns `RunResultsPanel` visibility, alpha fade, title text, stat text, and mode-specific button visibility. |
| Run Stats Source | Provides attempt snapshots for coins, time, enemies killed, and active enemy total. |
| Level Timer | Measures active gameplay time only. |
| Completion Trigger | Validates key completion and starts completion-mode results. |
| Failure Trigger | Starts respawn-mode results after death or fall animation read time. |
| Pause Input Gate | Blocks ESC pause while completion/failure/results flow is active. |

### Configuration

| Setting | Purpose |
|:--------|:--------|
| Results panel fade duration | Controls how long the panel takes to fade in/out. |
| Results title text | Displays level-specific passed/failed title. |
| Coin results text | Displays collected and total coins. |
| Time results text | Displays active gameplay time. |
| Enemy results text | Displays enemy kill progress or `N/A`. |
| Continue button | Visible only in completion mode. |
| Respawn button | Visible only in failure mode. |
| Settings button | Opens the existing settings presentation from results. |
| Exit button | Returns to Main Menu from results. |
| Completion target scene | Scene loaded by `Continue`, configured per checkpoint. |
| Completion zoom target | Optional checkpoint zoom target, falling back to the checkpoint transform. |

### Dependencies

| Dependency | Use |
|:-----------|:----|
| Scene transition system | Existing zoom/fade/scene-load language is reused. |
| Camera transition system | Owns zoom-in and zoom-out. |
| UI transition system | Owns black fade backdrop and raycast blocking while black. |
| HUD system | Supplies coin totals and presents the results panel. |
| Health/death flow | Supplies player failure events. |
| Enemy system | Supplies active enemy totals and any enemy death count. |
| Pause/menu system | Supplies settings behavior and Main Menu exit target. |

## 4. Edge Cases And Exceptions

- The black fade overlay stays fully opaque while `RunResultsPanel` is visible.
- `RunResultsPanel` is moved to the last sibling under `HUD_Canvas` when shown so it renders above sibling HUD panels and receives button raycasts.
- `HUD_Items` and `HUD_HP` are hidden while results are visible, then restored when the results panel closes.
- The results panel fades in using its own alpha rather than appearing instantly.
- `Time.timeScale` is set to `0` while results are visible so enemies, physics, and gameplay scripts stop behind the overlay.
- Continue, Respawn, and Exit restore `Time.timeScale` before loading or fading from black.
- ESC pause is blocked during completion, failure, results, and settings opened from results.
- Pause time does not count toward active gameplay time.
- Scene transition time does not count toward active gameplay time.
- Death/fall no longer auto-respawns; all death paths wait for the `Respawn` button.
- Coins shown on failure are the coins collected during that failed attempt, even though the scene reload resets them.
- Coin totals use the same HUD coin source as gameplay, including any configured manual total.
- Enemy kills count any active enemy death during the attempt, not only enemies directly killed by the player.
- Enemy kill totals count active enemies only.
- If no active enemies exist in the scene, enemy results show `Enemies Killed: N/A`.
- Level 1 completion can continue to Level 2, while Level 2 completion can continue to Main Menu using the same target-scene rule.
- Main Menu music should switch after the Main Menu scene loads, not immediately when Exit is clicked.

## 5. Out Of Scope

- No score grade, star rating, or rank.
- No leaderboard or persistent best-time storage.
- No live timer on the normal HUD.
- No key count display on the results panel.
- No new separate results scene.
- No new canvas separate from `HUD_Canvas`.
- No automatic respawn after death/fall.
- No final victory/credits scene beyond using configurable scene targets.

## Manual Unity Setup

Create the presentation hierarchy manually under the gameplay `HUD_Canvas` prefab:

1. Add a child GameObject named `RunResultsPanel`.
2. Add a `CanvasGroup` to `RunResultsPanel`.
3. Under `RunResultsPanel`, add a child named `ResultsPanel` for the summary/failure content.
4. Under `RunResultsPanel`, add a child named `settings_panel` for the copied settings content.
5. Start `RunResultsPanel` inactive, or leave it active with alpha `0`; runtime code will force it hidden on scene start.
6. Add TextMeshProUGUI fields under `ResultsPanel` for:
   - result title
   - coins result
   - time result
   - enemies killed result
7. Add `Continue_bttn` under `ResultsPanel`.
8. Add `Respawn_bttn` under `ResultsPanel`.
9. Add copied `Settings_bttn` and `Exit_bttn` under `ResultsPanel`.
10. Add/assign two optional banner GameObjects under `ResultsPanel`:
   - `Summary Banner`, shown for completion mode
   - `Failed Banner`, shown for failure/respawn mode
11. Assign the following fields on `HUDManager`:
   - `Run Results Panel Canvas Group`
   - `Run Results Title Text`
   - `Run Results Coins Text`
   - `Run Results Time Text`
   - `Run Results Enemies Text`
   - `Run Results Content Panel`
   - `Run Results Settings Panel`
   - `Summary Banner`
   - `Failed Banner`
   - `Continue Button`
   - `Respawn Button`
12. Wire `Continue_bttn` OnClick to `HUDManager.OnRunResultsContinuePressed()`, or rely on runtime auto-binding by exact button name.
13. Wire `Respawn_bttn` OnClick to `HUDManager.OnRunResultsRespawnPressed()`, or rely on runtime auto-binding by exact button name.
14. Wire `Exit_bttn` OnClick to `HUDManager.OnRunResultsExitPressed()`, or rely on runtime auto-binding by exact button name.
15. Wire copied `Settings_bttn` to activate `settings_panel` and deactivate `ResultsPanel`.
16. Wire copied settings `Close_bttn` to deactivate `settings_panel` and activate `ResultsPanel`.

`RunResultsPanel` must remain under `HUD_Canvas`. The runtime temporarily raises the HUD canvas sorting order while results are visible, moves `RunResultsPanel` to the last sibling, activates `ResultsPanel`, deactivates the nested `settings_panel`, and hides `HUD_Items`/`HUD_HP` so the panel appears above the full-black fade overlay and can receive button clicks.

## Approval

This GDD captures the intended `RunResultsPanel`, `LevelCompletionFlow`, `DeathResultsFlow`, `LevelTimer`, and `RunStats` behavior. Approval is required before task breakdown and implementation.
