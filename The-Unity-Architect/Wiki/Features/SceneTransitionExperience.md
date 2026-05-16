# Scene Transition Experience

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Complete a level and see a zoom/fade transition into the next level with control restored afterward. |
| Related systems | [Scene Transitions](../Systems/SceneTransitions.md), [Camera](../Systems/Camera.md), [Progression](../Systems/Progression.md) |

## Current Behavior

1. Checkpoint door confirms quest completion.
2. Player control and physics are locked.
3. Camera zooms in toward the door/target.
4. Screen fades to black.
5. Target scene loads asynchronously.
6. New scene waits one frame for scene-loaded hooks.
7. Player state is reset and control remains locked.
8. Screen fades from black.
9. Camera zooms back out.
10. Player control is restored.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Managers/SceneTransitionManager.cs` | Transition orchestration. |
| `Assets/Scripts/Managers/CameraTransitionSystem.cs` | Zoom. |
| `Assets/Scripts/Managers/UITransitionManager.cs` | Fade. |
| `Assets/Scripts/Interactables/CheckpointDoor.cs` | Starts transition. |

## Known Gaps

- Target scenes are string names.
- Player locking is split between `CheckpointDoor` and `SceneTransitionManager`.
