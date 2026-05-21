# Sword Consumable And Player Charge Attack

Last updated: 2026-05-21

## 1. Feature Summary

| Field | Value |
|:------|:------|
| Name | Sword Consumable And Player Charge Attack |
| Objective | Let the player collect a sword upgrade that unlocks a FinalBoss-style charge dash attack for the current run. |
| Scope | Covers the sword pickup, scene-local ability unlock, `Q` input, charge attack animation contract, dash movement, charge hitbox timing, charge trail presentation, cooldown, and fallback safety. It does not cover animation clip creation, permanent save data, new UI meters, or new audio assets. |

## 2. Business Rules

### Main Flow

1. A `SwordConsumable` exists in the scene as a trigger collectible.
2. When the player collects it, `PlayerChargeAttack` unlocks for the current scene/run only.
3. The pickup is consumed and shows `ChargeAttackUnlockedFeedback`.
4. After unlock, pressing `Q` attempts to start `PlayerChargeAttack`.
5. The ability is rejected if the player is dead, falling, movement-locked, already charge attacking, or still on `PlayerChargeAttackCooldown`.
6. Starting the ability locks normal movement and regular `F` attack input.
7. The player animator receives `PlayerChargeAttackTrigger`, the `ChargeAttack` trigger.
8. The charge animation events drive gameplay timing through `PlayerChargeAttackAnimEvents`.
9. `StartDash` or `StartChargeDash` starts the actual dash in the player's last facing direction from the authored animation event frame.
10. `EnableChargeHitbox` opens the dedicated `ChargeHitbox` damage window.
11. `DisableChargeHitbox` closes the dedicated `ChargeHitbox` damage window.
12. `EndChargeAttack` releases the charge state after the authored animation/recovery timing.
13. While dashing, `ChargeTrail` may emit player afterimages using the player's sprite renderer.
14. If the dash hits a blocking obstacle, movement stops early, hitbox closes, trail stops, and recovery proceeds.
15. After the charge ends, `PlayerChargeAttackCooldown` must complete before another charge can start.

### States

| State | Purpose |
|:------|:--------|
| Locked | Ability is unavailable because `SwordConsumable` has not been collected in the current run. |
| Ready | Ability is unlocked and can start when the player presses `Q`. |
| ChargeWindUp | Player movement is locked and the charge animation path has started, but dash movement has not begun. |
| Charging | Player is moving in the captured facing direction, with optional `ChargeTrail` presentation and possible charge hitbox window. |
| ChargeRecovery | Dash movement has ended and player remains locked briefly until animation/recovery completes. |
| Cooldown | Ability is unlocked but cannot restart until `PlayerChargeAttackCooldown` expires. |

### Failure Conditions

| Condition | Result |
|:----------|:-------|
| Player presses `Q` before collecting `SwordConsumable` | No charge starts. |
| Player presses `Q` while already charging | Input is ignored; animation does not retrigger. |
| Player presses `Q` during cooldown | Input is ignored. |
| Player dies or falls during the charge | Charge ends, hitbox disables, trail stops, and the existing death/fall flow owns control. |
| Charge hits a blocking obstacle | Dash stops early and enters recovery. |
| Animation event is missing | `PlayerChargeAttackFallbacks` fail closed by starting/ending critical phases through serialized safety timings. |

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| `SwordConsumable` | Trigger collectible that unlocks `PlayerChargeAttack`, plays pickup feedback, disables its collider, and consumes itself. |
| `PlayerChargeAttack` | Owns unlock state, `Q` input, charge state, cooldown, dash movement, obstacle checks, charge hitbox control, and charge trail control. |
| `PlayerController` | Owns normal movement state and must expose a safe temporary movement lock path for charge attack ownership. |
| `PlayerAnimator` | Animation bridge that starts the `ChargeAttack` animation path and preserves the player's last facing direction. |
| `ChargeHitbox` | Dedicated player charge damage window, separate from the regular sword hitbox. |
| `ChargeTrail` | Optional presentation-only trail reused from the FinalBoss charge implementation. |
| `Health` | Prevents charge start when the player is dead and terminates safely if death occurs. |
| `HUDManager` | Shows unlock feedback. |

### Configuration

| Setting | Purpose |
|:--------|:--------|
| Charge key | Default `Q`. |
| Charge attack cooldown | Default about `1.5` seconds. |
| Charge speed | Dash movement speed, tuned similarly to FinalBoss but adjusted for player feel. |
| Charge distance | Maximum dash distance. |
| Charge recovery duration | Post-dash movement lock duration. |
| Obstacle mask | Layers that stop the dash early. |
| Obstacle skin width | Small collision offset to prevent clipping into blockers. |
| Charge hitbox reference | Dedicated hitbox enabled only during charge damage windows. |
| Charge trail reference | Optional `ChargeTrail` on the player. |
| Clear charge trail on dash start | Clears stale trail images before each charge. |
| Clear charge trail on dash stop | Optional instant clear when charge stops. |
| Dash event fallback delay | Optional opt-in dash failsafe. Keep `0` so `StartDash`/`StartChargeDash` animation events own dash timing. |
| Max charge duration fallback | Ends charge if `EndChargeAttack` animation event is missing. |
| Unlock feedback message | Default `Charge Attack Unlocked`. |
| Pickup collection state | Optional sword pickup animation state name. |
| Pickup destroy delay | Delay before destroying the consumed sword object. |

