# Player Combat

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Press attack to play attack animation, open a sword hitbox during animation events, damage enemies, and play attack SFX. |
| Related systems | [Player](../Systems/Player.md), [Combat](../Systems/Combat.md), [Audio](../Systems/Audio.md) |

## Current Behavior

1. Player presses `F`.
2. `PlayerCombat` ignores input if player health is dead or player is falling.
3. `PlayerAnimator.PlayAttack()` triggers the attack animation.
4. Animation events call `EnableHitbox()` and `DisableHitbox()`.
5. The sword `Hitbox` damages each `HurtBox` target once per activation.
6. Attack animation events can play one of three player attack SFX clips.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Player/PlayerCombat.cs` | Attack input and animation-event bridge. |
| `Assets/Scripts/Core/Hitbox.cs` | Damage trigger that targets HurtBoxes by default. |
| `Assets/Scripts/Core/HurtBox.cs` | Damage receiver marker used to resolve target Health. |
| `Assets/Scripts/Core/Health.cs` | Damage target. |
| `Assets/Prefabs/Player/Player 1.prefab` | Player combat prefab wiring. |

## Known Gaps

- `PlayerCombat.attackTrigger` is assigned but currently unused; `PlayerAnimator.PlayAttack()` owns the trigger.
- Combat SFX are called directly through `AudioManager`.
