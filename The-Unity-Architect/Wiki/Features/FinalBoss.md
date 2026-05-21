# FinalBoss

Last updated: 2026-05-21

## 1. Feature Summary

| Field | Value |
|:------|:------|
| Name | FinalBoss |
| Objective | Add a boss enemy that reuses the existing enemy combat foundation while introducing a readable second phase and a high-damage charge attack. |
| Scope | This feature covers FinalBoss behavior, animation contract, hitbox ownership, range detection, phase transition, charge movement, charge trail presentation, boss HP HUD, optional death loot, optional completion results, and Inspector tuning. It does not cover arena scripting, cutscenes, or new player abilities. |

## 2. Business Rules

### Main Flow

1. FinalBoss starts in phase one and behaves like an enemy combatant: idle at home, chase when the player enters aggro range, attack when the player enters attack range, return home when aggro is lost, and die when health reaches zero.
2. FinalBoss tracks current health through the shared health component.
3. When health reaches or falls below `PhaseTwoHealthThreshold`, default `0.5`, `PhaseTwo` unlocks permanently.
4. During `PhaseTwo`, FinalBoss keeps the regular attack but also gains `ChargeAttack`.
5. `ChargeAttack` has its own longer cooldown. When it is ready, it takes priority over the regular attack.
6. At charge start, FinalBoss captures `ChargeDirection` from itself to the player's current position.
7. FinalBoss moves in that fixed straight line for a short tuned dash.
8. `ChargeHitbox` is enabled for the full dash movement, disabled during wind-up and `ChargeRecovery`, and can damage each target only once per charge activation.
9. `ChargeTrail` samples the active boss animation frame into 3-4 pooled sprite afterimages during the active dash movement and has no collider, no damage, and no gameplay authority.
10. If FinalBoss hits a blocking obstacle during the charge, `ChargeObstacleImpact` stops the dash immediately and enters `ChargeRecovery`.
11. After the dash ends, FinalBoss enters `ChargeRecovery` before resuming chase or attack decisions.
12. When aggro activates, `FinalBossBehavior` asks `HUDManager` to show `HUD_HP_FinalBoss` bound to the boss `Health`.
13. When aggro deactivates, FinalBoss dies, or a modal results view opens, `HUD_HP_FinalBoss` is hidden.
14. On death, FinalBoss can optionally spawn a configured loot prefab and start the completion-mode `RunResultsPanel` flow to `Main Menu`.

### States

| State | Purpose |
|:------|:--------|
| IdleAtHome | Wait at home position with no active target pressure. |
| Chasing | Move toward the player while aggro is active and attack range is not satisfied. |
| RegularAttacking | Stop movement and play the regular attack animation. |
| ChargeWindUp | Commit to `ChargeAttack`, face the captured direction, and prepare the dash. |
| Charging | Move along `ChargeDirection` with `ChargeHitbox` active. |
| ChargeRecovery | Lock chase and attack decisions briefly after charge completion or obstacle impact. |
| ReturningHome | Return to the home position after aggro is lost. |
| Dead | Stop movement, disable attacks, and play death presentation. |

### Failure Conditions

| Condition | Result |
|:----------|:-------|
| Health reaches zero | FinalBoss enters `Dead` and stops behavior decisions. |
| Health reaches zero with death rewards enabled | Death rewards run once: boss HUD hides, optional loot spawns, and completion results begin. |
| Player leaves aggro range outside committed attack states | FinalBoss returns home. |
| Blocking obstacle is hit during charge | Charge stops and enters `ChargeRecovery`. |
| Player damages FinalBoss during active charge | Damage applies, but `ChargeAttack` is not cancelled. |
| Player damages FinalBoss during wind-up | Damage applies and may show feedback, but the committed charge is not cancelled. |

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| FinalBoss root | Owns boss identity, physics body, health, behavior, and child references. |
| FinalBossBehavior | Owns phases, attack selection, charge movement, cooldowns, recovery locks, home position, and death state. |
| FinalBossCombatBridge | Exposes animation-event methods for regular and charge hitbox windows. |
| FinalBossAnimationBridge | Writes directional animator parameters and triggers boss animation paths. |
| Shared Health | Supplies current health, death state, and death events. |
| Shared Hitbox | Applies damage during enabled attack windows. |
| IEnemyRangeReceiver | Shared range receiver contract implemented by `FinalBossBehavior`. |
| EnemyRangeTrigger-compatible range detection | Reports aggro and attack range state to the boss behavior owner. |
| Visuals child | Hosts Animator, sprite presentation, and animation-event bridge. |
| ChargeTrail | Pooled sprite afterimage presentation controlled by `FinalBossBehavior` during active charge movement only. |
| HUDManager | Shows, binds, updates, and hides `HUD_HP_FinalBoss` while boss aggro is active. |
| SceneTransitionManager | Starts checkpoint-style completion results when FinalBoss death results are enabled. |

