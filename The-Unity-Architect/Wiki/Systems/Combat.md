# Combat System

Last audited: 2026-05-19

## Ownership

Combat is built around reusable `Health` and `Hitbox` components. Player and enemy combat scripts control animation-event timing for hitbox activation.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Core/Health.cs` | Shared health, hurt reaction, death dispatch. |
| `Assets/Scripts/Core/Hitbox.cs` | Shared trigger damage window. |
| `Assets/Scripts/Player/PlayerCombat.cs` | Player attack input and sword hitbox animation events. |
| `Assets/Scripts/Enemy/EnemyCombat.cs` | Enemy hitbox animation events. |
| `Assets/Scripts/Enemy/EnemyAnimationEventForwarder.cs` | Forwards animation events from enemy visuals to parent combat/audio. |
| `Assets/Prefabs/Player/Player 1.prefab` | Contains player health/hitbox path. |
| `Assets/Prefabs/Enemy/Enemy.prefab` | Contains enemy health/hitbox path. |

## Health

`Health` owns:

- `maxHealth`
- `currentHealth`
- dead state
- hurt trigger
- hurt direction animator floats
- death trigger/bool
- `OnHealthChanged(currentHealth, maxHealth)` event
- `OnDied(Health)` event for death-specific listeners such as run enemy kill stats

When health reaches zero:

1. It marks itself dead.
2. It clamps `currentHealth` to `0` and notifies health listeners.
3. It drives animator death parameters if present.
4. It invokes `OnDied`.
5. If a `PlayerController` exists on self or parent, it starts the player death sequence.
6. Otherwise it sends `OnDeath` using `SendMessage`.

`Health.DepleteHealth()` sets HP to `0` and notifies listeners without starting the standard death animation path. Fall zones use this before starting the fall sequence.

## Hitbox

`Hitbox` owns:

- `damageAmount`
- `owner`
- disabled trigger collider by default
- per-activation list of already-hit `Health` targets

`EnableHitbox()` clears the hit list and enables the collider. `DisableHitbox()` disables it.

Self-damage is prevented when the target health transform is the owner or shares the same root.

## Player Combat

- Attack key: `F`.
- Attack does not run if player health is dead or player is falling.
- Attack calls `PlayerAnimator.PlayAttack()`.
- Animation events call `EnableHitbox()`, `DisableHitbox()`, and attack SFX wrappers.

## Enemy Combat

- `EnemyAI` triggers animator attack.
- Enemy animation events should call methods on `EnemyAnimationEventForwarder`.
- The forwarder calls `EnemyCombat.EnableHitbox()` and `EnemyCombat.DisableHitbox()`.
- The forwarder also plays enemy swing SFX.

## Known Risks

- `Health` currently mixes simulation state with animator presentation.
- `SendMessage("OnDeath")` is flexible but weakly typed.
- Hitbox target tracking uses a `List<Health>`; this is fine for small melee windows but could be replaced with pooled/set storage if many hitboxes are active.
