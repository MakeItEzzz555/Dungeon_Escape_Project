# Player Movement And State

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Move the player through dungeon rooms, idle naturally, fall/die when hazards trigger, and respawn by scene reload. |
| Related systems | [Player](../Systems/Player.md), [Input](../Systems/Input.md) |

## Current Behavior

1. Player movement uses horizontal and vertical legacy input axes.
2. Movement is cardinal: one axis is chosen at a time.
3. The player stops when dead, falling, or control-locked.
4. Idle time drives standby animation.
5. Falling starts a fall animation/state and reloads the active scene after delay.
6. Death starts death animation/state and reloads the active scene after delay.
7. A short spawn grace period prevents instant fall-zone activation on scene load.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Player/Player 1.prefab` | Player prefab. |
| `Assets/Scripts/Player/PlayerController.cs` | Movement, lock state, death/fall, idle, footstep audio, HurtBox creation. |
| `Assets/Scripts/Player/PlayerAnimator.cs` | Animation bridge. |

## Known Gaps

- `PlayerController` owns too many responsibilities.
- Movement input still uses legacy `Input`.
- HurtBox ownership is split between runtime creation and prefab setup instructions.
