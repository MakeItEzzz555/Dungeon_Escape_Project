# HUD And Feedback

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | See coin/key progress and short feedback messages when interacting with objects. |
| Related system | [UI And HUD](../Systems/UIAndHUD.md) |

## Current Behavior

1. HUD starts hidden.
2. Collecting coins or keys reveals the HUD.
3. Coin and key sections are enabled only after discovered.
4. Coin count uses discovered scene coin count unless `manualMaxCoins` overrides it.
5. Key count is updated by `GlobalQuestManager`.
6. UX messages fade in, hold, and fade out.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Managers/HUDManager.cs` | HUD and feedback owner. |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | Gameplay HUD and pause menu canvas. |

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
