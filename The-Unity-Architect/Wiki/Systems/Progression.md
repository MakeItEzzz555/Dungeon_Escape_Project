# Progression System

Last audited: 2026-05-16

## Ownership

Progression is key-based. `GlobalQuestManager` tracks required and collected keys. `CheckpointDoor` uses quest completion to authorize level completion, which now opens the run results flow before the target scene is loaded.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Managers/GlobalQuestManager.cs` | Persistent quest/key state. |
| `Assets/Prefabs/Managers/QuestManager.prefab` | Scene-authored quest manager prefab used in Level 1 and Level 2. |
| `Assets/Scripts/Collectibles/Collect_keys.cs` | Key pickup. |
| `Assets/Scripts/Interactables/CheckpointDoor.cs` | Level completion gate and transition trigger. |
| `Assets/Prefabs/Doors_Gates/CheckPointDoor.prefab` | Checkpoint door prefab used in gameplay scenes. |

## Quest State

| State | Owner |
|:------|:------|
| `requiredKeys` | `GlobalQuestManager` |
| `collectedKeys` | `GlobalQuestManager` |
| `IsQuestComplete` | `GlobalQuestManager`, true when collected keys are at least required keys and required keys is greater than zero |

## Flow

1. A level sets or serializes required key state through `GlobalQuestManager`.
2. `Collect_keys` detects player trigger, plays pickup animation/SFX, then calls `GlobalQuestManager.AddKey()`.
3. `GlobalQuestManager.AddKey()` increments collected keys and updates `HUDManager`.
4. `CheckpointDoor` checks `GlobalQuestManager.IsQuestComplete` when player presses `E` in range.
5. If complete, it locks the player and calls `SceneTransitionManager.BeginLevelCompletionTransition(targetScene, zoomTarget)`.
6. If incomplete, it shows `Requires Key` through `HUDManager`.

## Completion Results

After a checkpoint accepts completion, the scene transition system zooms in, fades to black, shows `RunResultsPanel` in completion mode, and waits for `Continue`. The target scene is loaded only after the player confirms. Level 1 can target Level 2, and Level 2 can target Main Menu using the same serialized `targetScene` field.

## Scene Transition Contract

`CheckpointDoor` serializes:

- `targetScene`
- `zoomTarget`

The target scene name must match the Unity scene asset name exactly.

## Known Risks

- `GlobalQuestManager.ResetKeys()` exists but scene reset ownership is not fully documented in code.
- `CheckpointDoor` directly disables player collider and rigidbody simulation before transition; this is effective but couples progression to player physics.
- Quest state persistence across scene loads is singleton-based. Confirm desired reset behavior per level before adding branching progression.