### Prefab Hierarchy

```text
FinalBoss
  AggroRange
  AttackRange
  AttackHitbox
  ChargeHitbox
  HurtBox
  Visuals
```

### Configuration

FinalBoss tuning must be exposed through serialized Inspector fields. Required settings include:

| Setting | Purpose |
|:--------|:--------|
| Move speed | Chasing and returning speed. |
| Regular attack cooldown | Delay between normal attacks. |
| Charge attack cooldown | Longer independent cooldown for `ChargeAttack`. |
| Phase two health threshold | Normalized default `0.5`. |
| Charge speed | Dash movement speed. |
| Charge distance or duration | Maximum dash length. |
| Charge animation trigger delay | Delay before sending the `ChargeAttack` Animator trigger, so the first charge is not swallowed by idle/hurt transition timing. |
| Minimum charge dash delay after animation trigger | Minimum time after the charge trigger before a dash event or fallback is allowed to move the boss. |
| Start charge dash from animation event | Whether `StartChargeDash` is expected to start the dash from the authored animation event. |
| Charge dash event fallback duration | Safety delay that starts the dash if the animation event is missing; set to `0` or less to disable fallback. |
| Charge recovery duration | Post-charge lockout. |
| Charge trail reference | Optional `ChargeTrail` component enabled only during active charge dash movement. |
| Show boss HUD on aggro | Enables `HUD_HP_FinalBoss` while aggro is active. |
| Loot prefab | Optional prefab spawned once when FinalBoss dies. |
| Loot spawn point | Optional transform used for loot spawn position; falls back to FinalBoss transform. |
| Show run results on death | Starts completion-mode `RunResultsPanel` when FinalBoss dies. |
| Results target scene | Scene loaded by Continue after boss death results; default is `Main Menu`. |
| Results zoom target | Optional transform used for the death-completion zoom; falls back to FinalBoss transform. |
| Clear charge trail on dash start | Clears old trail points before a new dash begins. |
| Clear charge trail on dash stop | Optional instant clear when the dash stops; leave false if the trail should fade out naturally. |
| Return stop distance | Distance considered returned home. |
| Attack range fallback | Optional fallback distance only when the prefab has no working `AttackRange` trigger child. In the normal prefab setup, `AttackRange` is authoritative and the boss keeps chasing until that trigger reports the player inside. |
| Obstacle mask | Blocking layers that stop `ChargeAttack`. |
| Regular attack hitbox reference | Lower-damage hitbox. |
| Charge hitbox reference | Higher-damage hitbox. |
| Animator reference | Visual Animator controlled by the boss animation bridge. |
| Visuals reference | Child presentation transform. |
| Animator parameter names | `IsWalking`, `Attack`, `ChargeAttack`, movement/facing floats, hurt/death parameters. |

### Animator Contract

| Parameter | Type | Purpose |
|:----------|:-----|:--------|
| MoveX | Float | Current movement direction X. |
| MoveY | Float | Current movement direction Y. |
| LastMoveX | Float | Last facing direction X. |
| LastMoveY | Float | Last facing direction Y. |
| IsWalking | Bool | Walking/chasing/returning blend transition. |
| Attack | Trigger | Regular attack animation path. |
| ChargeAttack | Trigger | Charge attack animation path. |
| Hurt | Trigger | Hurt presentation. |
| IsDead | Bool | Death state presentation. |
| Die | Trigger | Death animation path if used by the controller. |

