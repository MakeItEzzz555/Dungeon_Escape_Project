# Chest Interaction

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | Press `E` near a chest to open it and optionally reveal an item. |
| Related systems | [Interactables](../Systems/Interactables.md), [Audio](../Systems/Audio.md), [UI And HUD](../Systems/UIAndHUD.md) |

## Current Behavior

1. Player enters chest trigger range.
2. Player presses `E`.
3. Chest opens once.
4. Animator trigger `Open` fires.
5. Optional `itemInside` GameObject is enabled.
6. Interact SFX plays.
7. HUD shows `Chest Opened`.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Interactables/chest_yellow_blue.prefab` | Chest prefab. |
| `Assets/Scripts/Interactables/open_chest.cs` | Chest interaction script. |

## Known Gaps

- Uses `PlayInteract()` instead of the existing `AudioManager.PlayOpenChest()` wrapper.
- Chest reward behavior is limited to enabling an assigned GameObject.
