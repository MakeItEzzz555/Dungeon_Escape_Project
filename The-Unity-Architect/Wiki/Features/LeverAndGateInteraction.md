# Lever And Gate Interaction

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Press `E` near levers to open/close gates or toggle trap-related targets. |
| Related system | [Interactables](../Systems/Interactables.md) |

## Current Behavior

1. Player enters lever trigger range.
2. Player presses `E`.
3. Lever toggles internal on/off state.
4. Lever animator bool `ON` updates.
5. Interact SFX plays.
6. Lever emits bool UnityEvent.
7. `LeverResponder` or directly wired targets react to the bool signal.
8. Gates can open/close through animator parameters and collider enable/disable events.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Interactables/LeverSwitch.prefab` | Lever prefab. |
| `Assets/Prefabs/Doors_Gates/MetalGateInter.prefab` | Interactive gate prefab. |
| `Assets/Scripts/Interactables/Lever_on.cs` | Lever trigger/input/toggle. |
| `Assets/Scripts/Interactables/LeverResponder.cs` | Bool signal adapter. |
| `Assets/Scripts/Interactables/InteractiveGate.cs` | Gate open/close and collider control. |

## Known Gaps

- UnityEvent wiring must be inspected in scenes/prefabs.
- There are legacy gate scripts still present; prefer `InteractiveGate` for future gates.