### Animation Events

| Event Method | Purpose |
|:-------------|:--------|
| EnableHitbox | Enable the regular attack hitbox. |
| DisableHitbox | Disable the regular attack hitbox. |
| EnableChargeHitbox | Enable `ChargeHitbox` for the active dash. |
| DisableChargeHitbox | Disable `ChargeHitbox`. |
| StartChargeDash | Start the scripted charge dash from the charge animation event frame. Also supported aliases: `BeginChargeDash`, `InvokeChargeDash`. |
| PlayAttack1SFX | Compatibility event for existing FinalBoss attack clips that currently reuse player-style attack SFX event names. |
| PlayAttack2SFX | Compatibility event for existing FinalBoss attack clips that currently reuse player-style attack SFX event names. |
| PlayAttack3SFX | Compatibility event for existing FinalBoss attack clips that currently reuse player-style attack SFX event names. |

### Dependencies

| Dependency | Use |
|:-----------|:----|
| Health | Phase threshold, death state, damage response. |
| Hitbox | Regular and charge damage windows. |
| IEnemyRangeReceiver / EnemyRangeTrigger-compatible detection | Aggro and attack range reporting. |
| Animator | Directional movement, regular attack, charge attack, hurt, and death presentation. |
| Rigidbody2D | Kinematic-style movement and obstacle-aware charge motion. |
| AudioManager | Optional swing or charge SFX through animation events. |

## 4. Edge Cases and Exceptions

| Edge Case | Decision |
|:----------|:---------|
| Phase threshold is reached during regular attack | `PhaseTwo` unlocks permanently, but the current committed attack does not need to cancel. |
| Charge cooldown is ready while regular attack is also available | `ChargeAttack` takes priority during `PhaseTwo`. |
| Player moves after charge begins | FinalBoss does not retarget; `ChargeDirection` remains fixed. |
| Player overlaps FinalBoss during charge | `ChargeHitbox` can apply damage once for that charge activation. |
| Charge hits wall or blocking obstacle | Charge stops immediately and enters `ChargeRecovery`. |
| Player damages boss during charge | Damage applies, but active charge is not interrupted. |
| Animation event is missed | Hitboxes must fail closed at state exit or recovery so they do not remain enabled. |
| Player leaves aggro range during committed charge | Charge completes or stops through obstacle impact before recovery/return decisions resume. |
| Regular attack transition is blocked by movement parameters | The `Attack` transition must be gated by `Attack` and death state only, not by `MoveX` or `MoveY`, because `FinalBossBehavior` stops walking before triggering the regular attack. |
| Hurt direction appears wrong | `Health` must write the same directional parameters read by `Hurt Tree`; for FinalBoss this means `hurtXParam = LastMoveX` and `hurtYParam = LastMoveY`, not legacy `LastTimeX` / `LastTimeY`. |
| Boss attacks before entering `AttackRange` | `FinalBossBehavior` treats the `AttackRange` child trigger as authoritative when it exists and is enabled. `attackDistanceFallback` is reserved for incomplete prefab setups where that trigger is missing or disabled. |

## 5. Out Of Scope

- Boss arena gates or camera locks.
- Cutscene intros or death cinematics.
- New player abilities.
- Multiple boss phases beyond `PhaseTwo`.
- Projectile attacks. `ChargeTrail` uses a small local pool because it is VFX.
- Reworking all enemies into a generalized boss framework.

## Implementation Status

Script support exists for `IEnemyRangeReceiver`, `FinalBossBehavior`, `FinalBossAnimationBridge`, `FinalBossCombatBridge`, boss HP HUD binding, optional FinalBoss loot spawning, and FinalBoss death completion results. `HUDManager` creates a clean `HUD_HP_FinalBoss` panel at runtime if the prefab does not already contain the boss panel, then populates exact boss-health image slots from the player HP image template. FinalBoss charge wind-up schedules the `ChargeAttack` Animator trigger before dash movement is allowed, preventing the first charge from dashing while stuck in idle or hurt presentation.
