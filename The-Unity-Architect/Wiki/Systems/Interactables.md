# Interactables System

Last audited: 2026-05-16

## Ownership

Interactables cover player-activated doors, levers, gates, and chests. Most use trigger-range detection plus the `E` key.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Interactables/Lever_on.cs` | Player-toggleable lever that emits a bool UnityEvent. |
| `Assets/Scripts/Interactables/LeverResponder.cs` | Converts lever bool signal into animator/gate/trap state changes. |
| `Assets/Scripts/Interactables/InteractiveGate.cs` | Robust gate controller for open/close animator state and physical collider. |
| `Assets/Scripts/Interactables/door_interaction.cs` | Legacy/simple player-toggleable door. |
| `Assets/Scripts/Interactables/open_chest.cs` | Player-opened chest with optional item reveal. |
| `Assets/Scripts/auto_gate.cs` | Legacy animation-event collider toggle for simple gates. |
| `Assets/Prefabs/Interactables/LeverSwitch.prefab` | Lever prefab. |
| `Assets/Prefabs/Doors_Gates/MetalGateInter.prefab` | Interactive gate prefab. |
| `Assets/Prefabs/Doors_Gates/Door.prefab` | Door prefab. |
| `Assets/Prefabs/Interactables/chest_yellow_blue.prefab` | Chest prefab. |

## Lever Flow

1. `Lever_On` detects player range through trigger enter/exit.
2. When in range, pressing `E` toggles internal `isOn`.
3. Animator bool `ON` is set.
4. `AudioManager.PlayInteract()` plays.
5. `onToggle` invokes a bool signal.
6. `LeverResponder.Respond(bool signal)` receives the signal through UnityEvent wiring.

## LeverResponder Targets

`LeverResponder` can drive:

- Animator trigger `parameterName`.
- Animator bool `parameterName`.
- Two-trigger setup using `parameterName` and `offParameterName`.
- `InteractiveGate.SetState(signal)` when `InteractiveGate` exists on the same object.
- `SpikeTrapFSM.SetTrapActive(bool)` when wired through UnityEvent or serialized scene connection.

`LeverResponder` is referenced by scenes and by these prefabs:

- `MetalGateInter.prefab`
- `Spike Trap.prefab`
- `RollingTrap.prefab`

## InteractiveGate

`InteractiveGate` owns:

- Physical collider.
- Animator.
- Open parameter.
- Close parameter.

It exposes:

- `Open()`
- `Close()`
- `SetState(bool isOpen)`
- `DisableCollider()`
- `EnableCollider()`

`DisableCollider()` and `EnableCollider()` are intended for animation events when the gate becomes passable or solid.

## Doors And Chests

`door_interaction`:

- Uses trigger range plus `E`.
- Toggles open/closed.
- Drives `Open` and `Close` animator triggers.
- Toggles a physical collider.
- Plays interact SFX.

`open_chest`:

- Uses trigger range plus `E`.
- Opens once.
- Drives an `Open` animator trigger.
- Activates optional `itemInside`.
- Plays interact SFX.
- Shows `Chest Opened`.

## Known Risks

- There are both robust and legacy gate/door scripts. Prefer `InteractiveGate` for new work.
- Interactable scripts use direct `Input.GetKeyDown`.
- UnityEvent wiring lives in scene/prefab Inspector data, so future debugging must inspect scene/prefab connections, not only scripts.
