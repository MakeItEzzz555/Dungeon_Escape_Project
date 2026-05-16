# Traps And Fall Hazards

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Avoid traps and dangerous fall zones that kill or reset the player. |
| Related systems | [Traps And Hazards](../Systems/TrapsAndHazards.md), [Player](../Systems/Player.md) |

## Current Behavior

### Spike/Rolling Trap

1. Trap starts enabled or disabled based on `startEnabled`.
2. Player trigger detection stores detected player.
3. Animation events open/close damage window.
4. If player is detected while damage window is open, trap starts player death sequence.
5. Lever wiring can enable/disable traps through `SetTrapActive(bool)`.

### Fall Zone

1. Player enters fall-zone trigger.
2. Zone ignores activation during scene transitions.
3. Zone ignores activation during player spawn grace period.
4. Zone starts player fall sequence once.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Traps/Spike Trap.prefab` | Spike trap prefab. |
| `Assets/Prefabs/Traps/RollingTrap.prefab` | Rolling trap prefab. |
| `Assets/Scripts/Interactables/SpikeTrapFSM.cs` | Trap state machine. |
| `Assets/Scripts/Interactables/FallZone.cs` | Fall hazard trigger. |

## Known Gaps

- Traps kill player directly instead of routing through `Health`.
- `SpikeTrapFSM.ResetTrap()` changes global `Time.timeScale`.
