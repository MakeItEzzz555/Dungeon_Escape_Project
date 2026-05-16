# Camera Follow And Zoom

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Camera follows the player during gameplay and zooms during level transitions. |
| Related system | [Camera](../Systems/Camera.md) |

## Current Behavior

1. Camera follows the object tagged `Player`.
2. On scene load, camera reacquires the player target.
3. Camera interpolates toward target plus offset.
4. Scene transitions can temporarily zoom camera in and out.
5. Camera transition system removes duplicate AudioListeners on scene load.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/FollowPlayer.cs` | Follow and target reacquisition. |
| `Assets/Scripts/Managers/CameraTransitionSystem.cs` | Transition zoom. |
| `Assets/Prefabs/Camera/Main Camera.prefab` | Camera prefab. |

## Known Gaps

- `FollowPlayer` also owns pause toggle input.
- Transition zoom stores original target but does not restore it explicitly in the current routine.
