# Coin And Key Collection

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Pick up coins and keys, see HUD/feedback updates, and hear pickup sounds. |
| Related systems | [Collectibles](../Systems/Collectibles.md), [UI And HUD](../Systems/UIAndHUD.md), [Audio](../Systems/Audio.md) |

## Current Behavior

### Coins

1. Player enters coin trigger.
2. Coin plays `Collect` animation if an Animator exists.
3. Coin pickup SFX plays.
4. HUD coin count updates.
5. HUD shows `Coin Collected`.
6. Coin destroys itself after delay.

### Keys

1. Player enters key trigger.
2. Key plays `Collect` animation if an Animator exists.
3. Key pickup SFX plays.
4. Quest key count updates.
5. HUD shows `Key Collected`.
6. Key destroys itself after delay.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Collectibles/BlueCoin.prefab` | Coin prefab. |
| `Assets/Prefabs/Collectibles/Key 1 - GOLD - .prefab` | Key prefab. |
| `Assets/Scripts/Collectibles/Collect_coins.cs` | Coin pickup script. |
| `Assets/Scripts/Collectibles/Collect_keys.cs` | Key pickup script. |

## Known Gaps

- Pickup detection requires collider tag `Player`.
- Destroy delay must match collection animation timing manually.
