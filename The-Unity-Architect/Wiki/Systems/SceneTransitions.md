# Scene Transition System

Last audited: 2026-05-19

## Ownership

Scene transitions and respawn transitions are orchestrated by `SceneTransitionManager`, with camera zoom owned by `CameraTransitionSystem` and fades owned by `UITransitionManager`.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Managers/SceneTransitionManager.cs` | Transition state machine, scene loading, and active-scene respawn reloads. |
| `Assets/Scripts/Managers/CameraTransitionSystem.cs` | Orthographic zoom and temporary follow target switching. |
| `Assets/Scripts/Managers/UITransitionManager.cs` | Fade overlay alpha and raycast blocking. |
| `Assets/Scripts/Interactables/CheckpointDoor.cs` | User-facing level transition trigger. |
| `Assets/Resources/PersistentSystems.prefab` | Contains persistent transition/fade/camera infrastructure. |
| `Assets/Prefabs/Camera/SceneTransManager.prefab` | Scene transition manager prefab. |
| `Assets/Prefabs/Camera/CameraTransition.prefab` | Camera transition prefab. |

## Transition States

`TransitionState`:

- `None`
- `FailureAnimation`
- `ZoomingIn`
- `FadingToBlack`
- `ResultsVisible`
- `LoadingScene`
- `ReloadingScene`
- `FadingFromBlack`
- `ZoomingOut`
- `RestoringControl`
- `ExitingToMenu`

## Transition Flow

1. `CheckpointDoor` validates quest completion.
2. It disables player control, physics velocity, rigidbody simulation, and collider.
3. It calls `SceneTransitionManager.BeginLevelCompletionTransition(targetScene, zoomTarget)`.
4. `SceneTransitionManager` disables player control again as a safety layer.
5. It calls `CameraTransitionSystem.StartZoomIn()`.
6. It calls `UITransitionManager.FadeToBlack()`.
7. It snapshots run stats through `HUDManager`.
8. It shows `RunResultsPanel` in completion mode over full black and freezes `Time.timeScale`.
9. `Continue` restores `Time.timeScale` and loads the target scene asynchronously while the screen remains black.
10. It waits one frame.
11. If the loaded scene contains a player, it calls `ResetState()` and `SetControlEnabled(false)`.
12. It calls `UITransitionManager.FadeFromBlack()`.
13. It calls `CameraTransitionSystem.StartZoomOut()` for gameplay scenes.
14. It re-enables player control and returns state to `None`.

## FinalBoss Death Completion Flow

1. `FinalBossBehavior` enters death state, stops hitboxes and charge trail, hides `HUD_HP_FinalBoss`, and reveals any configured `DeathRewardObject`.
2. If death results are enabled, it immediately calls `SceneTransitionManager.BeginLevelCompletionTransitionWithPresentationWindow(targetScene, zoomTarget, bossDeathTransitionDelay)`.
3. `SceneTransitionManager` disables player control and starts camera zoom immediately.
4. `CameraTransitionSystem` holds the FinalBoss zoom target through the presentation window.
5. `SceneTransitionManager` starts fade early enough that full black lands at the end of the presentation window, releases the held camera target, and shows `RunResultsPanel` in completion mode.

`FinalBossBehavior.resultsZoomTarget` must be a world transform. If a UI `RectTransform` is assigned, `FinalBossBehavior` logs a warning and falls back to its own transform so the camera cannot zoom toward HUD content.

## Respawn Transition Flow

1. `PlayerController` starts death or fall state and locks movement.
2. It calls `SceneTransitionManager.BeginRespawnTransition(playerTransform, failureAnimationDuration)`.
3. `SceneTransitionManager` enters `FailureAnimation` immediately, so hazard triggers see `IsTransitioning`.
4. After the failure animation read time, it zooms in on the player.
5. It fades to black.
6. It snapshots run stats through `HUDManager`.
7. It shows `RunResultsPanel` in respawn mode over full black and freezes `Time.timeScale`.
8. `Respawn` restores `Time.timeScale` and reloads the active scene asynchronously while the screen remains black.
9. It waits one frame, finds the new player, calls `ResetState()`, and keeps control disabled.
10. It fades from black.
11. It zooms out to gameplay zoom.
12. It restores player control and returns state to `None`.

## Run Results Exit Flow

When the results panel `Exit` button is pressed, `SceneTransitionManager` restores `Time.timeScale`, loads `Main Menu` while black, switches audio to main menu music after the menu scene loads, unlocks the cursor, fades from black, and returns state to `None`.

## Pause Menu Exit Flow

When the pause menu `Exit` button is pressed during gameplay, `MainMenu` delegates to `SceneTransitionManager.ExitToMainMenuFromGameplay()`. The transition manager restores `Time.timeScale`, fades to black, loads `Main Menu`, switches audio to main menu music, resets persistent camera zoom to gameplay zoom for menu presentation, unlocks the cursor, fades from black, and returns state to `None`.

## Main Menu Play Flow

When the `Main Menu` Play button is pressed, `MainMenu` delegates to `SceneTransitionManager.StartGameFromMainMenu("Level 1")`. The transition manager fades to black, loads `Level 1`, locks the new player until restoration, sets the persistent camera to transition zoom, fades from black, zooms out to gameplay zoom, restores player control, and returns state to `None`.

## Fade Overlay Input Rule

`UITransitionManager` uses a full-screen fade overlay canvas sorted above gameplay UI. The overlay must block raycasts only while fading or black. When transparent and idle, `fadeOverlay.raycastTarget` must be false or it will invisibly block pause menu UI input.

## Timeout Behavior

Zoom and fade waits have a 5 second timeout. If a callback does not arrive, the transition logs an error and continues.

## Known Risks

- `CheckpointDoor` and `SceneTransitionManager` both manipulate player control/physics.
- Scene loading uses scene names as strings.
- Transition logic finds the player by tag after load; missing `Player` tag will break control restoration.
- Respawn transitions depend on the persistent transition infrastructure being present. If missing, `PlayerController` logs an error and does not auto-reload because death/fall respawn must be player-confirmed through `RunResultsPanel`.
