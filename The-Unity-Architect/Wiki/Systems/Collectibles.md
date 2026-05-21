# Collectibles System

Last audited: 2026-05-21

## Ownership

Collectibles are trigger-based scene objects that play a collection animation/SFX, update HUD or quest state, then destroy themselves after a short delay.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Collectibles/Collect_coins.cs` | Coin pickup behavior. |
| `Assets/Scripts/Collectibles/Collect_keys.cs` | Key pickup behavior. |
| `Assets/Scripts/Collectibles/HPConsumable.cs` | HP pickup behavior. |
| `Assets/Prefabs/Collectibles/BlueCoin.prefab` | Coin prefab used in Level 1 and Level 2. |
| `Assets/Prefabs/Collectibles/Key 1 - GOLD - .prefab` | Key prefab used in Level 1 and Level 2. |

## Coin Flow

1. `Collect_coins.OnTriggerEnter2D()` checks `other.CompareTag("Player")`.
2. `Collect()` marks the coin collected.
3. It plays animation state `Collect` if an Animator exists.
4. It calls `AudioManager.PlayCoin()`.
5. It calls `HUDManager.RegisterCoinCollection()`.
6. It calls `HUDManager.ShowUXMessage("Coin Collected")`.
7. It destroys the GameObject after `destroyDelay`.

## Key Flow

1. `Collect_keys.OnTriggerEnter2D()` checks `other.CompareTag("Player")`.
2. `Collect()` marks the key collected.
3. It plays animation state `Collect` if an Animator exists.
4. It calls `AudioManager.PlayKey()`.
5. It calls `GlobalQuestManager.AddKey()`.
6. It calls `HUDManager.ShowUXMessage("Key Collected")`.
7. It destroys the GameObject after `destroyDelay`.

## HP Consumable Flow

1. `HPConsumable.OnTriggerEnter2D()` accepts the root player collider or a child collider that resolves to `PlayerController`.
2. It resolves the player's `Health` component.
3. It calls `Health.Heal(healAmount)`, default `2`.
4. If healing succeeds, it disables its trigger collider, optionally plays animation state `Collect`, shows HUD feedback, and destroys itself after `destroyDelay`.
5. If the player is at full health, it remains available unless `consumeWhenAtFullHealth` is enabled.

## HUD Integration

- `HUDManager` discovers total coins by counting objects tagged `Coin` unless `manualMaxCoins` is set.
- Key totals come from `GlobalQuestManager`.

## Known Risks

- Collection uses direct singleton calls to audio, HUD, and quest systems.
- The destroy delay assumes the collection animation length fits the configured `destroyDelay`.
- Coin and key collection still only use root/tagged player colliders; `HPConsumable` also supports child player colliders such as HurtBox contact.