### Animator Contract

| Parameter | Type | Purpose |
|:----------|:-----|:--------|
| `ChargeAttack` | Trigger | Starts the player charge attack animation path. |
| `LastMoveX` | Float | Existing facing direction X used to aim the charge when no movement input is active. |
| `LastMoveY` | Float | Existing facing direction Y used to aim the charge when no movement input is active. |
| `IsMoving` | Bool | Existing movement flag should be false while charge attack owns movement. |
| `IsFalling` | Bool | Existing fall state remains authoritative over charge attack. |
| `IsDead` | Bool | Existing death state remains authoritative over charge attack. |

### Animation Events

| Event Method | Purpose |
|:-------------|:--------|
| `StartDash` | Starts movement along the captured charge direction from the authored event frame. |
| `StartChargeDash` | Alias for `StartDash`, kept for consistency with FinalBoss naming. |
| `EnableChargeHitbox` | Enables the dedicated `ChargeHitbox`. |
| `DisableChargeHitbox` | Disables the dedicated `ChargeHitbox`. |
| `EndChargeAttack` | Ends recovery/charge lock and starts cooldown. |
| `PlayAttack1SFX` | Optional compatibility with existing player attack SFX. |
| `PlayAttack2SFX` | Optional compatibility with existing player attack SFX. |
| `PlayAttack3SFX` | Optional compatibility with existing player attack SFX. |

### Dependencies

| Dependency | Use |
|:-----------|:----|
| Player movement system | Locks normal movement while charge attack owns dash movement. |
| Shared combat system | Uses `Hitbox` and `HurtBox` targeting rules. |
| FinalBoss charge feature | Provides the reference behavior for dash, obstacle impact, and `ChargeTrail` presentation. |
| HUD and audio feedback | Confirms ability unlock on pickup. |

## 4. Edge Cases and Exceptions

| Edge Case | Decision |
|:----------|:---------|
| Q is spammed during charge animation | Script-side `PlayerChargeAttack` lock ignores input until the ability finishes. |
| Animator trigger is pressed while already in charge | Trigger is not sent because gameplay lock rejects the input first. |
| Bool-driven Animator state could get stuck | Use `ChargeAttack` trigger plus script lock, not Animator-only bool gating. |
| Normal attack is pressed during charge | Regular `F` attack input is blocked while the charge owns movement. |
| Dash begins before animation event is wired | Fallback delay starts dash for testing, then authored event should replace timing. |
| Hitbox event is missed | Charge end disables `ChargeHitbox` as a fail-closed safeguard. |
| End event is missed | Max duration fallback unlocks the player and starts cooldown. |
| Player dies/falls during charge | Existing death/fall state cancels charge immediately, disables charge presentation/damage, clears movement ownership, and forces the failure animation state. |
| Player has not moved yet | Default facing direction is down, matching current player movement defaults. |

## 5. Out Of Scope

- Creating the animation clips.
- Permanent ability save data.
- Charge stamina, mana, ammo, or UI cooldown meter.
- New input system migration.
- New VFX system.
- New audio clips.
- Reworking FinalBoss charge behavior.
- Multi-charge combo chains.

## Implementation Status

Script support was implemented on 2026-05-21:

- `SwordConsumable` unlocks `PlayerChargeAttack`, plays pickup feedback, disables its trigger, and consumes itself.
- `PlayerChargeAttack` owns `Q` input, scene-local unlock state, script-side reentry lock, cooldown, dash movement, obstacle stop, dedicated `ChargeHitbox`, optional `ChargeTrail`, and fallback safety.
- `PlayerController` exposes a charge-safe movement lock and last facing direction.
- `PlayerAnimator` exposes the `ChargeAttack` trigger path.
- `PlayerCombat` ignores regular `F` attack input while the player is control-locked.
- `PlayerAnimatorController` has the `ChargeAttack` trigger parameter.

Remaining Editor wiring:

1. Add `PlayerChargeAttack` to the player root or scene player instance.
2. Add a dedicated `ChargeHitbox` child with trigger collider and `Hitbox` owner set to the player root.
3. Add optional `ChargeTrail` to the player and assign the source sprite renderer if auto-resolution is not sufficient.
4. Build the `ChargeAttack` Animator state or blend tree using the authored charge clips.
5. Add Any State -> charge transition on `ChargeAttack`, guarded by `IsFalling == false` and `IsDead == false`.
6. Add animation events to each charge clip: `StartDash`, `EnableChargeHitbox`, `DisableChargeHitbox`, and `EndChargeAttack`. `StartChargeDash` is also supported if you prefer the FinalBoss naming.
7. Create the sword pickup prefab with `SwordConsumable` and a trigger `Collider2D`.
