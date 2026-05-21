# HP Consumable

Last updated: 2026-05-21

## Design Contract

| Field | Value |
|:------|:------|
| Name | HPConsumable |
| Objective | Let Level 3 health pickup objects restore player HP through the shared health system. |
| Scope | Trigger pickup, player health restore, HUD feedback, optional collect animation, and self-consumption. |

## Business Rules

1. `HPConsumable` is collected through a `Collider2D` set to `Is Trigger`.
2. Only the player can collect it.
3. Collection heals the player's `Health` by `healAmount`, default `2`.
4. Healing clamps at `Health.maxHealth`.
5. The pickup is not consumed at full health unless `consumeWhenAtFullHealth` is enabled.
6. Dead players cannot collect or consume HP pickups.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Collectibles/HPConsumable.cs` | Pickup trigger behavior. |
| `Assets/Scripts/Core/Health.cs` | Owns `Heal(int amount)` and `OnHealthChanged` notification. |
| `Assets/Scenes/Level 3.unity` | Target scene for `HP_consumable` objects. |

## Setup

Each `HP_consumable` object in Level 3 should have:

| Component | Required Setup |
|:----------|:---------------|
| `Collider2D` | `Is Trigger` enabled. |
| `HPConsumable` | `healAmount = 2`. |
| Optional `Animator` | State named `Collect` if collection animation is desired. |

## Verification

1. Damage the player below max health.
2. Walk the player into an `HP_consumable` trigger.
3. Confirm player HP increases by 2 without exceeding max health.
4. Confirm HUD HP updates through `Health.OnHealthChanged`.
5. Confirm the pickup consumes itself only after successful healing, unless full-health consumption is enabled.
