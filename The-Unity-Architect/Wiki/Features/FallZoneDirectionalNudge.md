# FallZone Directional Nudge

## 1. Feature Summary

| Field | Value |
|:------|:------|
| Name | FallZone Directional Nudge |
| Objective | Make `FallZone` deaths feel smoother by gliding the player a short distance into the hazard in the last valid cardinal movement direction while the fall animation begins. |
| Scope | Applies only to `FallZone` death triggers. It does not apply to spike traps, rolling traps, enemy damage, standard death animation, checkpoints, or respawn rules. |

## 2. Business Rules

### Main Flow

1. Player moves in a cardinal direction: left, right, up, or down.
2. The player system remembers the last valid cardinal movement direction.
3. Player enters a `FallZone` trigger.
4. Existing fall-zone guards still run: transition guard, spawn protection, and duplicate-trigger guard.
5. The fall sequence locks normal movement immediately.
6. `IsFalling` becomes true and the `Fall_Dive` animation starts.
7. At the same time, `FallZoneDirectionalNudge` glides the player a short distance in the last valid cardinal movement direction.
8. Existing fall respawn behavior continues through `DeathResultsFlow` and `RespawnTransition`.

### States

| State | Meaning |
|:------|:--------|
| Idle | Player is under normal movement control and no fall sequence is active. |
| FallZoneTriggered | A `FallZone` has accepted the player trigger and will start the fall sequence once. |
| FallingWithNudge | Player movement is locked, `IsFalling` is true, and the short directional glide is active. |
| FallingNoNudge | Player movement is locked and `IsFalling` is true, but the nudge cannot or should not run. |
| RespawnTransition | The existing death/fall recovery flow owns camera, fade, result panel, and scene reload. |

### Failure Conditions

- The nudge must not run when a scene transition is already active.
- The nudge must not run during player spawn protection.
- The nudge must not run for trap hazards or enemy death.
- The nudge must not allow player input to continue after the fall sequence starts.
- Duplicate `FallZone` triggers must not create multiple nudges or multiple respawn transitions.

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| Player State Owner | Owns the last valid cardinal movement direction, movement lock, fall state, and nudge timing. |
| Player Animation Bridge | Mirrors movement and fall state into animation parameters without becoming the source of gameplay truth. |
| FallZone Trigger | Accepts the player trigger, applies existing guards, depletes health presentation, and requests the fall sequence. |
| Respawn Transition Orchestrator | Continues to own camera zoom, fade, result panel, and active scene reload after the fall animation read time. |

### Configuration

| Value | Purpose |
|:------|:--------|
| Nudge distance | Fixed world distance the player glides after entering a `FallZone`. |
| Nudge duration | Short time window for the glide, tuned to the opening frames of `Fall_Dive`. |
| Missing-direction fallback | Down direction, matching the player's default idle-down presentation. |

### Dependencies

- Player movement state.
- Player fall animation state.
- `FallZone` trigger flow.
- `Health` depletion for immediate HUD update.
- Existing `RespawnTransition` and `DeathResultsFlow`.

## 4. Edge Cases and Exceptions

- The last valid cardinal movement direction is authoritative, not collider contact geometry.
- If no last valid movement direction exists, the nudge falls back to down.
- The nudge starts at the same time as `IsFalling = true`.
- The nudge runs after normal player movement has been locked, so input and physics do not fight the fall sequence.
- Current water hazards are `FallZone` instances, but the rule is feature-wide: all future `FallZone` hazards get the same behavior.
- Trap hazards remain outside this feature even if they also kill the player.

## 5. Out of Scope

- Per-hazard nudge distances or directions.
- Contact-point or collider-side detection.
- Trap hazard movement offsets.
- Respawn transition redesign.
- Health, damage, or HUD redesign.
- New water-specific hazard type.
- New animation clips or animation controller restructuring.

## Approval

Approved by the user on 2026-05-20 and implemented as the active `FallZone` fall-entry behavior.

## Implementation Notes

- Implemented through player-owned last valid cardinal movement state.
- `PlayerAnimator` mirrors the direction into `LastMoveX` and `LastMoveY` but does not own gameplay direction.
- `FallZoneDirectionalNudge` starts with `IsFalling = true` and runs as a short `Rigidbody2D` position glide while normal movement is locked.
- Default tuning is owned by the player: fixed nudge distance and fixed nudge duration.
