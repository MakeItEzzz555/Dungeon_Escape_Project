# Key Quest And Level Exit

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Collect enough keys to unlock the checkpoint door and transition to the next level. |
| Related systems | [Progression](../Systems/Progression.md), [Scene Transitions](../Systems/SceneTransitions.md) |

## Current Behavior

1. Level defines required key count through `GlobalQuestManager`.
2. Key pickups call `GlobalQuestManager.AddKey()`.
3. HUD updates key count.
4. Player enters checkpoint door range.
5. Player presses `E`.
6. If enough keys are collected, scene transition begins.
7. If not enough keys are collected, HUD shows `Requires Key`.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Managers/GlobalQuestManager.cs` | Key state and quest completion. |
| `Assets/Scripts/Collectibles/Collect_keys.cs` | Key pickup behavior. |
| `Assets/Scripts/Interactables/CheckpointDoor.cs` | Level exit interaction. |
| `Assets/Prefabs/Doors_Gates/CheckPointDoor.prefab` | Checkpoint door prefab. |

## Known Gaps

- Required key count setup is scene/prefab dependent and should be verified in the Inspector per level.
- Level names are string-based.
