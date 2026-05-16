# Player System

Last audited: 2026-05-16

## Ownership

The player system controls movement, animation bridging, combat input, fall/death handling, footstep audio, pause camera following, and sprite sorting.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Player/Player 1.prefab` | Player prefab used in Level 1 and Level 2. |
| `Assets/Scripts/Player/PlayerController.cs` | Movement, lock state, idle timer, footstep calls, fall/death/reload flow, HurtBox creation. |
| `Assets/Scripts/Player/PlayerAnimator.cs` | Animator parameter bridge. |
| `Assets/Scripts/Player/PlayerCombat.cs` | Attack input, attack animation trigger, sword hitbox events, attack SFX. |
| `Assets/Scripts/FollowPlayer.cs` | Camera follow and pause menu toggle. |
| `Assets/Scripts/YSort.cs` | Sprite sorting from world Y. |

## Runtime State

| State | Owner | Notes |
|:------|:------|:------|
| `isDead` | `PlayerController` | Blocks movement/input and starts death reload. |
| `isFalling` | `PlayerController` | Blocks movement/input and starts fall reload. |
| `canMove` | `PlayerController` | Used by scene transitions and pause-like locks. |
| `spawnGracePeriod` | `PlayerController` | Prevents instant fall-zone trigger after scene load. |
| `IsInStandBy` | `PlayerAnimator` / Animator | Idle animation state. |

## Movement

- Movement uses `Input.GetAxisRaw("Horizontal")` and `Input.GetAxisRaw("Vertical")`.
- Only one cardinal axis is selected at a time.
- Movement applies `Rigidbody2D.linearVelocity` in `FixedUpdate`.
- `Rigidbody2D.gravityScale` is forced to `0`.
- Rotation is frozen.

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
- `IsDead`
- `Die`

`PlayerAnimator.OnStandByAnimationEnd()` forwards the animation event back to `PlayerController.OnStandByAnimationEnd()`.

## HurtBox

`PlayerController.EnsureHurtBox()` creates a child named `HurtBox` at runtime if missing. It adds a trigger `BoxCollider2D` sized approximately `0.8 x 1.0`.

This coexists with `ADD_PLAYER_HURTBOX_INSTRUCTIONS.md`, which documents a prefab-authored HurtBox. Future combat work should choose one ownership model.

## Death And Fall

- `Health.Die()` calls `PlayerController.StartDeathSequence()` for player objects.
- `SpikeTrapFSM` calls `PlayerController.StartDeathSequence()` directly.
- `FallZone` calls `PlayerController.StartFallSequence()`.
- Both fall and death eventually reload the active scene.
- `SceneTransitionManager` calls `ResetState()` and `SetControlEnabled(false)` during scene transitions.

## Known Risks

- `PlayerController` is a high-responsibility class.
- Footstep audio is called directly through `AudioManager`.
- Gameplay input is legacy `Input`.
- `ResetIdleTimer()` logs every call and can be noisy.
