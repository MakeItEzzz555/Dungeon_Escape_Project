# Player HP HUD

Last audited: 2026-05-19

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | See current player HP as ten images and understand enemy, trap, and fall damage immediately. |
| Related systems | [UI And HUD](../Systems/UIAndHUD.md), [Combat](../Systems/Combat.md), [Traps And Hazards](../Systems/TrapsAndHazards.md) |

## Current Behavior

1. `Health` owns `maxHealth`, `currentHealth`, death state, and health-change notification.
2. `HUDManager` binds to the current player `Health` on start and after scene loads.
3. `HUDManager` finds `HUD_HP` from its serialized field or by searching under `HUD_Canvas`.
4. `HUDManager` reads child `Image` components under `HUD_HP` and sorts them by screen position.
5. HP starts visible from scene start.
6. HP loss disables HP image rendering from right to left.
7. Enemy hitboxes reduce HP through `Health.TakeDamage()`.
8. Trap damage windows reduce HP by the trap's configured `trapDamageAmount`, default `5`.
9. Fall zones call `Health.DepleteHealth()` so all HP images hide immediately before `Fall_Dive`.
10. Respawn scene reload restores player health and all HP images.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Core/Health.cs` | Health state and `OnHealthChanged` event. |
| `Assets/Scripts/Managers/HUDManager.cs` | HP image binding and display. |
| `Assets/Scripts/Interactables/SpikeTrapFSM.cs` | Configurable trap damage. |
| `Assets/Scripts/Interactables/FallZone.cs` | Instant HP depletion before fall sequence. |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | Contains `HUDManager`; `HUD_HP` should be assigned or discoverable by name. |

## Known Gaps

- `HUD_HP` wiring is expected to be completed in the Unity Inspector if automatic name lookup does not find the panel.
- HP presentation uses UGUI to match the current HUD stack.
