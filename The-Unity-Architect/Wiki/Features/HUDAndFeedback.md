# HUD And Feedback

Last audited: 2026-05-19

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | See current HP, coin/key progress, and short feedback messages when interacting with objects. |
| Related system | [UI And HUD](../Systems/UIAndHUD.md) |

## Current Behavior

1. HP images under `HUD_HP` are visible from scene start.
2. Player `Health` changes hide or show HP images through `HUDManager`.
3. HP loss hides images from right to left.
4. Coin/key HUD sections start hidden.
5. Collecting coins or keys reveals the HUD.
6. Coin and key sections are enabled only after discovered.
7. Coin count uses discovered scene coin count unless `manualMaxCoins` overrides it.
8. Key count is updated by `GlobalQuestManager`.
9. UX messages fade in, hold, and fade out.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Managers/HUDManager.cs` | HUD and feedback owner. |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | Gameplay HUD and pause menu canvas. |
| `HUD_HP` | HP image panel under `HUD_Canvas`. |

## Current Feedback Messages

- `Coin Collected`
- `Key Collected`
- `Requires Key`
- `Gate Opened`
- `Gate Closed`
- `Chest Opened`

## Known Gaps

- Gameplay systems call `HUDManager` directly.
- HUD uses UGUI, while Architect standards prefer UI Toolkit for future UI work.
