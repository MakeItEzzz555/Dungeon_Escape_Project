# Traps And Hazards System

Last audited: 2026-05-19

## Ownership

Hazards damage, kill, or reset the player through trap state machines and fall zones.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Interactables/SpikeTrapFSM.cs` | Spike/rolling trap state machine and damage window. |
| `Assets/Scripts/Interactables/FallZone.cs` | Trigger zone that starts player fall sequence. |
| `Assets/Prefabs/Traps/Spike Trap.prefab` | Spike trap prefab used in Level 1 and Level 2. |
| `Assets/Prefabs/Traps/RollingTrap.prefab` | Rolling trap prefab; script references exist but no scene prefab references were found in the latest audit. |

## SpikeTrapFSM States

- `Disabled`
- `Armed`
- `PlayerDetected`
- `Active`
- `Triggered`
- `CoolingDown`

## SpikeTrap Runtime Flow

1. `Awake()` sets `trapEnabled` from `startEnabled`.
2. Animator speed is `1` when enabled and `0` when disabled.
3. Player trigger enter stores `detectedPlayer`.
4. Animation event `EnableDamageWindow()` opens the damage window.
5. If a detected player exists while the damage window is open, `SpikeTrapFSM` applies `trapDamageAmount` to player `Health`.
6. Each damage window can damage the player once; closing the window resets the per-window hit guard.
7. Animation event `DisableDamageWindow()` closes the damage window.
8. `SetTrapActive(bool)` supports lever-driven enable/disable.

## FallZone Runtime Flow

1. Trigger enter requires tag `Player`.
2. It ignores activation during `SceneTransitionManager.IsTransitioning`.
3. It ignores activation while `PlayerController.IsSpawnProtected()` is true.
4. It triggers only once per enable.
5. It calls `Health.DepleteHealth()` to hide all HP images immediately.
6. It calls `PlayerController.StartFallSequence()`.

## FallZone Directional Nudge

- `FallZoneDirectionalNudge` applies to all `FallZone` death triggers.
- It does not apply to `SpikeTrapFSM`, rolling traps, enemy damage, or standard death triggers.
- The player glides briefly in the last valid cardinal movement direction while `Fall_Dive` begins.
- The player system, not the animation bridge, owns the gameplay direction used by the nudge.

## Scene Wiring

- `FallZone` is directly referenced in `Level 2.unity`.
- `SpikeTrapFSM` is referenced by `Spike Trap.prefab` and `RollingTrap.prefab`.
- `LeverResponder` is also referenced by trap prefabs, indicating traps can be lever-controlled.

## Known Risks

- `SpikeTrapFSM` contains several non-ASCII comment artifacts from prior encoding issues; keep future docs/code ASCII.
- `ResetTrap()` sets `Time.timeScale = 1f`, which is a global side effect inside a trap component.
- Trap damage is routed through `Health`, but trap detection still stores `PlayerController` directly.
