# Player Dash

Last audited: 2026-05-21

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Let the player burst in any direction for a short distance to dodge enemies or cross gaps, with a brief cooldown to prevent spamming. |
| Input | Left Shift |
| Related systems | [Player](../Systems/Player.md), [Input](../Systems/Input.md) |

## Current Behavior

1. Pressing Left Shift while alive, not falling, not control-locked, and not already dashing starts `PlayerDash`.
2. The dash direction uses current cardinal movement input. If the player is standing still, `PlayerController.LastMoveDirection` is used instead.
3. The player moves at `dashSpeed` for up to `dashDuration`, capped by `dashDistance`.
4. Normal movement input, footsteps, idle logic, regular attack, and charge attack are suppressed while dashing through the existing `PlayerController` ability movement lock.
5. `dashCooldown` prevents consecutive dashes.
6. The dash is cancelled early if the player runs into a solid wall. Detection uses `Rigidbody2D.Cast` with `useTriggers = false`, so `FallZone` triggers still activate normally instead of being treated as dash blockers.
7. A secondary stuck guard ends the dash if the player barely moves for the configured duration.
8. Starting a fall or death sequence cancels any active dash immediately to prevent conflicts.
9. `ChargeTrail` is reused as the dash ghost trail so the player dash and charge attack share one pooled afterimage implementation.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Player/Player 1.prefab` | Player prefab with `PlayerDash` and reusable `ChargeTrail` components. |
| `Assets/Scripts/Player/PlayerDash.cs` | Dash input, cooldown, movement, wall detection, stuck guard, and trail control. |
| `Assets/Scripts/Player/PlayerController.cs` | Movement lock, last facing direction, fall/death cancellation, and reset integration. |
| `Assets/Scripts/Player/PlayerAnimator.cs` | Optional `SetDashingStatus(bool)` bridge for an `IsDashing` Animator bool if the controller later adds it. |
| `Assets/Scripts/Enemy/ChargeTrail.cs` | Reused pooled sprite afterimage presentation. |

## Key Inspector Fields (`PlayerDash`)

| Field | Default | Notes |
|:------|:--------|:------|
| `dashKey` | Left Shift | Rebindable in Inspector. |
| `dashSpeed` | 20 | World units per second during the burst. |
| `dashDuration` | 0.18 s | Time ceiling on burst length. Wall hits end it early. |
| `dashDistance` | 3.6 | Distance ceiling on burst length. |
| `dashCooldown` | 1 s | Minimum seconds between dashes. |
| `solidLayerMask` | auto | Auto-built from Default + Water layers in `Awake` if left at zero. Assign manually to override. |
| `obstacleSkinWidth` | 0.03 | Small collision offset to avoid clipping into blockers. |
| `stuckDistanceThreshold` | 0.01 | Minimum movement expected before the stuck timer counts. |
| `stuckTimeThreshold` | 0.08 s | Ends dash if movement remains below threshold this long. |

## Edge Cases

| Case | Expected Result |
|:-----|:----------------|
| Shift is pressed during charge attack | Ignored because charge owns the ability movement lock. |
| Q/F is pressed during dash | Ignored because dash owns the ability movement lock. |
| Player enters a fall zone during dash | Dash cancels and `Fall_Dive` owns presentation immediately. |
| Player dies during dash | Dash cancels and death presentation owns immediately. |
| Dash starts with no movement input | Last facing direction is used. |
| Dash would enter a solid collider | Dash stops at the collision edge. |

## Implementation Status

Script and prefab support is implemented. No required Animator Controller edit is needed. If a future dash animation is added, add an optional `IsDashing` bool parameter to the player Animator Controller; `PlayerAnimator.SetDashingStatus(bool)` will write it when present.
