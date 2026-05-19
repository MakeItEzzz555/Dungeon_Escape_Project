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
- `LoadingScene`
- `ReloadingScene`
- `FadingFromBlack`
- `ZoomingOut`
- `RestoringControl`

## Transition Flow

1. `CheckpointDoor` validates quest completion.
2. It disables player control, physics velocity, rigidbody simulation, and collider.
3. It calls `SceneTransitionManager.BeginTransition(targetScene, zoomTarget)`.
4. `SceneTransitionManager` disables player control again as a safety layer.
5. It calls `CameraTransitionSystem.StartZoomIn()`.
6. It calls `UITransitionManager.FadeToBlack()`.
7. It loads the target scene asynchronously.
8. It waits one frame.
9. It finds the new player and calls `ResetState()` and `SetControlEnabled(false)`.
10. It calls `UITransitionManager.FadeFromBlack()`.
11. It calls `CameraTransitionSystem.StartZoomOut()`.
12. It re-enables player control and returns state to `None`.

## Respawn Transition Flow

1. `PlayerController` starts death or fall state and locks movement.
2. It calls `SceneTransitionManager.BeginRespawnTransition(playerTransform, failureAnimationDuration)`.
3. `SceneTransitionManager` enters `FailureAnimation` immediately, so hazard triggers see `IsTransitioning`.
4. After the failure animation read time, it zooms in on the player.
5. It fades to black.
6. It reloads the active scene asynchronously.
7. It waits one frame, finds the new player, calls `ResetState()`, and keeps control disabled.
8. It fades from black.
9. It zooms out to gameplay zoom.
10. It restores player control and returns state to `None`.

## Fade Overlay Input Rule

`UITransitionManager` uses a full-screen fade overlay canvas sorted above gameplay UI. The overlay must block raycasts only while fading or black. When transparent and idle, `fadeOverlay.raycastTarget` must be false or it will invisibly block pause menu UI input.

## Timeout Behavior

Zoom and fade waits have a 5 second timeout. If a callback does not arrive, the transition logs an error and continues.

## Known Risks

- `CheckpointDoor` and `SceneTransitionManager` both manipulate player control/physics.
- Scene loading uses scene names as strings.
- Transition logic finds the player by tag after load; missing `Player` tag will break control restoration.
- Respawn transitions depend on the persistent transition infrastructure being present; if missing, `PlayerController` falls back to direct active-scene reload.
