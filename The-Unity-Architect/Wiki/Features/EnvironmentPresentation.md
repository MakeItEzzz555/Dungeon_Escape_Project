# Environment Presentation

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player goal | See layered dungeon environments with correct sprite ordering, animated props, light rays, torches, waterfalls, and decorative objects. |
| Related system | [Environment And Presentation](../Systems/EnvironmentAndPresentation.md) |

## Current Behavior

1. Scene tilemaps and environment prefabs provide background/midground/foreground composition.
2. `YSort` adjusts sprite sorting order from world Y for characters, props, traps, gates, and collectibles.
3. `LightRaySequencedFX` alternates/fades two light-ray sprites with small drift/noise.
4. Imported animator controllers drive torches, waterfalls, gates, traps, coins, keys, and other animated props.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/YSort.cs` | Dynamic sprite sorting. |
| `Assets/Scripts/LightRaysPulse.cs` | Light ray sequence effect. |
| `Assets/Prefabs/Env Objects/` | Environment props. |
| `Assets/Animations/` | Animator controllers and animated tiles. |

Full non-deluxe animation inventory: [AnimationAssets.md](../Systems/AnimationAssets.md).

## Known Gaps

- Large imported asset folders are present; package audit tooling currently expects a missing `Assets/Hovl Studio` folder.
- Presentation effects are Update-driven and should be profiled if duplicated heavily.
