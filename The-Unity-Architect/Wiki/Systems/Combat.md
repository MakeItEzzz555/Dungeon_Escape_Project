# Combat System

Last audited: 2026-05-19

## Ownership

Combat is built around reusable `Health` and `Hitbox` components. Player and enemy combat scripts control animation-event timing for hitbox activation.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Core/Health.cs` | Shared health, hurt reaction, death dispatch. |
| `Assets/Scripts/Core/Hitbox.cs` | Shared trigger damage window. |
| `Assets/Scripts/Core/HurtBox.cs` | Shared damage receiver marker for valid character damage colliders. |
| `Assets/Scripts/Player/PlayerCombat.cs` | Player attack input and sword hitbox animation events. |
| `Assets/Scripts/Enemy/EnemyCombat.cs` | Enemy hitbox animation events. |
| `Assets/Scripts/Enemy/EnemyAnimationEventForwarder.cs` | Forwards animation events from enemy visuals to parent combat/audio. |
| `Assets/Scripts/Enemy/FinalBossCombatBridge.cs` | Forwards FinalBoss regular and charge hitbox animation events. |
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

`Health.Heal(amount)` restores HP, clamps to `maxHealth`, ignores dead targets, and invokes `OnHealthChanged` only when HP actually changes. `HPConsumable` uses this path so HUD HP stays synchronized.

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
- target mode, defaulting to `HurtBoxOnly`
- disabled trigger collider by default
- per-activation list of already-hit `Health` targets

`EnableHitbox()` clears the hit list and enables the collider. `DisableHitbox()` disables it. Damage is resolved from both trigger enter and trigger stay callbacks, with the hit list preventing repeat damage, so an attack still lands when the target was already overlapping the hitbox at the animation-event frame.

By default, `Hitbox` only resolves damage from a `HurtBox` component or a legacy child collider named `HurtBox`. This prevents attacks from damaging enemies through `AggroRange`, `AttackRange`, root movement colliders, or other utility triggers. `AnyHealthInHierarchy` exists only for objects that intentionally need the older broad behavior.

Self-damage is prevented when the target health transform is the owner or shares the same root.

## HurtBox

`HurtBox` is the canonical damage receiver for character combat. It lives on a trigger collider child and resolves its target `Health` from the parent hierarchy unless a specific `Health` reference is assigned.

Player, normal enemies, and FinalBoss should all expose damage through a `HurtBox`. `PlayerController.EnsureHurtBox()` adds the marker to prefab-authored or runtime-created player HurtBoxes. Range triggers remain separate and should not have `HurtBox`.

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

## FinalBoss Combat

- `FinalBossBehavior` triggers regular and charge attacks through `FinalBossAnimationBridge`.
- FinalBoss animation events should call methods on `FinalBossCombatBridge`.
- `FinalBossCombatBridge` forwards `EnableHitbox()` and `DisableHitbox()` to the regular attack hitbox.
- `FinalBossCombatBridge` forwards `EnableChargeHitbox()` and `DisableChargeHitbox()` to `ChargeHitbox`.
- `FinalBossCombatBridge.StartChargeDash()` forwards a charge animation event to `FinalBossBehavior.StartChargeDash()`, starting the scripted dash from the authored frame.
- `FinalBossBehavior` controls optional pooled sprite `ChargeTrail` presentation from the actual charge dash lifecycle; the trail has no collider and no damage behavior.
- `FinalBossCombatBridge` exposes `PlayAttack1SFX()`, `PlayAttack2SFX()`, and `PlayAttack3SFX()` for compatibility with existing FinalBoss attack clips.

## Known Risks

- `Health` currently mixes simulation state with animator presentation.
- `SendMessage("OnDeath")` is flexible but weakly typed.
- Hitbox target tracking uses a `List<Health>`; this is fine for small melee windows but could be replaced with pooled/set storage if many hitboxes are active.
- FinalBoss uses a dedicated `ChargeHitbox` with higher damage and animation-event methods for enabling/disabling the charge damage window.
