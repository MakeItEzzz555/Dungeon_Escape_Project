# Player System

Last audited: 2026-05-21

## Ownership

The player system controls movement, animation bridging, combat input, fall/death handling, footstep audio, pause camera following, and sprite sorting.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Player/Player 1.prefab` | Player prefab used in Level 1 and Level 2. |
| `Assets/Scripts/Player/PlayerController.cs` | Movement, lock state, idle timer, footstep calls, fall/death respawn transition delegation, HurtBox creation. |
| `Assets/Scripts/Player/PlayerAnimator.cs` | Animator parameter bridge and failure-state interruption helper. |
| `Assets/Scripts/Player/PlayerDash.cs` | Left Shift dash input, dash movement, cooldown, wall stop, stuck guard, and dash trail ownership. |
| `Assets/Scripts/Player/PlayerCombat.cs` | Attack input, attack animation trigger, sword hitbox events, attack cancellation, attack SFX. |
| `Assets/Scripts/Player/PlayerChargeAttack.cs` | Scene-local unlocked charge dash attack input, movement lock, hitbox window, cooldown, trail, and fallback safety. |
| `Assets/Scripts/FollowPlayer.cs` | Camera follow and pause menu toggle. |
| `Assets/Scripts/YSort.cs` | Sprite sorting from world Y. |

## Runtime State

| State | Owner | Notes |
|:------|:------|:------|
| `isDead` | `PlayerController` | Blocks movement/input and starts death respawn transition. |
| `isFalling` | `PlayerController` | Blocks movement/input and starts fall respawn transition. |
| `canMove` | `PlayerController` | Used by scene transitions and pause-like locks. |
| `abilityMovementLocked` | `PlayerController` | Temporary gameplay lock used while `PlayerChargeAttack` owns dash movement. |
| `isDashing` | `PlayerDash` | Blocks normal player control through `abilityMovementLocked` while dash movement is active. |
| `spawnGracePeriod` | `PlayerController` | Prevents instant fall-zone trigger after scene load. |
| `IsInStandBy` | `PlayerAnimator` / Animator | Idle animation state. |

## Movement

- Movement uses `Input.GetAxisRaw("Horizontal")` and `Input.GetAxisRaw("Vertical")`.
- Only one cardinal axis is selected at a time.
- Movement applies `Rigidbody2D.linearVelocity` in `FixedUpdate`.
- `Rigidbody2D.gravityScale` is forced to `0`.
- Rotation is frozen.
- `PlayerDash` uses `Rigidbody2D.MovePosition` during its own `FixedUpdate` while `PlayerController` input movement is locked.
- `PlayerDash` resolves wall stops through `Rigidbody2D.Cast` against the configured solid layer mask with trigger colliders ignored.

## Animation

`PlayerAnimator` writes:

- `MoveX`
- `MoveY`
- `LastMoveX`
- `LastMoveY`
- `IsMoving`
- `LastTimeMove`
- `IsInStandBy`
- `IsFalling`
- `Attack`
- `ChargeAttack`
- `IsDead`
- `IsDashing` if that optional Animator bool exists
- `Die`

`PlayerAnimator.OnStandByAnimationEnd()` forwards the animation event back to `PlayerController.OnStandByAnimationEnd()`.

`PlayerAnimator.InterruptToFallDive()` and `InterruptToDeath()` clear action triggers and force the saved failure animation state on layer 0, so attack and charge states cannot visually block fall/death presentation.

## HurtBox

`PlayerController.EnsureHurtBox()` creates a child named `HurtBox` at runtime if missing. It adds a trigger `BoxCollider2D` sized approximately `0.8 x 1.0`.

This coexists with `ADD_PLAYER_HURTBOX_INSTRUCTIONS.md`, which documents a prefab-authored HurtBox. Future combat work should choose one ownership model.

## Death And Fall

- `Health.Die()` calls `PlayerController.StartDeathSequence()` for player objects.
- `SpikeTrapFSM` damages player `Health`; lethal trap damage reaches `Health.Die()` and starts death sequence.
- `FallZone` calls `Health.DepleteHealth()` to wipe HP presentation, then calls `PlayerController.StartFallSequence()`.
- `StartFallSequence()` is allowed to override `abilityMovementLocked` from `PlayerChargeAttack`; it cancels regular attack and charge attack state before forcing `Fall_Dive`.
- `StartDeathSequence()` also cancels regular attack and charge attack state before forcing death presentation.
- Active `PlayerDash` is cancelled by fall/death before failure presentation starts.
- Both fall and death delegate to `SceneTransitionManager.BeginRespawnTransition()` after setting the correct failure animation state.
- The respawn transition shows the configured animation read time, zooms in on the player, fades to black, shows `RunResultsPanel` in failure mode, and waits for the player to press `Respawn`.
- Pressing `Respawn` reloads the active scene, fades from black, zooms out, and restores control.
- If `SceneTransitionManager` is missing, `PlayerController` logs an error and does not auto-reload because death/fall respawn must be player-confirmed.
- `SceneTransitionManager` calls `ResetState()` and `SetControlEnabled(false)` during scene transitions.

## FallZone Directional Nudge

- The player system owns the last valid cardinal movement direction as gameplay state.
- `PlayerAnimator` mirrors direction into animation parameters, but it is not the gameplay source of truth for `FallZoneDirectionalNudge`.
- When a `FallZone` starts the fall sequence, `IsFalling` and the short glide begin together after normal movement has been locked.

## Known Risks

- `PlayerController` is a high-responsibility class.
- Footstep audio is called directly through `AudioManager`.
- Gameplay input is legacy `Input`.
- `ResetIdleTimer()` logs every call and can be noisy.
