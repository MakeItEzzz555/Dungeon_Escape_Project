# Enemy AI System

Last audited: 2026-05-16

## Ownership

Enemy behavior is prefab-driven and built from an AI state machine, range trigger children, health, combat hitbox control, and animation event forwarding.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Enemy/Enemy.prefab` | Enemy prefab used in Level 2. |
| `Assets/Scripts/Enemy/EnemyAI.cs` | Enemy state machine and movement. |
| `Assets/Scripts/Enemy/IEnemyRangeReceiver.cs` | Shared aggro/attack range receiver contract. |
| `Assets/Scripts/Enemy/EnemyRangeTrigger.cs` | Aggro and attack range trigger children. |
| `Assets/Scripts/Enemy/EnemyCombat.cs` | Enemy attack hitbox activation from animation events. |
| `Assets/Scripts/Enemy/EnemyAnimationEventForwarder.cs` | Visuals-layer animation event bridge. |
| `Assets/Scripts/Core/Health.cs` | Enemy HP/death. |
| `Assets/Scripts/Core/Hitbox.cs` | Enemy attack damage window. |

## States

`EnemyAI.EnemyState`:

- `IdleAtHome`
- `Chasing`
- `Attacking`
- `ReturningHome`
- `Dead`

## Runtime Flow

1. `EnemyAI.Awake()` caches animator, health, rigidbody, home position, and player target.
2. `EnemyRangeTrigger` children report range state to an `IEnemyRangeReceiver`.
3. `EnemyAI` implements `IEnemyRangeReceiver` and stores `playerInAggroRange` and `playerInAttackRange`.
4. `EnemyAI.Update()` chooses a state.
5. Chasing moves toward the player.
6. Attacking stops movement and triggers attack on cooldown.
7. Leaving aggro returns enemy to home.
8. Dead state stops walking.

Active enemy deaths are also counted by the run results flow. `HUDManager` counts active `EnemyAI` instances at attempt start, subscribes to their `Health.OnDied` events, and reports `Enemies Killed` on `RunResultsPanel`. If no active enemies exist, results display `Enemies Killed: N/A`.

## Range Detection

`EnemyRangeTrigger.IsPlayer()` returns true when:

- The collider tag is `Player`.
- The collider root tag is `Player`.
- A `PlayerController` exists in the collider parent hierarchy.

This supports both root player colliders and child HurtBox trigger colliders.

`EnemyRangeTrigger` resolves an optional `rangeReceiverOverride` or falls back to the nearest parent `IEnemyRangeReceiver`. This keeps normal enemies compatible while allowing FinalBoss to reuse the same range trigger children without requiring `EnemyAI`.

## Animation And Combat

- Animator bool: `IsWalking`.
- Animator trigger: `Attack`.
- Enemy visuals may contain the Animator.
- `EnemyAnimationEventForwarder` should live on the visuals object that receives animation events.
- Forwarder calls parent `EnemyCombat`.

## Known Risks

- `EnemyRangeTrigger.OnTriggerStay2D()` logs every stay event and can flood the console.
- `EnemyAI` finds player only in `Awake`; if the player is replaced later, enemies may need retargeting.
- Movement uses `Time.deltaTime` with `Rigidbody2D.MovePosition`, not `FixedUpdate`.
- FinalBoss phase and charge behavior remain outside generic `EnemyAI`; range detection uses `IEnemyRangeReceiver` so `EnemyRangeTrigger` can report to both normal enemies and FinalBoss behavior.
