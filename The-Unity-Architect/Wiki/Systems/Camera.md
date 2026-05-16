# Camera System

Last audited: 2026-05-16

## Ownership

Camera behavior is split between continuous player follow and transition zoom.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/FollowPlayer.cs` | Camera follow, target reacquisition, pause toggle input. |
| `Assets/Scripts/Managers/CameraTransitionSystem.cs` | Transition zoom system. |
| `Assets/Prefabs/Camera/Main Camera.prefab` | Camera prefab with `FollowPlayer`. |
| `Assets/Prefabs/Camera/CameraTransition.prefab` | Camera transition prefab. |
| `Assets/Resources/PersistentSystems.prefab` | Contains current persistent camera transition script reference. |

## FollowPlayer

`FollowPlayer` owns:

- `target`
- follow offset
- smoothing speed
- snap-on-start behavior
- pause menu reference

It subscribes to `SceneManager.sceneLoaded` and reacquires the object tagged `Player`.

If the camera object has no parent, it marks itself `DontDestroyOnLoad`.

## Pause Menu Coupling

`FollowPlayer.Update()` watches `Escape` and toggles `pauseMenu`.

This means the camera follow script currently owns pause input routing. Future input cleanup should move pause input into a dedicated input/menu controller.

## CameraTransitionSystem

`CameraTransitionSystem` owns:

- gameplay orthographic size
- transition orthographic size
- zoom-in duration
- zoom-out duration
- camera reference
- `FollowPlayer` reference

During zoom in, it can temporarily assign the follow target to a transition target. During zoom out, it returns orthographic size to gameplay zoom.

## AudioListener Cleanup

On scene load, `CameraTransitionSystem` destroys duplicate `AudioListener` components that are not on its own GameObject.

## Known Risks

- `CameraTransitionSystem.ZoomRoutine()` stores `originalTarget` but does not currently restore it inside the routine.
- Pause input is coupled to camera follow.
- Duplicate prevention in `FollowPlayer` is static and destroys duplicate camera follow objects.
