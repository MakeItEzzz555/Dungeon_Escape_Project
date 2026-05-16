# Enemy Encounters

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Encounter enemies that idle, chase, attack, return home, and die. |
| Related systems | [Enemy AI](../Systems/EnemyAI.md), [Combat](../Systems/Combat.md) |

## Current Behavior

1. Enemy starts at home position.
2. Aggro trigger detects player or player HurtBox.
3. Enemy chases while player is in aggro range.
4. Enemy attacks while player is in attack range or close enough by fallback distance.
5. Enemy returns home when player leaves aggro.
6. Enemy enters dead state when its `Health` is dead.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Enemy/Enemy.prefab` | Enemy prefab used in Level 2. |
| `Assets/Scripts/Enemy/EnemyAI.cs` | State machine and movement. |
| `Assets/Scripts/Enemy/EnemyRangeTrigger.cs` | Aggro/attack range trigger detection. |
| `Assets/Scripts/Enemy/EnemyCombat.cs` | Enemy hitbox animation events. |
| `Assets/Scripts/Enemy/EnemyAnimationEventForwarder.cs` | Visual animator event bridge. |

## Known Gaps

- Enemy retargeting after player replacement is limited.
- Range trigger logs are noisy.
- Movement uses `Time.deltaTime` in `Update` rather than fixed-step physics timing.
